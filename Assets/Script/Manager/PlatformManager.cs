/**
 * @file PlatformManager.cs
 * @date 2014/03/24
 * @author SungMin Lee(bestdev@gameonstudio.co.kr)
 * @brief 바닥면 관리 스크립트
 */

using UnityEngine;
using System.Collections.Generic;

public class PlatformManager : MonoBehaviour
{
	public Transform platform;  ///< 복제할 기본 바닥면
	internal Transform platformCopy;    ///< 복제 생성된 바닥면
	
	internal GameObject Player; ///< 캐릭터 게임 오브젝트
	
	public int numberOfObjects; ///< 최초 생성 바닥면 개수 (해당 개수로 재활용)
	public float recycleOffset; ///< 바닥면 재활용 카운트
	public Vector3[] startPosition; ///< 게임 시작 시 바닥면 시작 위치
	public Vector3[] startRotation; ///< 게임 시작 시 바닥면 시작 회전각
	public Vector3 minSize; ///< 바닥면 최소 사이즈
	public Vector3 maxSize; ///< 바닥면 최대 사이즈
	
	public Material roadMaterials;  ///< 일반 바닥면 메터리얼
	public Material[] trapMaterials;    ///< 함정 바닥면 메터리얼
	public float[] trapRegenPercent;    ///< 함정 생성 확률 (100분률)
	
	public static float platformDifficulty = 95;    ///< 바닥 타일 배치 난이도 (0 : Hard ~ 100 : Easy)
    public Transform[,] wallArray;  ///< 바닥면 관리 배열 (해당 배열내 바닥면 재활용)

	private Transform obj;  ///< 바닥면 재활용 시 사용하는 바닥면
	
	private int nNowWallIndex = 0;  ///< 현재 재활용되는 바닥면 index
	
	private float fQueuePosition = 0;   ///< 현재 바닥면(nNowWallIndex) z위치
	private Vector3[] nextPosition; ///< 바닥면 재활용 되서 배치되는 위치
	private Vector3[] nextRotation; ///< 바닥면 재활용 되서 배치되는 회전각

	private int turnFlag = 0;	///< 회전 방향 (1 : 좌회전 / 2 : 우회전)
	private int nowBottomLayer = 0;	///< 현재 바닥 레이어

	
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
		}

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
		fQueuePosition = wallArray[0, nNowWallIndex].localPosition.z;

		if(fQueuePosition + recycleOffset < Runner.distanceTraveled - 3)
		{
			// 벽면 재활용
			for(int i = 0; i < startPosition.Length; i++)
			{
				Recycle(i, nNowWallIndex);
			}
			
			nNowWallIndex++;
			
			if(nNowWallIndex == numberOfObjects)
			{
				nNowWallIndex = 0;
			}

			// 일정 거리 이동 시 난이도 상승 및 이동속도 상승
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
	
	/**
	@brief 벽면 재활용
	@date 2014/04/02
	@author SungMin Lee (bestdev@gameonstudio.co.kr)
	@param int _nWall (0 : Bottom, 1 : Left, 2 : Right, 3 : Top)
	@param int _nIndex (재활용 바닥면 index)
	@return void
	@see 
	@warning 
	*/
	private void Recycle(int _nWall, int _nIndex)
	{
		int nRnd = Random.Range(0, 100);
		
		// 난이도는 size 4짜리 면이 어느정도 나오느냐로 결정
		// 랜덤 난이도 예외처리 추가하여 존 구성 필요
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
		
		Vector3 position = nextPosition[_nWall];

		int nPosRnd = Random.Range(0, 4);
		
		obj = wallArray[_nWall, _nIndex];

		// 면 사이즈에 따른 좌표 보정
		WallPosCorrection(ref position, _nWall, fSizeRnd, nPosRnd);

		if(obj.gameObject.layer == 0)
		{
			if(_nWall == 0)
			{
				obj.gameObject.layer = LayerMask.NameToLayer("Bottom");
			}
			else if(_nWall == 1)
			{
				obj.gameObject.layer = LayerMask.NameToLayer("Left");
			}
			else if(_nWall == 2)
			{
				obj.gameObject.layer = LayerMask.NameToLayer("Right");
			}
			else if(_nWall == 3)
			{
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
		
		nextPosition[_nWall].z += scale.z;
	}

    /**
    @brief 벽면 재사용 시 사이즈에 따른 좌표 보정
    @date 2014/04/02
    @author SungMin Lee (bestdev@gameonstudio.co.kr)
    @param ref Vector3 _v3Pos
    @param int _nWall (0 : Bottom, 1 : Left, 2 : Right, 3 : Top)
    @param float _fSizeRnd (벽면 사이즈)
    @param int _nPosRnd (좌표보정 위치)
    @return void
    @see 
    @warning 
    */
    private void WallPosCorrection(ref Vector3 _v3Pos, int _nWall, float _fSizeRnd, int _nPosRnd)
	{
		float fCorrection = 0;

		// 면 사이즈에 따른 각각의 좌표 보정값 결정
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
			}
		}
		else if(_fSizeRnd == 3f)
		{
			if(_nPosRnd == 0 || _nPosRnd == 2)
				fCorrection = -0.5f;
			else if(_nPosRnd == 1 || _nPosRnd == 3)
				fCorrection = 0.5f;
		}

		// 회전 상태에 따른 x, y 좌표 보정
		if(turnFlag == 0)
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
		}
	}

	/**
	@brief 벽면 충돌 시 회전
	@date 2014/04/02
	@author SungMin Lee (bestdev@gameonstudio.co.kr)
	@param int[] _nLayer (0 : OldLayer, 1 : NewLayer)
	@return void
	@see 
	@warning 
	*/
	public void ApplyPlatTurn(int[] _nLayer)
	{
		Vector3 pos = Player.transform.localPosition;

		for(int i = 0; i < startPosition.Length; i++)
		{
			for(int k = 0; k < numberOfObjects; k++)
			{
				obj = wallArray[i, k];

				// 벽면 회전 시 캐릭터 위치에 따라서 y 좌표 이동하지 않고 고정 좌표로 보정
                // -1.7f : 캐릭터 중심점 위치
				pos.y = -1.7f;

				// 좌 / 우 회전에 따른 벽면 회전 처리
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
					nextPosition[i].x = obj.localPosition.x;
					nextPosition[i].y = obj.localPosition.y;
				}

				nowBottomLayer = _nLayer[1];
			}

			// +- 360도를 넘어갈 경우 보정하여 항상 0 ~ 360 사이 값으로 유지
			// 각도가 커지면 화면 출력 시 문제가 있을것 같은 느낌에 추가
			// 굳이 필요한 코드인지 모르겠음
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
	}
}