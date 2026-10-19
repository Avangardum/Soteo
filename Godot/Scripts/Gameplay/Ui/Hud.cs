using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Commands;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
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
    private readonly HudNode _node;
    private readonly TextureProgress _healthBar;
    private readonly TextureProgress _manaBar;
    private readonly Label _healthLabel;
    private readonly Label _manaLabel;
    private readonly ImmutableList<AbilityButton> _abilityButtons;
    private readonly ImmutableList<StatusIndicator> _statusIndicators;
    private readonly GridContainer _inventoryGrid;
    private readonly ImmutableList<InventorySlot> _inventorySlots;

    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentCharacterIdRepository _currentCharIdRepo;
    private readonly IPalette _palette;
    private readonly ITooltip _tooltip;
    private readonly ILocalizer _localizer;
    private readonly IFromGameplayPacketSender _packetSender;
    private readonly ISynchronizedCampaignStatePuppetRepository _syncCampaignStateRepo;
    private readonly IVisibleShardIdRepository _visibleShardIdRepo;
    private readonly ICurrentUserIdRepository _currentUserIdRepo;

    private bool _isDragging;
    private bool _isCancelingItemStackDrag;

    public IUnitPuppet? SelectedUnit
    {
        get;
        set
        {
            if (value == field) return;
            field = value;

            bool isControllable = value != null &&
                _currentUserIdRepo.Value != null &&
                value.ControllingPlayerIds.Contains(_currentUserIdRepo.Value.Value);
            _isCancelingItemStackDrag = true;
            foreach (InventorySlot slot in _inventorySlots)
            {
                slot.ItemStack.StopDrag();
                slot.ItemStack.IsDraggable = isControllable;
            }
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
        ICurrentUserIdRepository currentUserIdRepo
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
            .Cast<StatusIndicator>()
            .ToImmutableList();
        _inventoryGrid = node.GetNode<GridContainer>("Inventory/MarginContainer/Slots");
        _inventorySlots = _inventoryGrid.GetChildren()
            .Cast<InventorySlot>()
            .ToImmutableList();

        SubscribeToAbilityButtonEvents();
        SubscribeToStatusIndicatorEvents();
        SubscribeToInventorySlotEvents();
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
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            int index = i;
            _inventorySlots[i].Connect("mouse_entered", () => OnMouseEnteredInventorySlot(index));
            _inventorySlots[i].Connect("mouse_exited", OnMouseExitedTooltipableControl);
            _inventorySlots[i].DragStarted += () => OnInventoryItemStackDragStarted(index);
            _inventorySlots[i].DragStopped += position => OnInventoryItemStackDragStopped(index, position);
        }
    }

    private void OnAbilityButtonDown(int buttonIndex)
    {
        if (SelectedUnit != null && SelectedUnit.Id == _currentCharIdRepo.Value)
            Input.ParseInputEvent(new InputEventAction{ Action = "use_ability_class" + buttonIndex, Pressed = true });
    }

    private void OnAbilityButtonUp(int buttonIndex)
    {
        if (SelectedUnit != null && SelectedUnit.Id == _currentCharIdRepo.Value)
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
        if (index >= SelectedUnit.Inventory.Count) return;

        Item? item = SelectedUnit.Inventory[index]?.Item;
        if (item == null) return;

        _tooltip.Show(_inventorySlots[index].TopCenterGlobalPosition.ToSys(), item.Name, "");
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

    private void OnInventoryItemStackDragStopped(int index, GdVector2 position)
    {
        _isDragging = false;

        if (_isCancelingItemStackDrag) return;
        if (SelectedUnit == null) return;
        if (_currentUserIdRepo.Value == null) return;
        if (!SelectedUnit.ControllingPlayerIds.Contains(_currentUserIdRepo.Value.Value)) return;
        if (_visibleShardIdRepo.Value == null) return;

        for (int i = 0; i < _inventorySlots.Count && _inventorySlots[i].Visible; i++)
        {
            if
            (
                IsInsideRect(_node.GetGlobalMousePosition(), _inventorySlots[i].DropArea) ||
                IsInsideRect(position, _inventorySlots[i].DropArea)
            )
            {
                MoveItemStack(index, i, _visibleShardIdRepo.Value.Value, SelectedUnit);
                break;
            }
        }
    }

    private bool IsInsideRect(GdVector2 position, Control control)
    {
        return position.x >= control.RectGlobalPosition.x &&
            position.x <= control.RectGlobalPosition.x + control.RectSize.x &&
            position.y >= control.RectGlobalPosition.y &&
            position.y <= control.RectGlobalPosition.y + control.RectSize.y;
    }

    private void MoveItemStack(int fromIndex, int toIndex, Guid shardId, IUnitPuppet unitPuppet)
    {
        if (_syncCampaignStateRepo.Value.IsPaused) return;

        _packetSender.SendReliable
        (
            new MoveItemStackPacket
            {
                UnitId = _currentCharIdRepo.Value.Required,
                Command = new MoveItemStackCommand
                {
                    FromIndex = fromIndex,
                    ToIndex = toIndex
                }
            },
            shardId
        );

        if (fromIndex < unitPuppet.VisualInventory.Count && toIndex < unitPuppet.VisualInventory.Count)
        {
            unitPuppet.LockVisualInventory();
            ItemStack? stack = unitPuppet.VisualInventory.RemoveAt(fromIndex);
            if (stack != null)
            {
                ItemStack? putResult = unitPuppet.VisualInventory.PutAt(toIndex, stack);
                if (putResult != null)
                    unitPuppet.VisualInventory.PutAt(fromIndex, putResult);
            }
        }
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
    }

    [MemberNotNullWhen(true, nameof(SelectedUnit))]
    public bool TrySelectCurrentUnit()
    {
        if (_currentCharIdRepo.Value == null) return false;
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Required, out IUnitPuppet? unit, out _))
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
        _inventoryGrid.Columns = Maths.CeilToInt(unit.VisualInventory.Count / 2.0);

        for (int i = 0; i < unit.VisualInventory.Count; i++)
        {
            _inventorySlots[i].Visible = true;
            ItemStack? stack = unit.VisualInventory[i];
            _inventorySlots[i].TextureRect.Visible = stack != null;
            if (stack != null)
            {
                _inventorySlots[i].TextureRect.Texture = stack.Item.Icon;
                _inventorySlots[i].CountBackground.Visible = stack.Count > 1;
                if (stack.Count > 1)
                    _inventorySlots[i].CountLabel.Text = stack.Count.ToString();
            }
        }
        for (int i = unit.VisualInventory.Count; i < _inventorySlots.Count; i++)
        {
            _inventorySlots[i].Visible = false;
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
