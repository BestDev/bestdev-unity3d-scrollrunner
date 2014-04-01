using UnityEngine;
using System.Collections.Generic;

public class PlatformManager : MonoBehaviour
{
	public Transform platform;
	internal Transform platformCopy;
	
	internal GameObject Player;
	//internal Runner runner;
	
	public int numberOfObjects;
	public float recycleOffset;
	public Vector3[] startPosition;
	public Vector3[] startRotation;
	public Vector3 minSize;
	public Vector3 maxSize;
	
	public float turnAngle = 180;	// 회전 속도 (angle/s)
	
	public Material roadMaterials;
	public Material[] trapMaterials;
	public float[] trapRegenPercent;
	
	public static float platformDifficulty = 95;
	public static int turnType = -1;	// 0:BL, 1:LT, 2:TR, 3:RB, 4:BR, 5:RT, 6:TL, 7:RB
	
	private Transform obj;
	
	public Transform[,] wallArray;
	private int nNowWallIndex = 0;
	
	private float fQueuePosition = 0;
	private Vector3[] nextPosition;
	private Vector3[] nextRotation;

	private int turnFlag = 0;	// 1 : 좌회전 / 2 : 우회전
	private int nowBottomLayer = 0;	// 현재 바닥 레이어

	
	// Use this for initialization
	void Start ()
	{
		GameEventManager.RunStart += RunStart;
		GameEventManager.RunEnd += RunEnd;

		Player = GameObject.FindWithTag("Runner");

		wallArray = new Transform[4, numberOfObjects];

		nextPosition = new Vector3[startPosition.Length];
		nextRotation = new Vector3[startRotation.Length];

		nowBottomLayer = LayerMask.NameToLayer("Bottom");

		for(int i = 0; i < startPosition.Length; i++)
		{
			nextPosition[i] = startPosition[i];
			nextRotation[i] = startRotation[i];
		}
		
		for(int i = 0; i < numberOfObjects; i++)
		{
			for(int k = 0; k < startPosition.Length; k++)
			{
				platformCopy = Instantiate(platform, transform.position, Quaternion.Euler(nextRotation[k])) as Transform;
				
				wallArray[k, i] = platformCopy;
				// 0 : Bottom, 1 : Left, 2 : Right, 3 : Top
				Recycle(k, i);
			}
		}

		enabled = false;
	}

	private void RunStart()
	{
		//Debug.Log("RunStart");
		//nowBottomLayer = LayerMask.NameToLayer("Bottom");
		nNowWallIndex = 0;
        turnFlag = 0;

		if(platformDifficulty < 10)
		{
			platformDifficulty = 95;
		}

		for(int i = 0; i < startPosition.Length; i++)
		{
			nextPosition[i] = startPosition[i];
			nextRotation[i] = startRotation[i];
			//Debug.Log("next pos " + i + " " + startPosition[i]);
			//Debug.Log("next rot " + i + " " + startRotation[i]);
		}
		Debug.Log(numberOfObjects);		
		for(int i = 0; i < numberOfObjects; i++)
		{
			for(int k = 0; k < startPosition.Length; k++)
			{
				// 0 : Bottom, 1 : Left, 2 : Right, 3 : Top
				Recycle(k, i);
			}
		}

		enabled = true;
	}

	private void RunEnd()
	{
		enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Debug.Log("pm update");
		//fQueuePosition = objectQueueBottom.Peek().localPosition.z;
		fQueuePosition = wallArray[0, nNowWallIndex].localPosition.z;

		//Debug.Log(wallArray[0, nNowWallIndex].localPosition);
		//Debug.Log("fQueuePosition " + fQueuePosition + " run distravel " + Runner.distanceTraveled);
		if(fQueuePosition + recycleOffset < Runner.distanceTraveled - 3)
		{
			//Debug.Log("fQueuePosition " + fQueuePosition);
			//Debug.Log(nNowWallIndex);
			for(int i = 0; i < startPosition.Length; i++)
			{
				Recycle(i, nNowWallIndex);
			}
			
			nNowWallIndex++;
			
			if(nNowWallIndex == numberOfObjects)
			{
				nNowWallIndex = 0;
			}

			if((int)Runner.distanceTraveled % 30 == 0)
			{
				platformDifficulty -= 5;

				if((int)platformDifficulty % 10 == 0)
				{
					if(Player.GetComponent<Runner>().runSpeed < 15)
					{
						Player.GetComponent<Runner>().runSpeed += 1;
					}
				}

				if(platformDifficulty < 5)
				{
					platformDifficulty = 95;
				}
			}
		}

	}
	
