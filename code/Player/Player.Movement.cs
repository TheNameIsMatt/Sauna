﻿namespace Sauna;

partial class Player
{
	/// <summary>
	/// Should we block the player's inputs?
	/// </summary>
	[Net] public bool BlockInput { get; set; }

	// Client Inputs
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	private TimeSince lastWaterSound = 0f;
	private Vector3 lastPosition;

	/// <summary>
	/// Normalized vector of the direction the player wishes to move towards.
	/// </summary>
	public Vector3 WishVelocity { get; set; }

	/// <summary>
	/// The collider used for player's movement collisions.
	/// </summary>
	public BBox CollisionBox => new BBox( 
		new Vector3( -16, -16, 0 ), 
		new Vector3( 16, 16, Ducking ? 40 : 68 ) 
	);

	/// <summary>
	/// A boolean determining if the player is ducked or not.
	/// </summary>
	public bool Ducking { get; private set; } = false;

	/// <summary>
	/// Is the player in a water entity?
	/// </summary>
	public SaunaWater Water { get; set; } = null;

	// Private fields
	private float stepSize => 8f;
	private float walkSpeed => 60f;
	private float maxStandableAngle => 45f;
	private Vector3 gravity => Vector3.Down * 650f;

	private void moveSimulate( IClient cl )
	{
		// Don't keep on going if the player is ragdolled.
		if ( Ragdoll != null && Ragdoll.IsValid )
			return;

		// Handle jumping.
		if ( Input.Pressed( "jump" ) && !BlockInput && GroundEntity != null && Water == null )
		{
			GroundEntity = null;
			Velocity += Vector3.Up * 200f;
			SetAnimParameter( "jump", true );
		}

		// Handle ducking.
		var from = Position;
		var to = from + Vector3.Up * CollisionBox.Maxs.z;
		Ducking = (Ducking && Trace.Box( CollisionBox, in from, in to ).Ignore( this ).WithoutTags( "trigger" ).Run().Hit)
			|| (Water == null && Input.Down( "duck" )); // Beautiful.

		SetAnimParameter( "crouching", Ducking );

		// Handle rotation.
		var viewAngles = new Angles( 0, ViewAngles.yaw, 0 );
		Rotation = BlockInput 
			? Rotation 
			: Angles.Lerp( Rotation.Angles(), viewAngles, 10f * Time.Delta )
				.ToRotation();

		if ( !BlockInput )
			SetAnimLookAt( "lookat", EyePosition, EyePosition + ViewAngles.Forward );

		// Handle the player's wish velocity.
		var eyeRotation = ViewAngles.WithPitch( 0 ).ToRotation();
		WishVelocity = BlockInput 
			? 0
			: (InputDirection * eyeRotation).Normal
				.WithZ( 0 );

		var running = Water == null && !Ducking && Input.Down( "run" );
		var mult = (running ? 1f : 0.5f)
			 * MathF.Min( MathF.Abs( Velocity.WithZ( 0 ).Length ) / walkSpeed, 1f );

		SetAnimParameter( "move_x", MathX.LerpTo( GetAnimParameterFloat( "move_x" ), InputDirection.x * mult, 10f * Time.Delta ) );
		SetAnimParameter( "move_y", MathX.LerpTo( GetAnimParameterFloat( "move_y" ), -InputDirection.y * mult, 10f * Time.Delta ) );

		// Calculate velocity.
		var targetVelocity = WishVelocity
			* (walkSpeed * (running ? 3f : 1))
			* (Ducking ? 0.5f : 1f);
		
		Velocity = Vector3.Lerp( Velocity, targetVelocity, 10f * Time.Delta )
			.WithZ( Velocity.z );

		if ( GroundEntity == null )
			Velocity += gravity * Time.Delta;

		// Initialize MoveHelper.
		var helper = new MoveHelper( Position, Velocity )
		{
			MaxStandableAngle = maxStandableAngle
		};

		// Move the player using MoveHelper.
		helper.Trace = helper.Trace
			.Size( CollisionBox.Mins, CollisionBox.Maxs )
			.WithoutTags( "nocollide" )
			.IncludeClientside()
			.Ignore( this );

		if ( helper.HitWall )
			helper.ApplyFriction( 5f, Time.Delta );

		helper.TryUnstuck();
		helper.TryMoveWithStep( Time.Delta, stepSize );

		lastPosition = Position;
		Position = helper.Position
			.WithZ( Water != null ? MathF.Max( Water.Position.z - 95 + Water.WaveOffset( Position ), helper.Position.z ) : helper.Position.z );
		Velocity = helper.Velocity
			.WithZ( Water != null ? MathX.Lerp( Velocity.z, 0, 5f * Time.Delta ) : Velocity.z );

		// Check for ground collision.
		if ( Velocity.z <= stepSize )
		{
			var tr = helper.TraceDirection( Vector3.Down * 2f );
			GroundEntity = tr.Entity;

			// Move to the ground if there is something.
			if ( GroundEntity != null )
			{
				Position += tr.Distance * Vector3.Down;

				if ( Velocity.z < 0.0f )
					Velocity = Velocity.WithZ( 0 );
			}
		}
		else
			GroundEntity = null;

		SetAnimParameter( "grounded", GroundEntity != null );

		// Water movement sounds.
		if ( Water != null )
		{
			if ( lastWaterSound >= 0.75f && !WishVelocity.IsNearZeroLength )
			{
				var splashLoudness = Math.Min( (0.4f + lastPosition - Position).z, 1.3f ); // Louder if you're coming in, quieter if you're coming out.

				Sound.FromEntity( "sounds/water/water_splash.sound", this )
					.SetVolume( splashLoudness );

				lastWaterSound = 0f;
			}
		}
	}

	public override void BuildInput()
	{
		if ( Game.LocalPawn is not Player pawn )
			return;

		// Handle the client's inputs.
		InputDirection = Input.AnalogMove;

		var ang = (ViewAngles + Input.AnalogLook);

		ViewAngles = ang
			.WithPitch( MathX.Clamp( ang.pitch, -90, 90 ) )
			.Normal;
	}
}
