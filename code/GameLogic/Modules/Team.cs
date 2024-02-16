using Sandbox;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sandbox.GameLogic.Modules;

public class Team
{
	public string Name { get; set; }
	public string Color { get; set; }
	public List<Guid> TeamPlayers { get; set; } = new();

	public Team(string name, string color)
	{
		Name = name;
		Color = color;
		TeamPlayers = new();
	}
}
