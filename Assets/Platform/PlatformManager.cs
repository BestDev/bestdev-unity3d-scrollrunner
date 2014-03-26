using UnityEngine;
using System.Collections.Generic;

namespace PlatManager
{
	public class PlatformManager : MonoBehaviour
	{
		public Transform platform;
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
		private Queue<Transform> objectQueueBottom;
		private Queue<Transform> objectQueueLeft;
		private Queue<Transform> objectQueueRight;
		private Queue<Transform> objectQueueTop;
		
		private float fQueuePosition;
		private Vector3[] nextPosition;
		private float fTurn;
		private float fQueueTemp;

		// Use this for initialization
		void Start ()
		{
			objectQueueBottom = new Queue<Transform>(numberOfObjects);
			objectQueueLeft = new Queue<Transform>(numberOfObjects);
			objectQueueRight = new Queue<Transform>(numberOfObjects);
			objectQueueTop = new Queue<Transform>(numberOfObjects);

			nextPosition = new Vector3[startPosition.Length];
			//Debug.Log(startPosition.Length);

			for(int i = 0; i < numberOfObjects; i++)
			{
				objectQueueBottom.Enqueue((Transform)Instantiate(platform));
				objectQueueLeft.Enqueue((Transform)Instantiate(platform));
				objectQueueRight.Enqueue((Transform)Instantiate(platform));
				objectQueueTop.Enqueue((Transform)Instantiate(platform));
			}

			for(int i = 0; i < startPosition.Length; i++)
			{
				nextPosition[i] = startPosition[i];
			}
			
			for(int i = 0; i < numberOfObjects; i++)
			{
				// 0 : Bottom, 1 : Left, 2 : Right, 3 : Top
				Recycle(0);
				Recycle(1);
				Recycle(2);
				Recycle(3);
			}
		}

		// Update is called once per frame
		void Update ()
		{
			//Debug.Log("pm update");
			fQueuePosition = objectQueueBottom.Peek().localPosition.z;

			if(fQueuePosition + recycleOffset < Runner.distanceTraveled - 4)
			{
				Recycle(0);
                fQueuePosition = objectQueueLeft.Peek().localPosition.z;
				Recycle(1);
                fQueuePosition = objectQueueRight.Peek().localPosition.z;
				Recycle(2);
                fQueuePosition = objectQueueTop.Peek().localPosition.z;
				Recycle(3);
			}

			if(turnType != -1)
			{
				for(int i = 0; i < startPosition.Length; i++)
				{
					PlatformTurn(i);
				}
				/*
				for(int i = 0; i < numberOfObjects; i++)
				{
					fQueuePosition = objectQueueBottom.Peek().localPosition.z;
					PlatformTurn(0);
				}

				for(int i = 0; i < numberOfObjects; i++)
				{
					fQueuePosition = objectQueueLeft.Peek().localPosition.z;
					PlatformTurn(1);
				}

				for(int i = 0; i < numberOfObjects; i++)
				{
					fQueuePosition = objectQueueRight.Peek().localPosition.z;
					PlatformTurn(2);
				}

				for(int i = 0; i < numberOfObjects; i++)
				{
					fQueuePosition = objectQueueTop.Peek().localPosition.z;
					PlatformTurn(3);
				}
				*/

				turnType = -1;
			}
		}

		private void Recycle(int _nWall)
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
			float fHeightRnd = (int)Random.Range(minSize.x, 2);

			Vector3 scale = new Vector3(fSizeRnd, minSize.y, fHeightRnd);
				//Random.Range(minSize.y, maxSize.y),
				//Random.Range(minSize.z, maxSize.z));

			//Debug.Log("min : " + minSize.x + " max : " + maxSize.x);

			Vector3 position = nextPosition[_nWall];

			//position.z += scale.z * 0.5f;
			//position.y += scale.y * 0.5f;
			int nPosRnd = Random.Range(0, 4);

