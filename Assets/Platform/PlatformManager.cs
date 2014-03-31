using UnityEngine;
using System.Collections.Generic;

public class PlatformManager : MonoBehaviour
{
	public Transform platform;
	internal Transform platformCopy;
	
	internal GameObject Player;
	
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
	
	private float fQueuePosition;
	private Vector3[] nextPosition;
	private float fTurn;
	private float fQueueTemp;
	
	private int turnFlag = 0;
	
	// Use this for initialization
	void Start ()
	{
		Player = GameObject.FindWithTag("Runner");
		
		wallArray = new Transform[4, numberOfObjects];
		
		nextPosition = new Vector3[startPosition.Length];
		
		for(int i = 0; i < startPosition.Length; i++)
		{
			nextPosition[i] = startPosition[i];
		}
		
		for(int i = 0; i < numberOfObjects; i++)
		{
			for(int k = 0; k < startPosition.Length; k++)
			{
				platformCopy = Instantiate(platform, transform.position, Quaternion.Euler(startRotation[k])) as Transform;
				
				wallArray[k, i] = platformCopy;
				// 0 : Bottom, 1 : Left, 2 : Right, 3 : Top
				Recycle(k, i);
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Debug.Log("pm update");
		//fQueuePosition = objectQueueBottom.Peek().localPosition.z;
		fQueuePosition = wallArray[0, nNowWallIndex].localPosition.z;
		//Debug.Log(wallArray[0, nNowWallIndex].localPosition);
		if(fQueuePosition + recycleOffset < Runner.distanceTraveled - 3)
		{
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

			if((int)Runner.distanceTraveled % 50 == 0)
			{
				PlatformManager.platformDifficulty -= 5;
				
				if(PlatformManager.platformDifficulty < 5)
				{
					PlatformManager.platformDifficulty = 5;
				}
			}
		}
		
		if(turnType != -1)
		{
			for(int i = 0; i < startPosition.Length; i++)
			{
				for(int k = 0; k < numberOfObjects; k++)
				{
					PlatformTurn(i, k);
					//System.Threading.Thread.Sleep(1);
				}
			}
			
			if(turnType == 0 || turnType == 2 || turnType == 4 || turnType == 6)
			{
				//turnFlag -= 1;
				turnFlag = 1;
			}
			else if(turnType == 1 || turnType == 3 || turnType == 5 || turnType == 7)
			{
				//turnFlag += 1;
				turnFlag = 2;
			}

			for(int i = 0; i < startPosition.Length; i++)
			{
				if(turnFlag < 0)
				{
					startRotation[i].z += 90;
					if(startRotation[i].z > 360)
					{
						startRotation[i].z -= 360;
					}
				}
				else
				{
					startRotation[i].z -= 90;
					if(startRotation[i].z < 360)
					{
						startRotation[i].z += 360;
					}
				}

				//Debug.Log("wall " + i + "startrot z : " + startRotation[i].z + " turnFlag sum : " +(turnFlag * 90));
			}

			turnType = -1;

			//System.Threading.Thread.Sleep(100);
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
		WallPosCorrection(ref obj, ref position, _nWall, fSizeRnd, nPosRnd);

		//Debug.Log(_nWall + " " + _nIndex + "out " + position);
		
		//Debug.Log("wall " + _nWall + " index " + _nIndex);
		//Debug.Log(wallArray[_nWall, _nIndex]);
		
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
		
		obj.localScale = scale;
		obj.localPosition = position;
		obj.localRotation = Quaternion.Euler(startRotation[_nWall]);
		
		int nRegenIndex = Random.Range(0, trapMaterials.Length);
		int nTrapPercent = Random.Range(0, 100);
		
		// Trap
		if(trapRegenPercent[nRegenIndex] > nTrapPercent)
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
	
	private void WallPosCorrection(ref Transform _obj, ref Vector3 _v3Pos, int _nWall, float _fSizeRnd, int _nPosRnd)
	{
		float fCorrection = 0;
		
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
		}
			break;
		case 3:
		{
			if(_nPosRnd == 0 || _nPosRnd == 2)
				fCorrection = -0.5f;
			else if(_nPosRnd == 1 || _nPosRnd == 3)
				fCorrection = 0.5f;
		}
			break;
		default:
			break;
		}

		if(turnFlag == 0)
		{
			if(_nWall == 0 || _nWall == 3)
			{
				_v3Pos.x += fCorrection;
				_obj.Translate(_v3Pos);
			}
			else if(_nWall == 1 || _nWall == 2)
			{
				_v3Pos.y += fCorrection;
				_obj.Translate(_v3Pos);
			}
		}
		else
		{
			if(_nWall == 0 || _nWall == 3)
			{
				_v3Pos.y -= fCorrection;
				_obj.Translate(_v3Pos);
			}
			else if(_nWall == 1 || _nWall == 2)
			{
				_v3Pos.x -= fCorrection;
				_obj.Translate(_v3Pos);
			}
		}

		/*
		//Debug.Log("wall " + _nWall + " nextpos : " + _v3Pos);
		if(_nWall == 0 || _nWall == 3)
		{
			if(turnFlag % 2 == 0)
			{
				_v3Pos.x += fCorrection;
			}
			else
			{
				_v3Pos.y += fCorrection;
			}
			//if(_fSizeRnd != 4.0f)
			//	Debug.Log("size : " + _fSizeRnd + " pos : " + _v3Pos);
		}
		else if(_nWall == 1 || _nWall == 2)
		{
			if(turnFlag % 2 == 0)
			{
				_v3Pos.y += fCorrection;
			}
			else
			{
				_v3Pos.x += fCorrection;
			}
		}
*/
		//Debug.Log("turnFlag " + turnFlag %2 + "wallpos " + _nWall + " x : " + _v3Pos.x + " y : " + _v3Pos.y);
	}
	
	public void PlatformTurn(int _nWall, int _nIndex)
	{
		obj = wallArray[_nWall, _nIndex];
		// 0:BL, 1:LT, 2:TR, 3:RB, 4:BR, 5:RT, 6:TL, 7:LB

		Vector3 pos = Player.transform.localPosition;
		/*
		if(turnType <= 3)	// BL (B->R)
		{
			Debug.Log("Turn Type " + turnType + " wall " + _nWall + " idx " + _nIndex);
			//Vector3 pos = obj.localPosition;
			//pos.x -= 1.5f;
			//pos.y = 0f;
			
			//obj.localRotation = Quaternion.Euler(0, 0, 90f);
			//obj.localPosition = pos;
			
			//pos = obj.transform.TransformPoint(pos);
			obj.RotateAround(pos, transform.forward, 90.0f);
			//obj.localPosition = pos;
		}
		else if(turnType >= 4)	// BR (B->L)
		{
			Debug.Log("Turn Type " + turnType + " wall " + _nWall + " idx " + _nIndex);
			
			//pos = obj.transform.TransformPoint(pos);
			obj.RotateAround(pos, transform.forward, -90.0f);
		}
		obj.localPosition = pos;
		*/
		//Debug.Log("Turn Type " + turnType + " wall " + _nWall + " idx " + _nIndex);
		pos.y = -1.8f;

		if(_nWall == 0)
		{
			if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// BL (B->R)
			{
				obj.RotateAround(pos, Vector3.forward, 90.0f);
			}
			else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// BR (B->L)
			{
				obj.RotateAround(pos, Vector3.forward, -90.0f);
			}
		}
		else if(_nWall == 1)
		{
			if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// LT (L->B)
			{
				obj.RotateAround(pos, Vector3.forward, 90.0f);
			}
			else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// LB (L->T)
			{
				obj.RotateAround(pos, Vector3.forward, -90.0f);
			}
		}
		else if(_nWall == 2)
		{
			if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// RB (R->T)
			{
				obj.RotateAround(pos, Vector3.forward, 90.0f);
			}
			else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// RT (R->B)
			{
				obj.RotateAround(pos, Vector3.forward, -90.0f);
			}
		}
		else if(_nWall == 3)
		{
			if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// TR (T->L)
			{
				obj.RotateAround(pos, Vector3.forward, 90.0f);
			}
			else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// TL (T->R)
			{
				obj.RotateAround(pos, Vector3.forward, -90.0f);
			}
		}

		//Debug.Log("wall : " + _nWall + "old next " + nextPosition[_nWall]);
		//if(_nIndex == nNowWallIndex)
		{
			nextPosition[_nWall].x = obj.localPosition.x;
			nextPosition[_nWall].y = obj.localPosition.y;
		}

		//Debug.Log("wall : " + _nWall + "new next " + nextPosition[_nWall]);
		//Debug.Log("type : " + turnType + " " + _nWall + " pos : " + obj.localPosition + " rot :" + obj.localRotation);
		//obj.localPosition = pos;
	}
}