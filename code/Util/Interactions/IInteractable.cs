﻿namespace Sauna;

public interface IInteractable
{
	private readonly static Dictionary<IInteractable, Dictionary<string, List<InteractionInfo>>> all = new();

	/// <summary>
	/// The displayed title of this interactable.
	/// </summary>
	public string DisplayTitle => "";

	/// <summary>
	/// The color used for the displayed text of this interactable.
	/// </summary>
	public Color DisplayColor => Color.Orange;

	/// <summary>
	/// The offset of this interactable's interaction hint.
	/// </summary>
	public InteractionOffset Offset => null;

	/// <summary>
	/// If the interaction is enabled or not. Has to match on client & server.
	/// </summary>
	public bool Enabled => true;

	/// <summary>
	/// All interactions of this interactable
	/// </summary>
	public Dictionary<string, List<InteractionInfo>> All => Get( this );

	/// <summary>
	/// Get all the interactions for an IInteractable.
	/// </summary>
	/// <param name="interactable"></param>
	/// <returns></returns>
	public static Dictionary<string, List<InteractionInfo>> Get( IInteractable interactable )
	{
		if ( !all.TryGetValue( interactable, out var interactions ) )
			all.Add( interactable, interactions = new Dictionary<string, List<InteractionInfo>>() );

		return interactions;
	}

	/// <summary>
	/// Adds a new interaction that is bound to a specific InputButton.
	/// </summary>
	/// <param name="action"></param>
	/// <param name="info"></param>
	public void AddInteraction( string action, InteractionInfo info )
	{
		if ( !All.TryGetValue( action, out var interactions ) )
			All.Add( action, interactions = new List<InteractionInfo>() );

		interactions.Add( info );
	}
}
