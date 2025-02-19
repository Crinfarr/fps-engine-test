using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
    [Export] public float MouseSensitivity = 0.03f;
    [Export] public float DeltaSpeed = 1.0f;
    [Export] public float MaxSpeed = 5.0f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float PlayerMass = 30.0f;

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        Camera3D cam = (Camera3D)this.GetNode("povYouAreThePlayer");
        lastFacing = cam.Position;
    }
    public Vector3 lastFacing = Vector3.Zero;
    public Vector3 CurrFacing = Vector3.Zero;
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventKey)
        {
            InputEventKey iek = @event as InputEventKey;
            if (iek.IsActionPressed("ui_cancel"))
            {
                //DEBUG
                GetTree().Quit();
            }
        }
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion iemm = @event as InputEventMouseMotion;
            if (iemm.Relative != Vector2.Zero)
            {
                //mouse did somethin
                this.CurrFacing.X -= iemm.Relative.Y * MouseSensitivity;
                this.CurrFacing.Y -= iemm.Relative.X * MouseSensitivity;
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        Vector3 nv = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            nv += GetGravity() * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            nv.Y = JumpVelocity;
        }
        if (lastFacing != CurrFacing)
        {
            CurrFacing.X = Math.Clamp(CurrFacing.X, (float)-Math.PI / 2, (float)Math.PI / 2);
            this.Rotation = CurrFacing;
            lastFacing = CurrFacing;
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            nv.X += direction.X * DeltaSpeed;
            nv.Z += direction.Z * DeltaSpeed;
            nv.X = Math.Clamp(nv.X, -MaxSpeed, MaxSpeed);
            nv.Z = Math.Clamp(nv.Z, -MaxSpeed, MaxSpeed);
        }
        else
        {
            nv.X = Mathf.MoveToward(Velocity.X, 0, DeltaSpeed);
            nv.Z = Mathf.MoveToward(Velocity.Z, 0, DeltaSpeed);
        }

        Velocity = nv;
        MoveAndSlide();
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);
            if (collision.GetCollider() is RigidBody3D)
            {
                RigidBody3D body = collision.GetCollider() as RigidBody3D;
                Vector3 collide = collision.GetNormal();
                this.Velocity += collide * ((body.LinearVelocity.Length() * body.Mass) - (this.Velocity.Length() * PlayerMass));
                Vector3 impulse = collide * ((this.Velocity.Length() * PlayerMass) - (body.LinearVelocity.Length() * body.Mass));
                body.ApplyForce(impulse);
            }
        }
    }
}