	private void Recycle(int _nWall, int _nIndex)
	{
		int nRnd = Random.Range(0, 100);
		
		//Debug.Log("rnd : " + nRnd + " pd : " + platformDifficulty);
		if(nRnd < platformDifficulty)
		{
			minSize.x = 4.0f;
		}
		else
		{
			minSize.x = 0f;
		}
		/*
		else if(nRnd < platformDifficulty - 20)
		{
			minSize.x = 3.0f;
			maxSize.x = 3.0f;
		}
		else if(nRnd < platformDifficulty - 40)
		{
			minSize.x = 2.0f;
			maxSize.x = 2.0f;
		}
		*/
		
		//float fSizeRnd = (int)Random.Range(minSize.x, maxSize.x);
		float fSizeRnd = (int)Random.Range(minSize.x, maxSize.x);
		float fHeightRnd = Random.Range(minSize.y, maxSize.y);
		
		//Vector3 scale = new Vector3(fSizeRnd, minSize.y, minSize.z);
		Vector3 scale = new Vector3(fSizeRnd, fHeightRnd, minSize.z);
		//Random.Range(minSize.y, maxSize.y),
		//Random.Range(minSize.z, maxSize.z));
		
		//Debug.Log("min : " + minSize.x + " max : " + maxSize.x);
		
		Vector3 position = nextPosition[_nWall];
		//Debug.Log(_nWall + " " + _nIndex + "in " + position);
		//position.z += scale.z * 0.5f;
		//position.y += scale.y * 0.5f;
		int nPosRnd = Random.Range(0, 4);
		
		obj = wallArray[_nWall, _nIndex];
		//Debug.Log("idx " +_nIndex+  "wallpos " +obj.localPosition+ " nextpos " + nextPosition[_nWall]);

		// 타일 배치 보정 함수 (회전 시 타일 위치 보정에서 좌표가 어긋나서 일단 주석처리)
		WallPosCorrection(ref position, _nWall, fSizeRnd, nPosRnd);
		/*
		if(_nWall == 0)
		{
			if(fSizeRnd == 1)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.x -= 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.y += 0.5f;
				}
			}
			else if(fSizeRnd == 2)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.x += 1.0f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.y += 1.0f;
				}
			}
			else if(fSizeRnd == 3)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.x += 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.y += 0.5f;
				}
			}
		}
		else if(_nWall == 1)
		{
			if(fSizeRnd == 1)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.y += 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.x += 0.5f;
				}
			}
			else if(fSizeRnd == 2)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.y += 1.0f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.x += 1.0f;
				}
			}
			else if(fSizeRnd == 3)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.y += 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.x += 0.5f;
				}
			}
		}
		else if(_nWall == 2)
		{
			if(fSizeRnd == 1)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.y += 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.x += 0.5f;
				}
			}
			else if(fSizeRnd == 2)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.y += 1.0f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.x += 1.0f;
				}
			}
			else if(fSizeRnd == 3)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.y += 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.x += 0.5f;
				}
			}
		}
		else if(_nWall == 3)
		{
			if(fSizeRnd == 1)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.x += 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.y += 0.5f;
				}
			}
			else if(fSizeRnd == 2)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.x += 1.0f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.y += 1.0f;
				}
			}
			else if(fSizeRnd == 3)
			{
				if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
				{
					position.x += 0.5f;
				}
				else if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
				{
					position.y += 0.5f;
				}
			}
		}
		*/

		//Debug.Log(_nWall + " " + _nIndex + "out " + position);
		
		//Debug.Log("wall " + _nWall + " index " + _nIndex);
		//Debug.Log(wallArray[_nWall, _nIndex]);
		//Debug.Log("wall " +_nWall + " layer " + obj.gameObject.layer);
		if(obj.gameObject.layer == 0)
		{
			if(_nWall == 0)
			{
				//Debug.Log("cnt : " + cnt + " " + LayerMask.LayerToName(gameObject.layer));
				//obj = objectQueueBottom.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Bottom");
			}
			else if(_nWall == 1)
			{
				//Debug.Log("cnt : " + cnt + " " + LayerMask.LayerToName(gameObject.layer));
				//obj = objectQueueLeft.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Left");
			}
			else if(_nWall == 2)
			{
				//obj = objectQueueRight.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Right");
			}
			else if(_nWall == 3)
			{
				//obj = objectQueueTop.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Top");
			}
		}
		
		obj.localScale = scale;
		obj.localPosition = position;
		obj.localRotation = Quaternion.Euler(nextRotation[_nWall]);
		
		int nRegenIndex = Random.Range(0, trapMaterials.Length);
		int nTrapPercent = Random.Range(0, 100);
		
		// Trap
		if(trapRegenPercent[nRegenIndex] > nTrapPercent && _nIndex >= 5)
		{
			obj.renderer.material = trapMaterials[nRegenIndex];
		}
		// No Trap
		else
		{
			obj.renderer.material = roadMaterials;
		}
		
		nRnd = Random.Range(1, 100);
		
		//Debug.Log("nextpos old " + nextPosition[_nWall]);
		
		//nextPosition[_nWall] = position;
		nextPosition[_nWall].z += scale.z;
		//Debug.Log("nextpos new " + nextPosition[_nWall]);
	}
	
