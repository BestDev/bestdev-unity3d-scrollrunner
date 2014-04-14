/**
 * @file GUIManager.cs
 * @date 2014/03/24
 * @author SungMin Lee(bestdev@gameonstudio.co.kr)
 * @brief 게임 시작 / 종료 GUI 관리 스크립트
 */

using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{
	public GUIText startText, endText, titleText, resultText;
	// Use this for initialization
	void Start ()
	{
		GameEventManager.RunStart += RunStart;
		GameEventManager.RunEnd += RunEnd;

		endText.enabled = false;
		//resultText.transform.position = new Vector3(0.5f, 0.4f, 0);
		resultText.text = "Jump (up arrow)\nRed (Speed Up) / Blue (Speed Down)\nPink (Camera Left Turn)\nYellow (Camera Right Turn)\nWhite (Camera 180º Turn)";
		resultText.enabled = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
#if UNITY_STANDALONE_WIN
		// PC 시작
		if(Input.GetButtonDown("Jump"))
		{
			GameEventManager.TriggerRunStart();
		}
#endif

#if UNITY_ANDROID
		// 모바일 시작
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{
			GameEventManager.TriggerRunStart();
		}
#endif
	}

	private void RunStart()
	{
		endText.enabled = false;
		titleText.enabled = false;
		startText.enabled = false;
		resultText.enabled = false;
		enabled = false;
	}

	private void RunEnd()
	{
		float fRundistance = Runner.distanceTraveled - 1f;
		resultText.text = "Run distance : " + fRundistance + "m\n" + "Jump Count : " + Runner.jumpCount + "\nTurn Count : " + Runner.turnCount + "\nTotal Item : " + Runner.pickItemCount + "\nLast Difficulty : " + PlatformManager.platformDifficulty;

		resultText.enabled = true;
		endText.enabled = true;
		startText.enabled = true;
		enabled = true;
	}
}
