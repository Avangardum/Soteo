using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Commands;
using Soteo.Core.Dto.Packets.Command;
using Soteo.Core.Entities;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Items;
using Soteo.Core.Statuses;
using Soteo.Main.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class Hud : IHud
{
    private static readonly Color BlockedSlotItemStackColor = Color.Color8(255, 255, 255, 100);

    private readonly HudNode _node;
    private readonly TextureProgress _healthBar;
    private readonly TextureProgress _manaBar;
    private readonly Label _healthLabel;
    private readonly Label _manaLabel;
    private readonly ImmutableList<AbilityButton> _abilityButtons;
    private readonly ImmutableList<StatusIndicator> _statusIndicators;
    private readonly GridContainer _inventoryGrid;
    private readonly ImmutableList<ItemStackSlotControl> _inventorySlotControls;
    private readonly Control _equipmentPanel;
    private readonly ImmutableDictionary<EquipmentSlot, ItemStackSlotControl> _equipmentSlotControls;

    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentCharacterIdRepository _currentCharIdRepo;
    private readonly IPalette _palette;
    private readonly ITooltip _tooltip;
    private readonly ILocalizer _localizer;
    private readonly IFromGameplayPacketSender _packetSender;
    private readonly ISynchronizedCampaignStatePuppetRepository _syncCampaignStateRepo;
    private readonly IVisibleShardIdRepository _visibleShardIdRepo;
    private readonly ICurrentUserIdRepository _currentUserIdRepo;
    private readonly ICrossInventoryHelper _crossInventoryHelper;

    private bool _isDragging;
    private bool _isCancelingItemStackDrag;

    public IUnitPuppet? SelectedUnit
    {
        get;
        set
        {
            if (value == field) return;
            field = value;

            _isCancelingItemStackDrag = true;
            foreach (ItemStackSlotControl slot in _inventorySlotControls.Concat(_equipmentSlotControls.Values))
                slot.ItemStack.StopDrag();
            _isCancelingItemStackDrag = false;
        }
    }

    public Hud
    (
        HudNode node,
        IEntityLocator entityLocator,
        ICurrentCharacterIdRepository currentCharIdRepo,
        IPalette palette,
        ITooltip tooltip,
        ILocalizer localizer,
        IFromGameplayPacketSender packetSender,
        ISynchronizedCampaignStatePuppetRepository syncCampaignStateRepo,
        IVisibleShardIdRepository visibleShardIdRepo,
        ICurrentUserIdRepository currentUserIdRepo,
        ICrossInventoryHelper crossInventoryHelper
    )
    {
        _entityLocator = entityLocator;
        _currentCharIdRepo = currentCharIdRepo;
        _palette = palette;
        _tooltip = tooltip;
        _localizer = localizer;
        _packetSender = packetSender;
        _syncCampaignStateRepo = syncCampaignStateRepo;
        _visibleShardIdRepo = visibleShardIdRepo;
        _currentUserIdRepo = currentUserIdRepo;
        _crossInventoryHelper = crossInventoryHelper;

        node.Hud = this;
        _node = node;
        _healthBar = node.GetNode<TextureProgress>("VBoxContainer/UnitPanel/VBoxContainer/Health");
        _manaBar = node.GetNode<TextureProgress>("VBoxContainer/UnitPanel/VBoxContainer/Mana");
        _healthLabel = _healthBar.GetNode<Label>("Label");
        _manaLabel = _manaBar.GetNode<Label>("Label");
        _abilityButtons = node.GetNode("VBoxContainer/UnitPanel/VBoxContainer/Abilities").GetChildren()
            .Cast<AbilityButton>()
            .ToImmutableList();
        _statusIndicators = node.GetNode("VBoxContainer/Statuses").GetChildren()
            .OfType<StatusIndicator>()
            .ToImmutableList();
        _inventoryGrid = node.GetNode<GridContainer>("Inventory/MarginContainer/Slots");
        _inventorySlotControls = _inventoryGrid.GetChildren()
            .Cast<ItemStackSlotControl>()
            .ToImmutableList();
        _equipmentPanel = node.GetNode<Control>("Equipment");
        _equipmentSlotControls = Enum.GetValues<EquipmentSlot>()
            .ToImmutableDictionary
            (
                it => it,
                it => _equipmentPanel.GetNode<ItemStackSlotControl>($"MarginContainer/Slots/{it}")
            );

        SubscribeToAbilityButtonEvents();
        SubscribeToStatusIndicatorEvents();
        SubscribeToInventorySlotEvents();
        SubscribeToEquipmentSlotEvents();
    }

    private void SubscribeToAbilityButtonEvents()
    {
        for (int i = 0; i < _abilityButtons.Count; i++)
        {
            int index = i;
            _abilityButtons[i].Connect("button_down", () => OnAbilityButtonDown(index));
            _abilityButtons[i].Connect("button_up", () => OnAbilityButtonUp(index));
            _abilityButtons[i].Connect("mouse_entered", () => OnMouseEnteredAbilityButton(index));
            _abilityButtons[i].Connect("mouse_exited", OnMouseExitedTooltipableControl);
        }
    }

    private void SubscribeToStatusIndicatorEvents()
    {
        for (int i = 0; i < _statusIndicators.Count; i++)
        {
            int index = i;
            _statusIndicators[i].Connect("mouse_entered", () => OnMouseEnteredStatusIndicator(index));
            _statusIndicators[i].Connect("mouse_exited", OnMouseExitedTooltipableControl);
        }
    }

    private void SubscribeToInventorySlotEvents()
    {
        for (int i = 0; i < _inventorySlotControls.Count; i++)
        {
            int index = i;
            _inventorySlotControls[i].Connect("mouse_entered", () => OnMouseEnteredInventorySlot(index));
            _inventorySlotControls[i].Connect("mouse_exited", OnMouseExitedTooltipableControl);
            _inventorySlotControls[i].ItemStack.DragStarted += () => OnInventoryItemStackDragStarted(index);
            _inventorySlotControls[i].ItemStack.DragStopped += () => OnInventoryItemStackDragStopped(index);
            _inventorySlotControls[i].ItemStack.SelectClicked += () => OnInventoryItemStackSelectClicked(index);
        }
    }

    private void SubscribeToEquipmentSlotEvents()
    {
        foreach ((EquipmentSlot slot, ItemStackSlotControl control) in _equipmentSlotControls)
        {
            control.Connect("mouse_entered", () => OnMouseEnteredEquipmentSlot(slot));
            control.Connect("mouse_exited", OnMouseExitedTooltipableControl);
            control.ItemStack.DragStarted += () => OnEquipmentItemStackDragStarted(slot);
            control.ItemStack.DragStopped += () => OnEquipmentItemStackDragStopped(slot);
            control.ItemStack.SelectClicked += () => OnEquipmentItemStackSelectClicked(slot);
        }
    }

    private void OnAbilityButtonDown(int buttonIndex)
    {
        if (SelectedUnit != null && SelectedUnit.Id == _currentCharIdRepo.Id)
            Input.ParseInputEvent(new InputEventAction{ Action = "use_ability_class" + buttonIndex, Pressed = true });
    }

    private void OnAbilityButtonUp(int buttonIndex)
    {
        if (SelectedUnit != null && SelectedUnit.Id == _currentCharIdRepo.Id)
            Input.ParseInputEvent(new InputEventAction{ Action = "use_ability_class" + buttonIndex, Pressed = false });
    }

    private void OnMouseEnteredAbilityButton(int index)
    {
        if (_isDragging) return;
        if (SelectedUnit == null) return;
        AbilitySlot slot = AbilitySlot.Class0 + index;
        if (!SelectedUnit.AbilitySlotStates.TryGetValue(slot, out AbilitySlotState? state)) return;

        string header = state.Ability.Name;
        string body = state.Ability.Description(_localizer, state.Level);
        _tooltip.Show(_abilityButtons[index].TopCenterGlobalPosition.ToSys(), header, body);
    }

    private void OnMouseEnteredStatusIndicator(int index)
    {
        if (_isDragging) return;
        if (SelectedUnit == null) return;
        ImmutableList<PuppetStatusContext> contexts = GetVisibleStatusContexts(SelectedUnit);
        if (index >= contexts.Count) return;
        Status status = contexts[index].Status;

        string header = "";
        string body = status.Description(_localizer);
        _tooltip.Show(_statusIndicators[index].TopCenterGlobalPosition.ToSys(), header, body);
    }

    private void OnMouseEnteredInventorySlot(int index)
    {
        if (_isDragging) return;
        if (SelectedUnit == null) return;
        if (!SelectedUnit.Inventory.HasSlot(index)) return;

        Item? item = SelectedUnit.Inventory[index]?.Item;
        if (item == null) return;

        _tooltip.Show(_inventorySlotControls[index].TopCenterGlobalPosition.ToSys(), item.Name, "");
    }

    private void OnMouseEnteredEquipmentSlot(EquipmentSlot slot)
    {
        if (_isDragging) return;
        if (SelectedUnit == null) return;
        if (!SelectedUnit.EquipmentInventory.TryGetValue(slot, out ItemStack? stack)) return;
        if (stack == null) return;

        _tooltip.Show(_equipmentSlotControls[slot].TopCenterGlobalPosition.ToSys(), stack.Item.Name, "");
    }

    public void OnMouseExitedTooltipableControl()
    {
        _tooltip.Hide();
    }

    private void OnInventoryItemStackDragStarted(int index)
    {
        _isDragging = true;
        _tooltip.Hide();
    }

    private void OnInventoryItemStackDragStopped(int index)
    {
        _isDragging = false;

        if (!CanControlSelectedUnit(out Guid? shardId)) return;

        GdVector2 mousePos = _node.GetGlobalMousePosition();

        for (int i = 0; i < _inventorySlotControls.Count && _inventorySlotControls[i].Visible; i++)
        {
            if (IsInsideRect(mousePos, _inventorySlotControls[i]))
            {
                MoveInventoryItemStack(index, i, shardId.Value, SelectedUnit);
                return;
            }
        }

        foreach ((EquipmentSlot slot, ItemStackSlotControl control) in _equipmentSlotControls)
        {
            if (IsInsideRect(mousePos, control))
            {
                if (control.Visible)
                    Equip(index, slot, shardId.Value, SelectedUnit);
                return;
            }
        }
    }

    [MemberNotNullWhen(true, nameof(SelectedUnit))]
    private bool CanControlSelectedUnit([NotNullWhen(true)] out Guid? shardId)
    {
        if
        (
            _syncCampaignStateRepo.Value.IsPaused ||
            _isCancelingItemStackDrag ||
            SelectedUnit == null ||
            _currentUserIdRepo.Id == null ||
            !SelectedUnit.ControllingPlayerIds.Contains(_currentUserIdRepo.Id.Value) ||
            _visibleShardIdRepo.Id == null
        )
        {
            shardId = null;
            return false;
        }

        shardId = _visibleShardIdRepo.Id;
        return true;
    }

    private void OnEquipmentItemStackDragStarted(EquipmentSlot slot)
    {
        _isDragging = true;
        _tooltip.Hide();
    }

    private void OnEquipmentItemStackDragStopped(EquipmentSlot slot)
    {
        _isDragging = false;

        if (!CanControlSelectedUnit(out Guid? shardId)) return;

        GdVector2 mousePos = _node.GetGlobalMousePosition();

        for (int i = 0; i < _inventorySlotControls.Count && _inventorySlotControls[i].Visible; i++)
        {
            if (IsInsideRect(mousePos, _inventorySlotControls[i]))
            {
                Unequip(slot, i, shardId.Value, SelectedUnit);
                return;
            }
        }
    }

    private void OnInventoryItemStackSelectClicked(int slot)
    {
        if (!CanControlSelectedUnit(out Guid? shardId)) return;
        Equip(slot, toEquipmentSlot: null, shardId.Value, SelectedUnit);
        _tooltip.Hide();
    }

    private void OnEquipmentItemStackSelectClicked(EquipmentSlot slot)
    {
        if (!CanControlSelectedUnit(out Guid? shardId)) return;
        Unequip(slot, toInventorySlot: null, shardId.Value, SelectedUnit);
        _tooltip.Hide();
    }

    private bool IsInsideRect(GdVector2 position, Control control)
    {
        return position.x >= control.RectGlobalPosition.x &&
            position.x <= control.RectGlobalPosition.x + control.RectSize.x &&
            position.y >= control.RectGlobalPosition.y &&
            position.y <= control.RectGlobalPosition.y + control.RectSize.y;
    }

    private void MoveInventoryItemStack(int fromIndex, int toIndex, Guid shardId, IUnitPuppet unit)
    {
        if (!unit.Inventory.HasSlot(fromIndex)) return;
        if (!unit.Inventory.HasSlot(toIndex)) return;

        _packetSender.SendReliable
        (
            new MoveItemStackPacket
            {
                UnitId = unit.Id,
                Command = new MoveItemStackCommand
                {
                    FromIndex = fromIndex,
                    ToIndex = toIndex
                }
            },
            shardId
        );

        unit.DesyncInventory();
        ItemStack? stack = unit.Inventory.RemoveAt(fromIndex);
        if (stack != null)
        {
            ItemStack? remainder = unit.Inventory.PutAt(toIndex, stack);
            if (remainder != null)
                unit.Inventory.PutAt(fromIndex, remainder);
        }
    }

    private void Equip(int fromInventorySlot, EquipmentSlot? toEquipmentSlot, Guid shardId, IUnitPuppet unit)
    {
        if (!_crossInventoryHelper.Equip(unit.Inventory, unit.EquipmentInventory, fromInventorySlot, toEquipmentSlot))
            return;
        unit.DesyncInventory();
        _packetSender.SendReliable(new EquipPacket
        {
            UnitId = unit.Id,
            Command = new EquipCommand(fromInventorySlot)
        }, shardId);
    }

    private void Unequip(EquipmentSlot fromEquipmentSlot, int? toInventorySlot, Guid shardId, IUnitPuppet unit)
    {
        if (!_crossInventoryHelper.Unequip(unit.Inventory, unit.EquipmentInventory, fromEquipmentSlot, toInventorySlot))
            return;
        unit.DesyncInventory();
        _packetSender.SendReliable(new UnequipPacket
        {
            UnitId = unit.Id,
            Command = new UnequipCommand(fromEquipmentSlot, toInventorySlot)
        }, shardId);
    }

    public void Process(double delta)
    {
        if (SelectedUnit == null && !TrySelectCurrentUnit())
        {
            _node.Visible = false;
            return;
        }

        _node.Visible = true;
        ProcessBars(SelectedUnit);
        ProcessAbilities(SelectedUnit);
        ProcessStatuses(SelectedUnit);
        ProcessInventory(SelectedUnit);
        ProcessEquipment(SelectedUnit);
    }

    [MemberNotNullWhen(true, nameof(SelectedUnit))]
    public bool TrySelectCurrentUnit()
    {
        if (_currentCharIdRepo.Id == null) return false;
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Id.Required, out IUnitPuppet? unit, out _))
            return false;

        SelectedUnit = unit;
        return true;
    }

    private void ProcessBars(IUnitPuppet unit)
    {
        _healthBar.TintProgress = _palette.FactionColor(unit.Faction);

        _healthBar.Value = unit.Stats[Stat.CurrentHealth];
        _healthBar.MaxValue = unit.Stats[Stat.MaxHealth];
        _healthLabel.Text = $"{Maths.CeilToInt(unit.Stats[Stat.CurrentHealth])} / " +
             $"{Maths.CeilToInt(unit.Stats[Stat.MaxHealth])}";
        _manaBar.Value = unit.Stats[Stat.CurrentMana];
        _manaBar.MaxValue = unit.Stats[Stat.MaxMana];
        _manaLabel.Text = $"{Maths.CeilToInt(unit.Stats[Stat.CurrentMana])} / " +
            $"{Maths.CeilToInt(unit.Stats[Stat.MaxMana])}";
    }

    private void ProcessAbilities(IUnitPuppet unit)
    {
        for (var slot = AbilitySlot.Class0; slot <= AbilitySlot.ClassLast; slot++)
        {
            AbilityButton button = _abilityButtons[slot - AbilitySlot.Class0];
            if (!unit.AbilitySlotStates.TryGetValue(slot, out AbilitySlotState state))
            {
                button.Visible = false;
                continue;
            }

            button.Visible = true;
            button.IconRect.Texture = state.Ability.Icon;

            button.CooldownIndicator.Value = state.Cooldown;
            button.CooldownIndicator.MaxValue = state.MaxCooldown == 0 ? 1 : state.MaxCooldown;

            button.UseProgressIndicator.Value = unit.AbilityUseProgress?.Slot != slot ? 0 :
                unit.AbilityUseProgress.NormalizedProgress;
            button.UseProgressIndicator.MaxValue = 1;

            double healthCost = state.Ability.StaticHealthCost[state.Level];
            button.HealthCostLabel.Text = Maths.CeilToInt(healthCost).ToString();
            button.HealthCostLabel.Visible = healthCost > 0;

            double manaCost = state.Ability.StaticManaCost[state.Level];
            button.ManaCostLabel.Text = Maths.CeilToInt(manaCost).ToString();
            button.ManaCostLabel.Visible = manaCost > 0;
        }
    }

    private void ProcessStatuses(IUnitPuppet unit)
    {
        ImmutableList<PuppetStatusContext> contexts = GetVisibleStatusContexts(unit);
        for (int i = 0; i < contexts.Count; i++)
        {
            _statusIndicators[i].Visible = true;
            _statusIndicators[i].IconRect.Texture = contexts[i].Status.ResolveIcon(contexts[i]);
            _statusIndicators[i].Value = contexts[i].DisplayNormalizedRemainingTime;
        }
        for (int i = contexts.Count; i < _statusIndicators.Count; i++)
        {
            _statusIndicators[i].Visible = false;
        }
    }

    private void ProcessInventory(IUnitPuppet unit)
    {
        _inventoryGrid.Columns = Maths.CeilToInt(unit.Inventory.Count / 2.0);
        bool canControlSelectedUnit = CanControlSelectedUnit(out _);

        for (int i = 0; i < unit.Inventory.Count; i++)
        {
            _inventorySlotControls[i].Visible = true;
            ItemStack? stack = unit.Inventory[i];
            _inventorySlotControls[i].ItemStack.Visible = stack != null;
            if (stack != null)
            {
                _inventorySlotControls[i].ItemStack.TextureRect.Texture = stack.Item.Icon;
                _inventorySlotControls[i].ItemStack.CountBackground.Visible = stack.Count > 1;
                _inventorySlotControls[i].ItemStack.IsInteractive = canControlSelectedUnit;
                if (stack.Count > 1)
                    _inventorySlotControls[i].ItemStack.CountLabel.Text = stack.Count.ToString();
            }
        }
        for (int i = unit.Inventory.Count; i < _inventorySlotControls.Count; i++)
        {
            _inventorySlotControls[i].Visible = false;
        }
    }

    private void ProcessEquipment(IUnitPuppet unit)
    {
        if (!unit.EquipmentInventory.Any())
        {
            _equipmentPanel.Visible = false;
            return;
        }

        _equipmentPanel.Visible = true;
        IReadOnlyDictionary<EquipmentSlot, EquipmentSlot> blockedAndBlockingSlots =
            unit.EquipmentInventory.BlockedAndBlockingSlots;
        bool canControlSelectedUnit = CanControlSelectedUnit(out _);

        foreach ((EquipmentSlot slot, ItemStackSlotControl control) in _equipmentSlotControls)
        {
            if (!unit.EquipmentInventory.TryGetValue(slot, out ItemStack? stack))
            {
                control.Visible = false;
                continue;
            }

            control.Visible = true;

            if (blockedAndBlockingSlots.TryGetValue(slot, out EquipmentSlot blockingSlot))
            {
                control.ItemStack.Visible = true;
                control.ItemStack.Modulate = BlockedSlotItemStackColor;
                control.ItemStack.TextureRect.Texture = unit.EquipmentInventory[blockingSlot].Required.Item.Icon;
                control.ItemStack.CountBackground.Visible = false;
                control.ItemStack.IsInteractive = false;
                continue;
            }

            control.ItemStack.Modulate = Colors.White;
            control.ItemStack.IsInteractive = canControlSelectedUnit;

            if (stack == null)
            {
                control.ItemStack.Visible = false;
                continue;
            }

            control.ItemStack.Visible = true;
            control.ItemStack.TextureRect.Texture = stack.Item.Icon;

            if (stack.Count == 1)
            {
                control.ItemStack.CountBackground.Visible = false;
                continue;
            }

            control.ItemStack.CountLabel.Text = stack.Count.ToString();
        }
    }

    private ImmutableList<PuppetStatusContext> GetVisibleStatusContexts(IUnitPuppet unit)
    {
        return unit.Statuses.Values
            .Where(it => it.Status.HudVisible)
            .OrderBy(it => it.Ordinal)
            .Take(_statusIndicators.Count)
            .ToImmutableList();
    }
}