	private void WallPosCorrection(ref Vector3 _v3Pos, int _nWall, float _fSizeRnd, int _nPosRnd)
	{
		float fCorrection = 0;

		if(_fSizeRnd == 1f)
		{
			if(_nPosRnd == 0)
				fCorrection = -0.5f;
			else if(_nPosRnd == 1)
				fCorrection = 0.5f;
			else if(_nPosRnd == 2)
				fCorrection = -1.5f;
			else if(_nPosRnd == 3)
				fCorrection = 1.5f;
		}
		else if(_fSizeRnd == 2f)
		{
			if(_nPosRnd == 0)
				fCorrection = 0f;
			else if(_nPosRnd == 1)
				fCorrection = -1f;
			else if(_nPosRnd == 2)
				fCorrection = 1f;
			else if(_nPosRnd == 3)
			{
				// 3이 걸릴 경우 랜덤 사용 (확률적으로 이게 더 나을듯)
				fCorrection = (float)Random.Range(-1, 2);
				//Debug.Log(fCorrection);
			}
		}
		else if(_fSizeRnd == 3f)
		{
			if(_nPosRnd == 0 || _nPosRnd == 2)
				fCorrection = -0.5f;
			else if(_nPosRnd == 1 || _nPosRnd == 3)
				fCorrection = 0.5f;
		}

		/*
		switch((int)_fSizeRnd)
		{
		case 1:
		{
			if(_nPosRnd == 0)
				fCorrection = -0.5f;
			else if(_nPosRnd == 1)
				fCorrection = 0.5f;
			else if(_nPosRnd == 2)
				fCorrection = -1.5f;
			else if(_nPosRnd == 3)
				fCorrection = 1.5f;

			//Debug.Log(" size " + _fSizeRnd + " posrnd " + _nPosRnd + " corr " + fCorrection);
		}
			break;
		case 2:
		{
			if(_nPosRnd == 0)
				fCorrection = 0f;
			else if(_nPosRnd == 1)
				fCorrection = -1f;
			else if(_nPosRnd == 2)
				fCorrection = 1f;
			else if(_nPosRnd == 3)
			{
				// 3이 걸릴 경우 랜덤 사용 (확률적으로 이게 더 나을듯)
				fCorrection = (float)Random.Range(-1, 2);
				//Debug.Log(fCorrection);
			}

			//Debug.Log(" size " + _fSizeRnd + " posrnd " + _nPosRnd + " corr " + fCorrection);
		}
			break;
		case 3:
		{
			if(_nPosRnd == 0 || _nPosRnd == 2)
				fCorrection = -0.5f;
			else if(_nPosRnd == 1 || _nPosRnd == 3)
				fCorrection = 0.5f;

			//Debug.Log(" size " + _fSizeRnd + " posrnd " + _nPosRnd + " corr " + fCorrection);
		}
			break;
		default:
			break;
		}
		*/

		//Debug.Log("turnFlag " + turnFlag);
		//Debug.Log("old tf " + turnFlag + " wall " + _nWall + " pos " +_v3Pos + " now " + nowBottomLayer);
		if(turnFlag == 0)
		{
			if(_nWall == 0 || _nWall == 3)
			{
				_v3Pos.x += fCorrection;
				//_obj.Translate(_v3Pos);
			}
			else if(_nWall == 1 || _nWall == 2)
			{
				_v3Pos.y += fCorrection;
				//_obj.Translate(_v3Pos);
			}
		}
		else if(turnFlag == 1)
		{
			if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
			{
				if(_nWall == 0 || _nWall == 3)
				{
					_v3Pos.y -= fCorrection;
				}
				else if(_nWall == 1 || _nWall == 2)
				{
					_v3Pos.x -= fCorrection;
				}
			}
			else if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
			{
				if(_nWall == 0 || _nWall == 3)
				{
					_v3Pos.x -= fCorrection;
				}
				else if(_nWall == 1 || _nWall == 2)
				{
					_v3Pos.y -= fCorrection;
				}
			}

			//_obj.Translate(_v3Pos);
		}
		else if(turnFlag == 2)
		{
			if(nowBottomLayer == LayerMask.NameToLayer("Left") || nowBottomLayer == LayerMask.NameToLayer("Right"))
			{
				if(_nWall == 0 || _nWall == 3)
				{
					_v3Pos.y += fCorrection;
				}
				else if(_nWall == 1 || _nWall == 2)
				{
					_v3Pos.x += fCorrection;
				}
			}
			else if(nowBottomLayer == LayerMask.NameToLayer("Bottom") || nowBottomLayer == LayerMask.NameToLayer("Top"))
			{
				if(_nWall == 0 || _nWall == 3)
				{
					_v3Pos.x += fCorrection;
				}
				else if(_nWall == 1 || _nWall == 2)
				{
					_v3Pos.y += fCorrection;
				}
			}

			//_obj.Translate(_v3Pos);
		}

		//Debug.Log("new tf " + turnFlag + " wall " + _nWall + " pos " +_v3Pos);
	}

