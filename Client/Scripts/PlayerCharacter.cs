using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Soteo.Client
{
    public class PlayerCharacter : KinematicBody2D
    {
        [Export] private float _maxSpeed = 50;
        [Export] private float _stopSpeed = 5;
    }
}
