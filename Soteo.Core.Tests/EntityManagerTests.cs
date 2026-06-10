using System.Collections.Immutable;
using AwesomeAssertions;
using NSubstitute;
using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Entities;
using Soteo.Core.Enums;
using Soteo.Core.Services;
using ICamera = Soteo.Core.Interfaces.ICamera;
using IEntityNodeManager = Soteo.Core.Interfaces.IEntityNodeManager;
using IUnit = Soteo.Core.Interfaces.IUnit;

namespace Soteo.Core.Tests;

public sealed class EntityManagerTests
{
    private readonly EntityManager _sut;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityNodeManager _entityNodeManager;
    
    private readonly IUnit _projectileSourceChar;
    private readonly IUnit _projectileTargetChar;
    private readonly IUnit _bystanderChar;
    private readonly Projectile _projectile;

    public EntityManagerTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _entityNodeManager = Substitute.For<IEntityNodeManager>();
        _sut = new EntityManager(_serviceProvider, ClientDependency.Null<ICamera>(), _entityNodeManager);

        _projectileSourceChar = _sut.SpawnPlayerCharacter(Guid.NewGuid(), Guid.NewGuid());
        _projectileTargetChar = _sut.SpawnPlayerCharacter(Guid.NewGuid(), Guid.NewGuid());
        _bystanderChar = _sut.SpawnPlayerCharacter(Guid.NewGuid(), Guid.NewGuid());
        
        var abilityContext = new AbilityContext
        {
            Ability = Ability.Instance<TestAbility>(),
            Level = 0,
            User = _projectileSourceChar,
            UserStats = _projectileSourceChar.Stats.ToImmutableDictionary(),
            ServiceProvider = _serviceProvider,
            TargetUnit = _projectileTargetChar,
        };
        
        _projectile = _sut.SpawnProjectile(abilityContext, speed: 1, _projectileTargetChar);
    }
    
    [Fact]
    public void GetEntitySnapshotsReturnsSnapshotsForAllNotRemovedEntities()
    {
        IReadOnlyDictionary<Guid, EntitySnapshot> snapshots = _sut.GetEntitySnapshots();
        
        snapshots[_projectileSourceChar.Id].Id.Should().Be(_projectileSourceChar.Id);
        snapshots[_projectileTargetChar.Id].Id.Should().Be(_projectileTargetChar.Id);
        snapshots[_bystanderChar.Id].Id.Should().Be(_bystanderChar.Id);
        snapshots[_projectile.Id].Id.Should().Be(_projectile.Id);
    }
    
    [Fact]
    public void GetEntitySnapshotsReturnsSnapshotsForRemovedReferencedEntities()
    {
        _projectileSourceChar.Remove();
        _projectileTargetChar.Remove();
        IReadOnlyDictionary<Guid, EntitySnapshot> snapshots = _sut.GetEntitySnapshots();
        
        snapshots[_projectileSourceChar.Id].Id.Should().Be(_projectileSourceChar.Id);
        snapshots[_projectileTargetChar.Id].Id.Should().Be(_projectileTargetChar.Id);
    }
    
    [Fact]
    public void RemovingReferencedUnitAndSpawningAnotherWithSameIdReturnsReferenceToSameNotRemovedInstance()
    {
        _projectileSourceChar.Remove();
        _projectileTargetChar.Remove();
        IUnit newProjectileSourceChar =
            _sut.SpawnPlayerCharacter(_projectileSourceChar.Id, _projectileSourceChar.ControllingPlayerIds.Single());
        IUnit newProjectileTargetChar =
            _sut.SpawnPlayerCharacter(_projectileTargetChar.Id, _projectileTargetChar.ControllingPlayerIds.Single());
        
        newProjectileSourceChar.Should().BeSameAs(_projectileSourceChar);
        newProjectileSourceChar.IsRemoved.Should().BeFalse();
        newProjectileTargetChar.Should().BeSameAs(_projectileTargetChar);
        newProjectileTargetChar.IsRemoved.Should().BeFalse();
    } 
    
    [Fact]
    public void SpawningPlayerCharacterWithSameIdAsProjectileThrows()
    {
        _sut.Invoking(it => it.SpawnPlayerCharacter(_projectile.Id, _projectile.Id))
            .Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void SpawningPlayerCharacterWithSameIdAsRemovedProjectileThrows()
    {
        _projectile.Remove();
        _sut.Invoking(it => it.SpawnPlayerCharacter(_projectile.Id, _projectile.Id))
            .Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void SpawningPlayerCharacterWithSameIdAsAnotherPlayerCharacterThrows()
    {
        _sut.Invoking(it => it.SpawnPlayerCharacter(_bystanderChar.Id, _bystanderChar.ControllingPlayerIds.Single()))
            .Should().Throw<InvalidOperationException>();
    }
    
    public sealed class TestAbility : Ability
    {
        public override CanTarget Targeting => CanTarget.Passive;
    }
}
