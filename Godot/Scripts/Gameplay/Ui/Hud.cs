using System.Collections.Immutable;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Statuses;
using Soteo.Shared;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Ui;

public sealed class Hud : Control, IHud
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/Hud.tscn"); 
    
    private readonly TextureProgress _healthBar;
    private readonly TextureProgress _manaBar;
    private readonly Label _healthLabel;
    private readonly Label _manaLabel;
    private readonly ImmutableList<AbilityButton> _abilityButtons;
    private readonly ImmutableList<StatusIndicator> _statusIndicators;
    
    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentUserIdRepository _currentUserIdRepository;
    private readonly IPalette _palette;
    private readonly ITooltip _tooltip;
    private readonly ILocalizer _localizer;
    
    public UnitPuppet? SelectedUnit { get; set; }

    public Hud
    (
        IEntityLocator entityLocator,
        ICurrentUserIdRepository currentUserIdRepository,
        IPalette palette,
        ITooltip tooltip,
        ILocalizer localizer
    )
    {
        _entityLocator = entityLocator;
        _currentUserIdRepository = currentUserIdRepository;
        _palette = palette;
        _tooltip = tooltip;
        _localizer = localizer;
        
        Name = nameof(Hud);
        AnchorBottom = 1;
        AnchorRight = 1;
        MouseFilter = MouseFilterEnum.Ignore;
        
        Scene.InstanceAndReparentTo(this);
        _healthBar = GetNode<TextureProgress>("VBoxContainer/UnitPanel/VBoxContainer/Health");
        _manaBar = GetNode<TextureProgress>("VBoxContainer/UnitPanel/VBoxContainer/Mana");
        _healthLabel = _healthBar.GetNode<Label>("Label");
        _manaLabel = _manaBar.GetNode<Label>("Label");
        _abilityButtons = GetNode("VBoxContainer/UnitPanel/VBoxContainer/Abilities").GetChildren()
            .Cast<AbilityButton>()
            .ToImmutableList();
        _statusIndicators = GetNode("VBoxContainer/Statuses").GetChildren()
            .Cast<StatusIndicator>()
            .ToImmutableList();

        for (int i = 0; i < _abilityButtons.Count; i++)
        {
            _abilityButtons[i].Connect("button_down", this, nameof(OnAbilityButtonDown), [i]);
            _abilityButtons[i].Connect("button_up", this, nameof(OnAbilityButtonUp), [i]);
            _abilityButtons[i].Connect("mouse_entered", this, nameof(OnMouseEnteredAbilityButton), [i]);
            _abilityButtons[i].Connect("mouse_exited", this, nameof(OnMouseExitedTooltipableControl));
        }
        
        for (int i = 0; i < _statusIndicators.Count; i++)
        {
            _statusIndicators[i].Connect("mouse_entered", this, nameof(OnMouseEnteredStatusIndicator), [i]);
            _statusIndicators[i].Connect("mouse_exited", this, nameof(OnMouseExitedTooltipableControl));
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
        
        GdVector2 position = _abilityButtons[buttonIndex].RectGlobalPosition +
                             new GdVector2(_abilityButtons[buttonIndex].RectSize.x / 2, 0);
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
        
        GdVector2 position = _statusIndicators[indicatorIndex].RectGlobalPosition +
                             new GdVector2(_statusIndicators[indicatorIndex].RectSize.x / 2, 0);
        string header = "";
        string body = status.Description(_localizer);
        _tooltip.Show(position, header, body);
    }
    
    public void OnMouseExitedTooltipableControl()
    {
        _tooltip.Hide();
    }

    public override void _Process(float delta)
    {
        SelectedUnit ??= _entityLocator.FindEntity<UnitPuppet>(_currentUserIdRepository.UserId, out _);
        if (SelectedUnit == null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        ProcessBars();
        ProcessAbilities();
        ProcessStatuses(SelectedUnit);
    }
    
    private void ProcessBars()
    {
        _healthBar.TintProgress = SelectedUnit.Required.Faction switch
        {
            Faction.Empire => _palette.Empire,
            Faction.Syndicate => _palette.Syndicate,
            _ => _palette.Neutral
        };
        
        _healthBar.Value = SelectedUnit.Stats[Stat.CurrentHealth];
        _healthBar.MaxValue = SelectedUnit.Stats[Stat.MaxHealth];
        _healthLabel.Text = $"{Maths.CeilToInt(SelectedUnit.Stats[Stat.CurrentHealth])} / " +
             $"{Maths.CeilToInt(SelectedUnit.Stats[Stat.MaxHealth])}";
        _manaBar.Value = SelectedUnit.Stats[Stat.CurrentMana];
        _manaBar.MaxValue = SelectedUnit.Stats[Stat.MaxMana];
        _manaLabel.Text = $"{Maths.CeilToInt(SelectedUnit.Stats[Stat.CurrentMana])} / " +
            $"{Maths.CeilToInt(SelectedUnit.Stats[Stat.MaxMana])}";
    }
    
    private void ProcessAbilities()
    {
        for (var slot = AbilitySlot.Class0; slot <= AbilitySlot.ClassLast; slot++)
        {
            AbilityButton button = _abilityButtons[slot - AbilitySlot.Class0];
            if (!SelectedUnit.Required.AbilitySlotStates.ContainsKey(slot))
            {
                button.Visible = false;
                continue;
            }
            
            button.Visible = true;
            AbilitySlotState state = SelectedUnit.AbilitySlotStates[slot];
            
            button.IconRect.Texture = state.Ability.Icon;
            
            button.CooldownIndicator.Value = state.Cooldown;
            button.CooldownIndicator.MaxValue = state.MaxCooldown == 0 ? 1 : state.MaxCooldown;

            button.UseProgressIndicator.Value = SelectedUnit.AbilityUseProgress?.Slot != slot ? 0 :
                SelectedUnit.AbilityUseProgress.NormalizedProgress;
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
        int i = 0;
        for (; i < contexts.Count; i++)
        {
            _statusIndicators[i].Visible = true;
            _statusIndicators[i].IconRect.Texture = contexts[i].Status.ResolveIcon(contexts[i]);
            _statusIndicators[i].Value = contexts[i].DisplayNormalizedRemainingTime;
        }
        for (; i < _statusIndicators.Count; i++)
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