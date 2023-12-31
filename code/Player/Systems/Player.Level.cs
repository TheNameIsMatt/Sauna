﻿namespace Sauna;

partial class Player
{
	[Net, Change( "onExperience" )] private int experience { get; set; }
	public int Experience
	{ 
		get => experience; 
		set
		{
			Game.AssertServer();
			
			rankUpdate = true;
			experience = Math.Max( value, 0 );
		}
	}

	public float Progress => rankIndex + 1 >= Rank.All.Count - 1
		? 1f
		: (float)(experience - Rank.Requirement) / Rank.All[rankIndex + 1].Requirement;

	private bool rankUpdate = true;
	private int rankIndex;

	public Rank Rank => Rank.All[rankIndex];

	void onExperience( int previous, int current )
	{
		var difference = current - previous;

		for ( int i = 0; i < Rank.All.Count; i++ )
			if ( Experience >= Rank.All[i].Requirement )
				rankIndex = i;
			else
				break;

		// Send update to UI.
		Thermometer.OnChange( difference );
	}
}
