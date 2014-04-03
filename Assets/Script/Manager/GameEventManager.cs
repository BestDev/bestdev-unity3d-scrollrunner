/**
 * @file GameEventManager.cs
 * @date 2014/03/24
 * @author SungMin Lee(bestdev@gameonstudio.co.kr)
 * @brief 게임 시작 / 종료 관리 스크립트
 */

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
