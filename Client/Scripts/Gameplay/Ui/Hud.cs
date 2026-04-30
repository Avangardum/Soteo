using System.Collections.Immutable;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Ui;

public sealed class Hud : Control, IHud
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/Hud.tscn"); 
    
    private readonly TextureProgress _healthBar;
    private readonly TextureProgress _manaBar;
    private readonly Label _healthLabel;
    private readonly Label _manaLabel;
    private readonly ImmutableList<AbilityButton> _abilityButtons;
    private readonly ImmutableList<TextureProgress> _statusIndicators;
    
    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentUserIdRepository _currentUserIdRepository;
    private readonly IPalette _palette;
    
    public Unit? SelectedUnit { get; set; }

    public Hud(IEntityLocator entityLocator, ICurrentUserIdRepository currentUserIdRepository, IPalette palette)
    {
        _entityLocator = entityLocator;
        _currentUserIdRepository = currentUserIdRepository;
        _palette = palette;
        
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
            .Cast<TextureProgress>()
            .ToImmutableList();

        for (int i = 0; i < _abilityButtons.Count; i++)
        {
            _abilityButtons[i].Connect("button_down", this, nameof(OnButtonDown), [i]);
            _abilityButtons[i].Connect("button_up", this, nameof(OnButtonUp), [i]);
        }
    }

    public void OnButtonDown(int buttonIndex)
    {
        Input.ParseInputEvent(new InputEventAction{ Action = "use_ability_class" + buttonIndex, Pressed = true });
    }
    
    public void OnButtonUp(int buttonIndex)
    {
        Input.ParseInputEvent(new InputEventAction{ Action = "use_ability_class" + buttonIndex, Pressed = false });
    }

    public override void _Process(float delta)
    {
        SelectedUnit ??= _entityLocator.FindEntity<PlayerCharacter>(_currentUserIdRepository.UserId, out _);
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
        _healthLabel.Text = $"{Mathf.CeilToInt(SelectedUnit.Stats[Stat.CurrentHealth])} / " +
            $"{Mathf.CeilToInt(SelectedUnit.Stats[Stat.MaxHealth])}";
        _manaBar.Value = SelectedUnit.Stats[Stat.CurrentMana];
        _manaBar.MaxValue = SelectedUnit.Stats[Stat.MaxMana];
        _manaLabel.Text = $"{Mathf.CeilToInt(SelectedUnit.Stats[Stat.CurrentMana])} / " +
        $"  {Mathf.CeilToInt(SelectedUnit.Stats[Stat.MaxMana])}";
    }
    
    private void ProcessAbilities()
    {
        for (var slot = AbilitySlot.Class0; slot <= AbilitySlot.ClassLast; slot++)
        {
            AbilityButton button = _abilityButtons[slot - AbilitySlot.Class0];
            if (!SelectedUnit!.AbilityStates.ContainsKey(slot))
            {
                button.Visible = false;
                continue;
            }
            
            button.Visible = true;
            AbilityState state = SelectedUnit.AbilityStates[slot];
            AbilityContext context = SelectedUnit.GetAbilityContext(new UseAbilityCommand(slot));
            
            button.CooldownIndicator.Value = state.Cooldown;
            button.CooldownIndicator.MaxValue = state.MaxCooldown == 0 ? 1 : state.MaxCooldown;

            button.UseProgressIndicator.Value = SelectedUnit.AbilityUseProgress?.Slot != slot ? 0 :
                SelectedUnit.AbilityUseProgress.NormalizedProgress;
            button.UseProgressIndicator.MaxValue = 1;
            
            button.HealthCostLabel.Text = Mathf.CeilToInt(state.Ability.HealthCost(context)).ToString();
            button.HealthCostLabel.Visible = state.Ability.HealthCost(context) > 0;
            
            button.ManaCostLabel.Text = Mathf.CeilToInt(state.Ability.ManaCost(context)).ToString();
            button.ManaCostLabel.Visible = state.Ability.ManaCost(context) > 0;
        }
    }
    
    private void ProcessStatuses(Unit unit)
    {
        List<StatusContext> contexts = unit.Statuses.Values.Take(_statusIndicators.Count).ToList(); // todo order
        int i = 0;
        for (; i < contexts.Count; i++)
        {
            _statusIndicators[i].Visible = true;
            _statusIndicators[i].Value = contexts[i].DisplayNormalizedRemainingTime;
        }
        for (; i < _statusIndicators.Count; i++)
        {
            _statusIndicators[i].Visible = false;
        }
    }
}