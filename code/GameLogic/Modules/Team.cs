using Sandbox;
using System.Collections.Generic;

namespace Sandbox.GameLogic.Modules;

public class Team
{
	public string Name { get; set; }
	public string Color { get; set; }
	public List<Connection> TeamPlayers { get; set; }

	public Team(string name, string color)
	{
		Name = name;
		Color = color;
		TeamPlayers = new List<Connection>();
	}
}
