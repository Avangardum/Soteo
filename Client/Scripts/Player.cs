using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Soteo.Client
{
    public class Player : KinematicBody2D
    {
        [Export] private float _maxSpeed = 50;
        [Export] private float _stopSpeed = 5;
        
        private SceneTree _sceneTree;
        private TickCounter _tickCounter;
        
        private Dictionary<long, Vector2> _serverPositions = new Dictionary<long, Vector2>();
        private float _stopSpeedSquared;
        private Vector2? _movementTarget;
        private Vector2 _interpolationStartPosition;
        private float _ticksPerSecond;
        private RingBuffer<int> _extraFutureTicksCounts = new RingBuffer<int>(100);
        
        public int Id { get; set; }
        public bool IsLocal { get; set; }
        
        public override void _Ready()
        {
            _sceneTree = GetTree();
            _tickCounter = GetNode<TickCounter>("/root/Main/TickCounter");
            
            _stopSpeedSquared = _stopSpeed * _stopSpeed;
            _ticksPerSecond = Engine.IterationsPerSecond;
        }
        
        public override void _Process(float delta)
        {
            UpdateClientState(delta);
            ProcessInput();
        }
        
        public override void _PhysicsProcess(float delta)
        {
            Move(delta);
            BroadcastServerState();
        }

        private void ProcessInput()
        {
            if (!IsLocal) return;
            if (Input.IsActionJustPressed("move"))
            {
                RpcId(1, nameof(SetMovementTarget), GetGlobalMousePosition());
            }
            if (Input.IsActionJustPressed("stop")) RpcId(1, nameof(Stop));
        }

        private void Move(float delta)
        {
            if (!_sceneTree.IsNetworkServer()) return;
            if (_movementTarget == null) return;
            Vector2 desiredMovement = _movementTarget.Value - Position;
            float desiredSpeed = desiredMovement.Length() / delta;
            float speed = Mathf.Min(desiredSpeed, _maxSpeed);
            Vector2 velocity = desiredMovement.Normalized() * speed;
            Vector2 velocityAfterSlide = MoveAndSlide(velocity);
            if (velocityAfterSlide.LengthSquared() < _stopSpeedSquared) _movementTarget = null;
        }
        
        private void BroadcastServerState()
        {
            if (!_sceneTree.IsNetworkServer()) return;
            RpcUnreliable(nameof(RpcSetPosition), Position, false, _tickCounter.Tick);
        }
        
        private void UpdateClientState(float delta)
        {
            if (_sceneTree.IsNetworkServer()) return;
            if (!_tickCounter.IsInitialized) return;
            
            List<long> ticks = _serverPositions.Keys.OrderBy(it => it).ToList();
            if (ticks.Count < 2) return;
            
            long lastTick = ticks.Last();
            bool isFutureTickMissingBeforeTimerAdjustment = lastTick <= _tickCounter.Tick;
            if (isFutureTickMissingBeforeTimerAdjustment)
            {
                if (!IsLocal) return;
                long newCurrentTick = lastTick - 1;
                int rollbackTicks = (int)(_tickCounter.Tick - newCurrentTick);
                _tickCounter.Tick -= rollbackTicks;
                GD.Print($"Rollback {rollbackTicks} ticks.");
            }
            else if (IsLocal)
            {
                int extraFutureTickCountBeforeTimerAdjustment = Math.Max(ticks.Count(it => it > _tickCounter.Tick) - 1, 0);
                int fastForwardTicks = Math.Min(extraFutureTickCountBeforeTimerAdjustment, _extraFutureTicksCounts.Min()) - 2;
                if (fastForwardTicks > 0)
                {
                    _tickCounter.Tick += fastForwardTicks;
                    _extraFutureTicksCounts.Fill(0);
                    GD.Print($"Fast forward {fastForwardTicks} ticks.");
                }
            }
            
            bool hasPastPosition = ticks.First() <= _tickCounter.Tick;
            if (!hasPastPosition) return;
            
            long latestPastTick = ticks.Last(it => it <= _tickCounter.Tick);
            Vector2 latestPastPosition = _serverPositions[latestPastTick];
            
            long earliestFutureTick = ticks.First(it => it > _tickCounter.Tick);
            int extraFutureTickCount = ticks.Count - ticks.IndexOf(earliestFutureTick) - 1;
            _extraFutureTicksCounts[_tickCounter.Tick] = extraFutureTickCount;
            Vector2 earliestFuturePosition = _serverPositions[earliestFutureTick];
            float ticksBetweenPastAndFuture = earliestFutureTick - latestPastTick;
            float ticksBetweenPastAndPresent = _tickCounter.Tick - latestPastTick + _tickCounter.TickProgress;
            float interpolationWeight = ticksBetweenPastAndPresent / ticksBetweenPastAndFuture;
            Vector2 prevPosition = Position;
            Position = latestPastPosition.LinearInterpolate(earliestFuturePosition, interpolationWeight);
            
            CleanupServerPositions(ticks);
        }
        
        private void CleanupServerPositions(IList<long> orderedTicks)
        {
            const int sizeLimit = 50;
            int size = orderedTicks.Count;
            foreach (long tick in orderedTicks)
            {
                if (size <= sizeLimit) return;
                _serverPositions.Remove(tick);
                size--;
            }
        }
        
        [Master]
        public void SetMovementTarget(Vector2 target)
        {
            if (_sceneTree.GetRpcSenderId() != Id) return;
            _movementTarget = target;
        }
        
        [Master]
        public void Stop()
        {
            if (_sceneTree.GetRpcSenderId() != Id) return;
            _movementTarget = null;
        }
        
        [Puppet]
        public void RpcSetPosition(Vector2 position, bool teleport, long tickIndex)
        {
            _serverPositions[tickIndex] = position;
        }
    }
}
