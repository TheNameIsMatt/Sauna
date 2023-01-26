﻿namespace Sauna;

public partial class Player : AnimatedEntity
{
	public override void Spawn()
	{
		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}	

	public override void Simulate( IClient cl )
	{
		// Simulate the player's movement.
		MoveSimulate( cl );

		if ( Game.IsServer && Input.Pressed( InputButton.PrimaryAttack ) )
		{
			Experience = Game.Random.Int( 100 );
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		Rotation = ViewAngles.ToRotation();

		Camera.Position = Position + Vector3.Up * CollisionBox.Maxs.z;
		Camera.Rotation = Rotation;

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 70f );
		Camera.FirstPersonViewer = this;
	}
}