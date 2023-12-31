﻿namespace Sauna;

public abstract class BaseEffect
{
	/// <summary>
	/// Amount of stacks currently on this effect.
	/// </summary>
	public int Stacks { get; set; }

	/// <summary>
	/// Duration of this effect in seconds.
	/// </summary>
	public float Duration { get; set; }

	/// <summary>
	/// Is the effect permanent or not.
	/// </summary>
	public bool Permanent { get; set; }

	/// <summary>
	/// The target of this effect.
	/// </summary>
	public Player Target { get; set; }

	/// <summary>
	/// The text displayed for this effect.
	/// </summary>
	public virtual string Text { get; }

	/// <summary>
	/// The path for this effect's icon.
	/// </summary>
	public virtual string IconPath { get; }

	/// <summary>
	/// The maximum duration for this effect in seconds.
	/// </summary>
	public virtual float MaxDuration { get; } = 0f;

	/// <summary>
	/// The maximum amount of stacks for this effect.
	/// </summary>
	public virtual int MaxStacks { get; } = 1;

	public virtual void Simulate() { }

	public virtual void OnEnd() { }

	public void Remove()
	{
		if ( Target is not Player pawn || !pawn.IsValid )
			return;

		pawn.Effects.Remove( this );
	}
}
