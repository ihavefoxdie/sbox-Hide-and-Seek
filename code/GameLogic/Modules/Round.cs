using Sandbox;
using System;
using System.Threading;

namespace Sandbox.GameLogic.Modules;

public class Round
{
	#region Properties
	public TimeSince TimeSinceStart { get; private set; }
	/// <summary>
	/// RoundLength in seconds
	/// </summary>
	public int RoundLength { get; private set; }
	public bool IsStarted { get; private set; } = false;
	#endregion



	#region Actions
	public Action End { get; set; }
	public Action Start { get; set; }
	#endregion



	#region Variables
	#endregion



	public Round()
	{
		RoundLength = 300;
	}

	public Round( int roundLength = 300 )
	{
		RoundLength = roundLength;
	}



	#region Methods
	public void StartTheRound()
	{
		if(IsStarted) return;

		TimeSinceStart = 0;
		Start?.Invoke();
		IsStarted = true;
	}

	public void EndTheRound()
	{
		End?.Invoke();
		IsStarted = false;
	}

	public bool CheckRoundTime()
	{
		if ( TimeSinceStart.Relative >= RoundLength )
		{
			return true;
		}
		return false;
	}
	#endregion
}
