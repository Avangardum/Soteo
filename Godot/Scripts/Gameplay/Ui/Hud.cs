using System.Collections.Immutable;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Util;
using Soteo.Util.Extensions;

namespace Soteo.Gameplay.Ui;

public sealed class Hud : IHud
{
    private readonly HudNode _node;
    private readonly TextureProgress _healthBar;
    private readonly TextureProgress _manaBar;
    private readonly Label _healthLabel;
    private readonly Label _manaLabel;
    private readonly ImmutableList<AbilityButton> _abilityButtons;
    private readonly ImmutableList<StatusIndicator> _statusIndicators;
    
    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentCharacterIdRepository _currentCharIdRepository;
    private readonly IPalette _palette;
    private readonly ITooltip _tooltip;
    private readonly ILocalizer _localizer;
    
    public UnitPuppet? SelectedUnit { get; set; }

    public Hud
    (
        HudNode node,
        IEntityLocator entityLocator,
        ICurrentCharacterIdRepository currentCharIdRepository,
        IPalette palette,
        ITooltip tooltip,
        ILocalizer localizer
    )
    {
        _entityLocator = entityLocator;
        _currentCharIdRepository = currentCharIdRepository;
        _palette = palette;
        _tooltip = tooltip;
        _localizer = localizer;

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

        for (int i = 0; i < _abilityButtons.Count; i++)
        {
            int index = i;
            _abilityButtons[i].Connect("button_down", () => OnAbilityButtonDown(index));
            _abilityButtons[i].Connect("button_up", () => OnAbilityButtonUp(index));
            _abilityButtons[i].Connect("mouse_entered", () => OnMouseEnteredAbilityButton(index));
            _abilityButtons[i].Connect("mouse_exited", OnMouseExitedTooltipableControl);
        }
        
        for (int i = 0; i < _statusIndicators.Count; i++)
        {
            int index = i;
            _statusIndicators[i].Connect("mouse_entered", () => OnMouseEnteredStatusIndicator(index));
            _statusIndicators[i].Connect("mouse_exited", OnMouseExitedTooltipableControl);
        }
    }

    public void OnAbilityButtonDown(int buttonIndex)
    {
        Input.ParseInputEvent(new InputEventAction{ Action = "use_ability_class" + buttonIndex, Pressed = true });
    }
    
    public void OnAbilityButtonUp(int buttonIndex)
    {
        Input.ParseInputEvent(new InputEventAction{ Action = "use_ability_class" + buttonIndex, Pressed = false });
    }
    
    public void OnMouseEnteredAbilityButton(int buttonIndex)
    {
        if (SelectedUnit == null) return;
        AbilitySlot slot = AbilitySlot.Class0 + (byte)buttonIndex;
        if (!SelectedUnit.AbilitySlotStates.TryGetValue(slot, out AbilitySlotState? state)) return;
        
        Vector2 position = _abilityButtons[buttonIndex].RectGlobalPosition.ToSys() +
            new Vector2(_abilityButtons[buttonIndex].RectSize.x / 2, 0);
        string header = state.Ability.Name;
        string body = state.Ability.Description(_localizer, state.Level);
        _tooltip.Show(position, header, body);
    }
    
    public void OnMouseEnteredStatusIndicator(int indicatorIndex)
    {
        if (SelectedUnit == null) return;
        ImmutableList<PuppetStatusContext> contexts = GetVisibleStatusContexts(SelectedUnit);
        if (indicatorIndex >= contexts.Count) return;
        Status status = contexts[indicatorIndex].Status;
        
        Vector2 position = _statusIndicators[indicatorIndex].RectGlobalPosition.ToSys() +
            new Vector2(_statusIndicators[indicatorIndex].RectSize.x / 2, 0);
        string header = "";
        string body = status.Description(_localizer);
        _tooltip.Show(position, header, body);
    }
    
    public void OnMouseExitedTooltipableControl()
    {
        _tooltip.Hide();
    }

    public void Process(double delta)
    {
        if (_currentCharIdRepository.Value != null)
            SelectedUnit ??= _entityLocator.FindEntity<UnitPuppet>(_currentCharIdRepository.Required, out _);
        if (SelectedUnit == null)
        {
            _node.Visible = false;
            return;
        }

        _node.Visible = true;
        ProcessBars(SelectedUnit);
        ProcessAbilities(SelectedUnit);
        ProcessStatuses(SelectedUnit);
    }
    
    private void ProcessBars(UnitPuppet unit)
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
    
    private void ProcessAbilities(UnitPuppet unit)
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
    
    private void ProcessStatuses(UnitPuppet unit)
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
    
    private ImmutableList<PuppetStatusContext> GetVisibleStatusContexts(UnitPuppet unit)
    {
        return unit.Statuses.Values
            .Where(it => it.Status.HudVisible)
            .OrderBy(it => it.Ordinal)
            .Take(_statusIndicators.Count)
            .ToImmutableList();
    }
}