			WallPosCorrection(ref position, _nWall, fSizeRnd, nPosRnd);

			if(_nWall == 0)
			{
				//Debug.Log("cnt : " + cnt + " " + LayerMask.LayerToName(gameObject.layer));
				obj = objectQueueBottom.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Bottom");
			}
			else if(_nWall == 1)
			{
				//Debug.Log("cnt : " + cnt + " " + LayerMask.LayerToName(gameObject.layer));
				obj = objectQueueLeft.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Left");
			}
			else if(_nWall == 2)
			{
				obj = objectQueueRight.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Right");
			}
			else if(_nWall == 3)
			{
				obj = objectQueueTop.Dequeue();
				obj.gameObject.layer = LayerMask.NameToLayer("Top");
			}

			obj.localScale = scale;
			obj.localPosition = position;
			obj.localRotation = Quaternion.Euler(startRotation[_nWall]);

			//Debug.Log("pos : " + nextPosition[_nWall].z + " Wall : " + _nWall + "rot : " + startRotation[_nWall]);
			//Debug.Log("pos : " + nextPosition[_nWall] + " Wall : " + _nWall + "rot : " + startRotation[_nWall]);

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

			nextPosition[_nWall].z += scale.z;

			if(_nWall == 0)
			{
				objectQueueBottom.Enqueue(obj);
			}
			else if(_nWall == 1)
			{
				objectQueueLeft.Enqueue(obj);
			}
			else if(_nWall == 2)
			{
				objectQueueRight.Enqueue(obj);
			}
			else if(_nWall == 3)
			{
				objectQueueTop.Enqueue(obj);
			}
		}

