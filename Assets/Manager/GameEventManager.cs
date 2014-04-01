using UnityEngine;
using System.Collections;

public static class GameEventManager
{
	public delegate void GameEvent();

	public static event GameEvent RunStart, RunEnd;

	public static void TriggerRunStart()
	{
		if(RunStart != null)
		{
			RunStart();
		}
	}

	public static void TriggerRunEnd()
	{
		if(RunEnd != null)
		{
			RunEnd();
		}
	}
}
