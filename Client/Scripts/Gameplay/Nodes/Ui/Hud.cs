using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class Hud : Control, IHud
{
    private TextureProgress _healthBar = null!;
    private TextureProgress _manaBar = null!;
    private Label _healthLabel = null!;
    private Label _manaLabel = null!;
    private ImmutableList<AbilityButton> _abilityButtons = null!;
    
    private IEntityLocator _entityLocator = null!;
    private ICurrentUserIdRepository _currentUserIdRepository = null!;
    
    public Unit? SelectedUnit { get; set; }

    [Inject]
    public void Inject(IEntityLocator entityLocator, ICurrentUserIdRepository currentUserIdRepository)
    {
        _entityLocator = entityLocator;
        _currentUserIdRepository = currentUserIdRepository;
    }
    
    public override void _Ready()
    {
        if (IsServer)
        {
            SetProcess(false);
            QueueFree();
            return;
        }
        
        _healthBar = GetNode<TextureProgress>("UnitPanel/VBoxContainer/Health");
        _manaBar = GetNode<TextureProgress>("UnitPanel/VBoxContainer/Mana");
        _healthLabel = _healthBar.GetNode<Label>("Label");
        _manaLabel = _manaBar.GetNode<Label>("Label");
        _abilityButtons =
            GetNode("UnitPanel/VBoxContainer/Abilities").GetChildren().Cast<AbilityButton>().ToImmutableList();

        for (var i = 0; i < _abilityButtons.Count; i++)
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
            
            button.CooldownIndicator.Value = state.Cooldown;
            button.CooldownIndicator.MaxValue = state.Ability.Cooldown[state.Level];

            button.UseProgressIndicator.Value = SelectedUnit.CurrentAbilitySlot != slot ? 0 :
                state.Ability.UseTime[state.Level] - SelectedUnit.CurrentAbilityRemainingUseTime;
            button.UseProgressIndicator.MaxValue = state.Ability.UseTime[state.Level];
            
            button.HealthCostLabel.Text = Mathf.CeilToInt(state.Ability.HealthCost[state.Level]).ToString();
            button.HealthCostLabel.Visible = state.Ability.HealthCost[state.Level] > 0;
            
            button.ManaCostLabel.Text = Mathf.CeilToInt(state.Ability.ManaCost[state.Level]).ToString();
            button.ManaCostLabel.Visible = state.Ability.ManaCost[state.Level] > 0;
        }
    }
}