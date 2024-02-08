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
	public bool IsStarted { get; private set; }
	#endregion



	#region Actions
	public Action Timeout { get; set; }
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
		if (!IsStarted) return;

		Timeout?.Invoke();
		IsStarted = false;
	}

	public void CheckRoundTime()
	{
		if ( TimeSinceStart.Relative >= RoundLength )
		{
			EndTheRound();
		}
	}
	#endregion
}
