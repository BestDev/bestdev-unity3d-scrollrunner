/**
 * @mainpage GOSRun
 * @section intro 소개
 * - GameOnStudio 모바일 프로젝트
 * - Unity3D로 제작한 3D 벽면회전 러닝 게임
 * @section DevelopmentEnvironment 개발환경
 * - OS : Windows7 x64
 * - Client Engine : Unity3D 4.3.1f C#
 * @section Info 프로젝트 정보
 * - 개발 기간 : 2014/03/24 ~ 2014/00/00
 * - 개발 인원 : 기획 1 / 서버 1 / 클라 1 / 원화,UI 1 / 애니메이션,이펙트 1
 * @section MODIFYINFO 수정정보
 * - 2014/04/02 (SungMin Lee) : Doxygen 주석 추가
 */

/**
 * @file Runner.cs
 * @date 2014/03/24
 * @author SungMin Lee(bestdev@gameonstudio.co.kr)
 * @brief 캐릭터 컨트롤 스크립트
 */


using UnityEngine;
using System.Collections;

public class Runner : MonoBehaviour
{
	public static float distanceTraveled;	///< 캐릭터 이동 거리
	public static int jumpCount = 0;	///< 캐릭터 점프 횟수
	public static int turnCount = 0;	///< 벽면 회전 횟수
	public static int pickItemCount = 0;	/// 아이템 획득 횟수

	public Vector3 chrSize;	///< 캐릭터 사이즈
	public float runSpeed = 5;	///< 기본 이동 속도
	public float keySensitivity = 3;	///< 좌우 키 감도 (1 : Fast ~ n : Slow)
	public float jumpPower = 7;	///< 캐릭터 점프 강도

	public float deadPosY = -20;	///< 캐릭터 사망 처리 y좌표
	public int touchType = 0;	///< 모바일 조작 방식 옵션 (0 : 터치유지 이동, 1 : 좌측 터치 이동 / 우측 터치 점프)
	
	public GUIText guitext;
	public GUIText guitext2;

	private bool touchingTile = true;	///< 캐릭터 바닥 충돌 여부 (공중부양 상태인지 체크)
	private int oldLayer = 8;	///< 벽면 충돌 시 이전 벽면 Layer
	private GameObject MainCamera;	///< 메인 카메라 오브젝트
	private float oldRunSpeed = 5;	///< 이전 캐릭터 이동 속도

	private Vector3 startPosition;	///< 캐릭터 게임 시작 위치
	private float fCameraRot = 0;	///< 카메라 회전 각도

#if UNITY_ANDROID
	private float fHorizontal = 0f;	///< 캐릭터 좌 / 우 이동 (x > 0 : 우측 / x < 0 : 좌측)
#endif

	// Use this for initialization
	void Start ()
	{
		MainCamera = GameObject.Find("MainCamera");
		GameEventManager.RunStart += RunStart;
		GameEventManager.RunEnd += RunEnd;

		startPosition = transform.localPosition;
		renderer.enabled = false;
		rigidbody.isKinematic = true;
		enabled = false;
	}

	private void RunStart()
	{
		// 게임 시작 / 재시작 시 초기화
		distanceTraveled = 0f;

		transform.localPosition = startPosition;
		MainCamera.transform.localRotation = Quaternion.Euler(20f,0,0);
		oldLayer = 8;
		fCameraRot = 0;
		jumpCount = 0;
		turnCount = 0;
		pickItemCount = 0;
		renderer.enabled = true;
		//runSpeed = 5; // Start speed to 5 // 재시작 시 이전 속도 유지
		rigidbody.isKinematic = false;
		enabled = true;
	}
	
	private void RunEnd()
	{
		renderer.enabled = false;
		rigidbody.isKinematic = true;
		enabled = false;
	}

	// Update is called once per frame
	void Update () 
	{
		// 종스크롤
		transform.Translate(0.0f, 0.0f, runSpeed * Time.deltaTime);
		distanceTraveled = transform.localPosition.z;

		// 마우스 스크롤 스피드 업 / 다운
		// if(Input.GetKeyUp(KeyCode.PageUp))
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			runSpeed += 1;
		}

