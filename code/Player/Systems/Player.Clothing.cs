﻿namespace Sauna;

[Flags]
public enum ClothingSlot
{
	None = 0,
	Head = 1 << 0,
	Eyes = 1 << 1,
	Torso = 1 << 2,
	Legs = 1 << 3,
	Feet = 1 << 4,
}

partial class Player
{
	[Net] public AnimatedEntity Penoid { get; set; }

	/// <summary>
	/// Size of penoid shaped object in centimeters.
	/// </summary>
	public float? Size => Morphs.Get( "size" )
		* GetAttachment( "penoid_start", false )?.Position.Distance( GetAttachment( "penoid_max", false )?.Position ?? Vector3.Zero )
		* 2.54f; // Inches to centimeters.

	/// <summary>
	/// Get the transform of the penoid.
	/// </summary>
	/// <returns></returns>
	public Transform GetPenoidTransform()
	{
		if ( Penoid == null || !Penoid.IsValid ) // Something went wrong, penoid is invalid.
			return default;

		// Size of penice.
		var morph = Morphs.Get( "size" );

		// Size of smallest possible penice.
		var smallAttachment = GetAttachment( "penoid_min" );
		if ( smallAttachment == null )
			return default;

		// Size of biggest possible penice.
		var bigAttachment = GetAttachment( "penoid_max" );
		if ( bigAttachment == null )
			return default;

		// Lerp between the two values to get actual position of penice tip.
		return Transform.Lerp( smallAttachment.Value, bigAttachment.Value, morph, true );
	}

	[SaunaEvent.OnSpawn]
	private static void createPenoid( Player player )
	{
		if ( !Game.IsServer )
			return;

		var rand = new Random( (int)(player.Client.SteamId % int.MaxValue) );
		var size = rand.Next( 0, 100 ) / 100f;

		var penoid = new AnimatedEntity();
		penoid.SetModel( "models/guy/penoid.vmdl" );
		penoid.SetParent( player, true );
		penoid.EnableShadowCasting = false;
		penoid.EnableHideInFirstPerson = false;
		penoid.Transmit = TransmitType.Always;
		penoid.Tags.Add( "penoid" );

		player.Morphs.Set( "size", size );
		player.Penoid = penoid;
	}
}
