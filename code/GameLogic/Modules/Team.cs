using System;
using System.Collections.Generic;

namespace Sandbox.GameLogic.Modules;

public class Team
{
	/// <summary>
	/// Team's name.
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// Team's color.
	/// </summary>
	public string Color { get; set; }
	/// <summary>
	/// List of player connections in this team.
	/// </summary>
	public List<Guid> TeamPlayers { get; set; } = new();

	public Team(string name, string color)
	{
		Name = name;
		Color = color;
		TeamPlayers = new();
	}
}