		private void WallPosCorrection(ref Vector3 _v3Pos, int _nWall, float _fSizeRnd, int _nPosRnd)
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
					fCorrection = (float)Random.Range(-1, 2);
					//Debug.Log(fCorrection);
				}
					// 3이 걸릴 경우 기존 꺼 사용 (확률적으로 이게 더 나을듯)
					//position.x = 0f;
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
			/*
			switch(_nPosRnd)
			{
			case 0:
			{
				if(_fSizeRnd == 1.0f || _fSizeRnd == 3.0f)
				{
					fCorrection = -0.5f
				}
				else if(_fSizeRnd == 2.0f)
				{
					fCorrection = 0f
				}
			}
				break;
			case 1:
			{
				if(_fSizeRnd == 1.0f || _fSizeRnd == 3.0f)
				{
					fCorrection = 0.5f
				}
				else if(_fSizeRnd == 2.0f)
				{
					fCorrection = -1f
				}
			}
				break;
			case 2:
			{
				if(_fSizeRnd == 1.0f)
				{
					fCorrection = -1.5f
				}
				else if(_fSizeRnd == 2.0f)
				{
					fCorrection = 1f
				}
				else if(_fSizeRnd == 3.0f)
				{
					fCorrection = -0.5f
				}
			}
				break;
			case 3:
			{
				if(_fSizeRnd == 1.0f)
				{
					fCorrection = 1.5f
				}
				else if(_fSizeRnd == 2.0f)
				{
					fCorrection = 0f
				}
				else if(_fSizeRnd == 3.0f)
				{
					fCorrection = 0.5f
				}
			}
				break;
			default:
				break;
			}
			*/
			if(_nWall == 0 || _nWall == 3)
			{
				_v3Pos.x = fCorrection;
				//if(_fSizeRnd != 4.0f)
				//	Debug.Log("size : " + _fSizeRnd + " pos : " + _v3Pos);
			}
			else if(_nWall == 1 || _nWall == 2)
			{
				_v3Pos.y = fCorrection;
			}
		}

		public void PlatformTurn(int _nWall)
		{
			for(int k = 0; k < numberOfObjects; k++)
			{
				// 0:BL, 1:LT, 2:TR, 3:RB, 4:BR, 5:RT, 6:TL, 7:LB
				if(_nWall == 0)
				{
					fQueueTemp = objectQueueBottom.Peek().localPosition.z;
					//Debug.Log("cnt : " + cnt + " " + LayerMask.LayerToName(gameObject.layer));
					obj = objectQueueBottom.Dequeue();

					fTurn = 90f * turnAngle * Time.deltaTime;

					if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// BL (B->R)
					{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x += 2f;
						pos.y += 2f;

						obj.localRotation = Quaternion.Euler(0, 0, 90f);
						obj.localPosition = pos;
					}
					else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// BR (B->L)
					{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x -= 2f;
						pos.y += 2f;

						obj.localRotation = Quaternion.Euler(0, 0, 270f);
						obj.localPosition = pos;
					}

					objectQueueBottom.Enqueue(obj);
				}
				else if(_nWall == 1)
				{
					fQueueTemp = objectQueueLeft.Peek().localPosition.z;
					//Debug.Log("cnt : " + cnt + " " + LayerMask.LayerToName(gameObject.layer));
					obj = objectQueueLeft.Dequeue();
					if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// LT (L->B)
					{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x += 2f;
						pos.y -= 2f;
						
						obj.localRotation = Quaternion.Euler(0, 0, 0f);
						obj.localPosition = pos;
	              	}
					else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// LB (L->T)
	              	{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x += 2f;
						pos.y += 2f;

						obj.localRotation = Quaternion.Euler(0, 0, 180f);
						obj.localPosition = pos;
					}
					objectQueueLeft.Enqueue(obj);
				}
				else if(_nWall == 2)
				{
					fQueueTemp = objectQueueRight.Peek().localPosition.z;
					obj = objectQueueRight.Dequeue();

					if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// RB (R->T)
					{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x -= 2f;
						pos.y += 2f;
						
						obj.localRotation = Quaternion.Euler(0, 0, 180f);
						obj.localPosition = pos;
					}
					else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// RT (R->B)
					{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x -= 2f;
						pos.y -= 2f;
						
						obj.localRotation = Quaternion.Euler(0, 0, 0f);
						obj.localPosition = pos;
					}

					objectQueueRight.Enqueue(obj);
				}
				else if(_nWall == 3)
				{
					fQueueTemp = objectQueueTop.Peek().localPosition.z;
					obj = objectQueueTop.Dequeue();

					if(turnType == 0 || turnType == 1 || turnType == 2 || turnType == 3)	// TR (T->L)
					{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x -= 2f;
						pos.y -= 2f;
						
						obj.localRotation = Quaternion.Euler(0, 0, 270f);
						obj.localPosition = pos;
					}
					else if(turnType == 4 || turnType == 5 || turnType == 6 || turnType == 7)	// TL (T->R)
					{
						Debug.Log("Turn Type " + turnType);
						Vector3 pos = obj.localPosition;
						pos.x += 2f;
						pos.y -= 2f;
						
						obj.localRotation = Quaternion.Euler(0, 0, 90f);
						obj.localPosition = pos;
					}

					objectQueueTop.Enqueue(obj);
				}
			}
			/*
			switch(_nWall)
			{
			case 0:
				TurnCorrection(turnType);
				break;
			case 1:	// Bottom -> Left || Left -> Top || Top -> Right || Right -> Bottom
				fTurn = 90f * turnAngle * Time.deltaTime;
				//platform.transform.Rotate(Vector3(0, 0, fTurn));
				//platform.transform.position(platform.transform.position.x + 2, platform.transform.position.y - 2, platform.transform.position.z);
				break;
			case 2:	// Bottom -> Right || Right -> Top || Top -> Left || Left -> Bottom
				fTurn = -90f * turnAngle * Time.deltaTime;
				//platform.transform.Rotate(Vector3(0, 0, fTurn));
				//platform.transform.position(Vector3(platform.transform.position.x - 2, platform.transform.position.y - 2, platform.transform.position.z));
				break;
			case 3:
				break;
			default:
				break;
			}
			*/
		}
	}
}