		if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			runSpeed -= 1;
		}

		// 키보드 PageUp / PageDown 난이도 변경
		if (Input.GetKeyUp(KeyCode.PageUp))
		{
			if (PlatformManager.platformDifficulty < 100)
			{
				PlatformManager.platformDifficulty += 5;
			}
		}
		else if (Input.GetKeyUp(KeyCode.PageDown))
		{
			if (PlatformManager.platformDifficulty > 1)
			{
				PlatformManager.platformDifficulty -= 5;
			}
		}

		// 캐릭터 따라가는 카메라
		//MainCamera.transform.position = new Vector3(MainCamera.transform.position.x - (MainCamera.transform.position.x - transform.position.x) * 0.1f, transform.position.y + 1.7f, transform.position.z - 4);
		MainCamera.transform.position = new Vector3(MainCamera.transform.position.x - (MainCamera.transform.position.x - transform.position.x), transform.position.y + 1.7f, transform.position.z - 2.2f);

		// 캐릭터 점프 금지
		if (transform.position.y < -2.5f)
		{
			touchingTile = false;
		}

		// 캐릭터 사망 체크
		if (transform.position.y < deadPosY)
		{
			GameEventManager.TriggerRunEnd();
		}

		// 캐릭터 컨트롤
		RunnerControl();
	}

	/**
	@brief 캐릭터 벽면 충돌 시 처리
	@date 2014/04/03
	@author SungMin Lee (bestdev@gameonstudio.co.kr)
	@param Collision col (충돌면)
	@return void
	@see 
	@warning 
	*/
	//IEnumerator OnCollisionEnter(Collision col)
	public void OnCollisionEnter(Collision col)
	{
		guitext2.text = "wallpos : " + col.gameObject.transform.localPosition + " now bottom : " + LayerMask.LayerToName(col.gameObject.layer);
		//Debug.Log("col enter" + LayerMask.LayerToName(col.gameObject.layer) + " pos" + transform.localPosition);
		// 가끔 충돌 체크 못하고 빠져 죽는 문제 수정
		Vector3 locpos = transform.localPosition;
		if (locpos.y < -1.7f)
		{
			// 박스 캐릭터 일 경우
			locpos.y = -1.6f;
			transform.localPosition = locpos;
			//Debug.Log(locpos);
		}

		if(oldLayer != 0 && oldLayer != col.gameObject.layer && col.gameObject.layer != 0)
		{
			oldRunSpeed = runSpeed;
			//runSpeed = 0;
			
			int[] nLayer = {oldLayer, col.gameObject.layer};
			
			// 슬로우 효과
			//Time.timeScale = 0.5f;
			
			//col.gameObject.SendMessage("ApplyPlatTurn", nLayer, SendMessageOptions.DontRequireReceiver);
			GameObject.Find("PlatformManager").SendMessage("ApplyPlatTurn", nLayer, SendMessageOptions.RequireReceiver);
			//PlatformManager.turnLayer = nLayer;
			Vector3 fixPos = transform.localPosition;

			// 회전 이후 벽면 충돌로 인한 무한 회전 걸림 방지를 위해서 x좌표 보정
			if(nLayer[0] == LayerMask.NameToLayer("Bottom") && nLayer[1] == LayerMask.NameToLayer("Left")
			   || nLayer[0] == LayerMask.NameToLayer("Left") && nLayer[1] == LayerMask.NameToLayer("Top")
			   || nLayer[0] == LayerMask.NameToLayer("Top") && nLayer[1] == LayerMask.NameToLayer("Right")
			   || nLayer[0] == LayerMask.NameToLayer("Right") && nLayer[1] == LayerMask.NameToLayer("Bottom")
			   )
			{
				fixPos.x -= 0.05f;
				fixPos.y += 0.05f;
			}
			else if(nLayer[0] == LayerMask.NameToLayer("Bottom") && nLayer[1] == LayerMask.NameToLayer("Right")
			        || nLayer[0] == LayerMask.NameToLayer("Right") && nLayer[1] == LayerMask.NameToLayer("Top")
			        || nLayer[0] == LayerMask.NameToLayer("Top") && nLayer[1] == LayerMask.NameToLayer("Left")
			        || nLayer[0] == LayerMask.NameToLayer("Left") && nLayer[1] == LayerMask.NameToLayer("Bottom")
			        )
				
			{
				fixPos.x += 0.05f;
				fixPos.y += 0.05f;
			}

			transform.localPosition = fixPos;
			runSpeed = oldRunSpeed;

			// 슬로우 효과 원복
			//Time.timeScale = 1.0f;

			turnCount++;
		}

		oldLayer = col.gameObject.layer;

		string strMaterial = col.gameObject.renderer.material.name;

		// 함정면 충돌 시 캐릭터 이속 및 카메라 앵글 처리
		// 레이어 체크로 바꿔야 함
		if(strMaterial == "Tile Fast Mat (Instance)")
		{
			if(runSpeed < 12)
			{
				runSpeed += 1;
			}
		}
		else if(strMaterial == "Tile Slow Mat (Instance)")
		{
			if(runSpeed > 2)
			{
				runSpeed -= 1;
			}
		}
		else if(strMaterial == "Tile LeftRot Mat (Instance)")
		{
			fCameraRot += -90f;
			MainCamera.transform.localRotation = Quaternion.Euler(0,0,fCameraRot);
		}
		else if(strMaterial == "Tile RightRot Mat (Instance)")
		{
			fCameraRot += 90f;
			MainCamera.transform.localRotation = Quaternion.Euler(0,0,fCameraRot);
		}
		else if(strMaterial == "Tile TopRot Mat (Instance)")
		{
			fCameraRot += 180f;
			MainCamera.transform.localRotation = Quaternion.Euler(0,0,fCameraRot);
		}

		touchingTile = true;
	}

	/**
	@brief 캐릭터 컨트롤
	@date 2014/04/03
	@author SungMin Lee (bestdev@gameonstudio.co.kr)
	@return void
	@see 
	@warning 
	*/
	public void RunnerControl()
	{
		// 캐릭터 사이즈 설정
		Vector3 scale = new Vector3(
			Random.Range(chrSize.x, chrSize.x),
			Random.Range(chrSize.y, chrSize.y),
			Random.Range(chrSize.z, chrSize.z));
		
		transform.localScale = scale;

		guitext.text = "runSpeed : " + runSpeed + " Difficulty : " + PlatformManager.platformDifficulty + " locpos : " + transform.localPosition + " gbpos : " + transform.position;

#if UNITY_STANDALONE_WIN
		// 좌우 키 입력
		float keySide = Input.GetAxis("Horizontal");
		transform.Translate(Vector3.right * (runSpeed / keySensitivity) * Time.deltaTime * keySide);

		// PC 점프
		if (touchingTile && Input.GetButtonDown("Jump") && transform.localPosition.y > -2)
		{
			//rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange);
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
			touchingTile = false;
			jumpCount++;
		}
#endif

#if UNITY_ANDROID
		// 스마트폰 터치 좌우 이동
		// 조작 방식에 대한 추가 및 수정 필요
		// 0 : 터치유지 이동
		if (touchType == 0)
		{
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary)
			{
				Vector2 touchPosition = Input.GetTouch(0).position;

				Camera cam = Camera.main;

				//guitext.text = "touchpos " + touchPosition;

				if (touchPosition.x >= cam.pixelWidth / 2f)
				{
					fHorizontal = 1;
				}
				else if (touchPosition.x < cam.pixelWidth / 2f)
				{
					fHorizontal = -1;
				}

				// 이속에 따른 좌우 이동 처리 안하고 고정 값으로 처리
				//transform.Translate(Vector3.right * (runSpeed / keySensitivity) * Time.deltaTime * fHorizontal);
				transform.Translate(Vector3.right * 2 * Time.deltaTime * fHorizontal);
			}

			//guitext.text = "touchphase " + Input.GetTouch(0).phase + " tchTile " + touchingTile + " tchCnt " + Input.touchCount + " pos " + transform.localPosition;

			// 터치 점프
			if (touchingTile && Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Stationary && Input.GetTouch(1).phase == TouchPhase.Stationary) || (Input.GetTouch(0).phase == TouchPhase.Stationary && Input.GetTouch(1).phase == TouchPhase.Stationary))
			{
				if (transform.localPosition.y > -2f && transform.localPosition.y < -1f)
				{
					rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
					jumpCount++;
				}

				touchingTile = false;
			}
			/*
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
			{
				Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
				transform.Translate(Vector3.right * (runSpeed * 0.1f) * Time.deltaTime * touchDeltaPosition.x);
			}

			// y축 슬라이드 점프
			if (touchingTile && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
			{
				Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

				// 슬라이드 이동 거리 y축 5이상일 경우에만 점프
				if (touchDeltaPosition.y > 5)
				{
					rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
					touchingTile = false;
					jumpCount++;
				}
			}
			*/
		}
		// 1 : 좌측 터치 이동 / 우측 터치 점프
		else if (touchType == 1)
		{
			Camera cam = Camera.main;
			Vector2 touchPosition = Input.GetTouch(0).position;

			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
			{
				Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

				if (touchPosition.x < cam.pixelWidth / 2f)
				{
					if (touchDeltaPosition.x > 0)
					{
						fHorizontal = 1;
					}
					else if (touchDeltaPosition.x < 0)
					{
						fHorizontal = -1;
					}

					transform.Translate(Vector3.right * 3 * Time.deltaTime * fHorizontal);
				}
			}

			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary)
			{
				if (touchPosition.x < cam.pixelWidth / 2f)
				{
					transform.Translate(Vector3.right * 3 * Time.deltaTime * fHorizontal);
				}
			}

			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				if (touchPosition.x < cam.pixelWidth / 2f)
				{
					fHorizontal = 0;

					//transform.Translate(Vector3.right * 2 * Time.deltaTime * fHorizontal);
				}
			}

			//	//guitext.text = "tchpos " + touchPosition + " tchpos1 " + touchPosition1 + " cam.pixedwid " + cam.pixelWidth;
			//	//guitext.text = "tchpos " + touchPosition + " cam.pixedwid " + cam.pixelWidth;

			//guitext.text = "touchpos " + touchPosition + " touchpos1 " + touchPosition1;

			// 우측 터치 점프
			if (touchingTile && Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Began))
			{
				if (touchPosition.x >= cam.pixelWidth / 2f)
				{
					if (transform.localPosition.y > -2f && transform.localPosition.y < -1f)
					{
						rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
						jumpCount++;
					}

					touchingTile = false;
				}
			}

			touchPosition = Input.GetTouch(1).position;

			if (touchingTile && Input.touchCount > 0 && (Input.GetTouch(1).phase == TouchPhase.Began))
			{
				if (touchPosition.x >= cam.pixelWidth / 2f)
				{
					if (transform.localPosition.y > -2f && transform.localPosition.y < -1f)
					{
						rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
						jumpCount++;
					}

					touchingTile = false;
				}
			}
		}
#endif
	}

	public static void AddItem()
	{
		pickItemCount++;
		//Debug.Log(pickItemCount);
	}
}