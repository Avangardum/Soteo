using System.Collections.Immutable;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class Hud : Control, IHud
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/Hud.tscn"); 
    
    private readonly TextureProgress _healthBar;
    private readonly TextureProgress _manaBar;
    private readonly Label _healthLabel;
    private readonly Label _manaLabel;
    private readonly ImmutableList<AbilityButton> _abilityButtons;
    
    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentUserIdRepository _currentUserIdRepository;
    private readonly IPalette _palette;
    
    public Unit? SelectedUnit { get; set; }

    public Hud(IEntityLocator entityLocator, ICurrentUserIdRepository currentUserIdRepository, IPalette palette)
    {
        // todo fix editor crashes
        _entityLocator = entityLocator;
        _currentUserIdRepository = currentUserIdRepository;
        _palette = palette;
        
        Name = nameof(Hud);
        AnchorBottom = 1;
        AnchorRight = 1;
        MouseFilter = MouseFilterEnum.Ignore;
        
        Scene.InstanceAndReparentTo(this);
        _healthBar = GetNode<TextureProgress>("UnitPanel/VBoxContainer/Health");
        _manaBar = GetNode<TextureProgress>("UnitPanel/VBoxContainer/Mana");
        _healthLabel = _healthBar.GetNode<Label>("Label");
        _manaLabel = _manaBar.GetNode<Label>("Label");
        _abilityButtons =
            GetNode("UnitPanel/VBoxContainer/Abilities").GetChildren().Cast<AbilityButton>().ToImmutableList();

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
        if (SelectedUnit == null || !IsInstanceValid(SelectedUnit))
        {
            SelectedUnit = _entityLocator.FindEntity<PlayerCharacter>(_currentUserIdRepository.UserId, out _);
        }
        if (SelectedUnit == null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        ProcessBars();
        ProcessAbilities();
    }
    
    private void ProcessBars()
    {
        _healthBar.TintProgress = SelectedUnit!.Faction switch
        {
            Faction.Neutral => _palette.Neutral,
            Faction.Empire => _palette.Empire,
            Faction.Syndicate => _palette.Syndicate
        };
        
        _healthBar.Value = SelectedUnit!.Stats[Stat.CurrentHealth];
        _healthBar.MaxValue = SelectedUnit.Stats[Stat.MaxHealth];
        _healthLabel.Text = $"{SelectedUnit.Stats[Stat.CurrentHealth]} / {SelectedUnit.Stats[Stat.MaxHealth]}";
        _manaBar.Value = SelectedUnit.Stats[Stat.CurrentMana];
        _manaBar.MaxValue = SelectedUnit.Stats[Stat.MaxMana];
        _manaLabel.Text = $"{SelectedUnit.Stats[Stat.CurrentMana]} / {SelectedUnit.Stats[Stat.MaxMana]}";
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
            IReadOnlyAbilityState state = SelectedUnit.AbilityStates[slot];
            AbilityContext context = SelectedUnit.GetAbilityUseContext(new UseAbilityCommand(slot));
            
            button.CooldownIndicator.Value = state.Cooldown;
            button.CooldownIndicator.MaxValue = state.Ability.Cooldown(context);

            button.UseProgressIndicator.Value = SelectedUnit.CurrentAbilitySlot != slot ? 0 :
                state.Ability.UseTime(context) - SelectedUnit.CurrentAbilityRemainingUseTime;
            button.UseProgressIndicator.MaxValue = state.Ability.UseTime(context);
            
            button.HealthCostLabel.Text = Mathf.CeilToInt(state.Ability.HealthCost(context)).ToString();
            button.HealthCostLabel.Visible = state.Ability.HealthCost(context) > 0;
            
            button.ManaCostLabel.Text = Mathf.CeilToInt(state.Ability.ManaCost(context)).ToString();
            button.ManaCostLabel.Visible = state.Ability.ManaCost(context) > 0;
        }
    }
}