	// _nLayer[0] : OldLayer
	// _nLayer[1] : NewLayer
	public int ApplyPlatTurn(int[] _nLayer)
	{
		//Debug.Log("ApplyPlatTurn");
		Vector3 pos = Player.transform.localPosition;
		//Debug.Log("player pos " + pos + " old " + _nLayer[0] + " new " + _nLayer[1]);
		for(int i = 0; i < startPosition.Length; i++)
		{
			for(int k = 0; k < numberOfObjects; k++)
			{
				obj = wallArray[i, k];
				// 0:BL, 1:LT, 2:TR, 3:RB, 4:BR, 5:RT, 6:TL, 7:LB
				
				pos.y = -1.8f;

				//Debug.Log("Turn old : " + _nLayer[0] + " new : " + _nLayer[1]);
				if(_nLayer[0] == LayerMask.NameToLayer("Bottom") && _nLayer[1] == LayerMask.NameToLayer("Left")
				        || _nLayer[0] == LayerMask.NameToLayer("Left") && _nLayer[1] == LayerMask.NameToLayer("Top")
				        || _nLayer[0] == LayerMask.NameToLayer("Top") && _nLayer[1] == LayerMask.NameToLayer("Right")
				        || _nLayer[0] == LayerMask.NameToLayer("Right") && _nLayer[1] == LayerMask.NameToLayer("Bottom")
				        )
				{
					obj.RotateAround(pos, Vector3.forward, 90f);
					turnFlag = 1;
				}
				else if(_nLayer[0] == LayerMask.NameToLayer("Bottom") && _nLayer[1] == LayerMask.NameToLayer("Right")
				        || _nLayer[0] == LayerMask.NameToLayer("Right") && _nLayer[1] == LayerMask.NameToLayer("Top")
				        || _nLayer[0] == LayerMask.NameToLayer("Top") && _nLayer[1] == LayerMask.NameToLayer("Left")
				        || _nLayer[0] == LayerMask.NameToLayer("Left") && _nLayer[1] == LayerMask.NameToLayer("Bottom")
				        )
					
				{
					obj.RotateAround(pos, Vector3.forward, 270f);
					turnFlag = 2;
				}

				// 회전 후 재사용을 위해서 좌표 보정
				// 사이즈 4짜리 좌표만 추가 (나머지 좌표는 이미 보정된 좌표이므로 회전 시 좌표가 틀어짐)
				if(obj.transform.localScale.x == 4f)
				{
					//Debug.Log("4tile " + i);
					nextPosition[i].x = obj.localPosition.x;
					nextPosition[i].y = obj.localPosition.y;
				}

				nowBottomLayer = _nLayer[1];
				//Debug.Log("wall : " + i + " pos x : " + obj.localPosition.x + " pos y : " + obj.localPosition.y);
			}

			if(turnFlag == 1)
			{
				nextRotation[i].z += 90;
				if(nextRotation[i].z > 360)
				{
					nextRotation[i].z -= 360;
				}
			}
			else if(turnFlag == 2)
			{
				nextRotation[i].z -= 90;
				if(nextRotation[i].z < 360)
				{
					nextRotation[i].z += 360;
				}
			}
		}

		return 1;
	}
}