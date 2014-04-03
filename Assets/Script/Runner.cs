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
	public Vector3 chrSize;	///< 캐릭터 사이즈
	public float runSpeed = 5;	///< 기본 이동 속도
	public float keySensitivity = 3;	///< 좌우 키 감도 (1 : Fast ~ n : Slow)
	public float jumpPower = 10;	///< 캐릭터 점프 강도

	public float deadPosY = -20;	///< 캐릭터 사망 처리 y좌표

	public GUIText guitext;
	public GUIText guitext2;

	private bool touchingTile = true;	///< 캐릭터 바닥 충돌 여부 (공중부양 상태인지 체크)
	private int oldLayer = 8;	///< 벽면 충돌 시 이전 벽면 Layer
	private GameObject MainCamera;	///< 메인 카메라 오브젝트
	private float oldRunSpeed = 5;	///< 이전 캐릭터 이동 속도

	private Vector3 startPosition;	///< 캐릭터 게임 시작 위치
	private float fCameraRot = 0;	///< 카메라 회적 각도

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
		MainCamera.transform.localRotation = Quaternion.Euler(0,0,0);
		oldLayer = 8;
		fCameraRot = 0;
		jumpCount = 0;
		turnCount = 0;
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

		// 캐릭터 컨트롤
		RunnerControl();

		// 캐릭터 사망 체크
		if (transform.position.y < deadPosY)
		{
			GameEventManager.TriggerRunEnd();
		}
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
	public void OnCollisionEnter(Collision col)
	{
		guitext2.text = "wallpos : " + col.gameObject.transform.localPosition + " now bottom : " + LayerMask.LayerToName(col.gameObject.layer);

		if(oldLayer != 0 && oldLayer != col.gameObject.layer && col.gameObject.layer != 0)
		{
			oldRunSpeed = runSpeed;
			runSpeed = 0;
			
			int[] nLayer = {oldLayer, col.gameObject.layer};
			//col.gameObject.SendMessage("ApplyPlatTurn", nLayer, SendMessageOptions.DontRequireReceiver);
			GameObject.Find("PlatformManager").SendMessage("ApplyPlatTurn", nLayer, SendMessageOptions.RequireReceiver);
			Vector3 fixPos = transform.localPosition;

			// 회전 이후 벽면 충돌로 인한 무한 회전 걸림 방지를 위해서 x좌표 보정
			if(nLayer[0] == LayerMask.NameToLayer("Bottom") && nLayer[1] == LayerMask.NameToLayer("Left")
			   || nLayer[0] == LayerMask.NameToLayer("Left") && nLayer[1] == LayerMask.NameToLayer("Top")
			   || nLayer[0] == LayerMask.NameToLayer("Top") && nLayer[1] == LayerMask.NameToLayer("Right")
			   || nLayer[0] == LayerMask.NameToLayer("Right") && nLayer[1] == LayerMask.NameToLayer("Bottom")
			   )
			{
				fixPos.x -= 0.01f;
			}
			else if(nLayer[0] == LayerMask.NameToLayer("Bottom") && nLayer[1] == LayerMask.NameToLayer("Right")
			        || nLayer[0] == LayerMask.NameToLayer("Right") && nLayer[1] == LayerMask.NameToLayer("Top")
			        || nLayer[0] == LayerMask.NameToLayer("Top") && nLayer[1] == LayerMask.NameToLayer("Left")
			        || nLayer[0] == LayerMask.NameToLayer("Left") && nLayer[1] == LayerMask.NameToLayer("Bottom")
			        )
				
			{
				fixPos.x += 0.01f;
			}

			transform.localPosition = fixPos;
			runSpeed = oldRunSpeed;

			turnCount++;
		}

		oldLayer = col.gameObject.layer;

		string strMaterial = col.gameObject.renderer.material.name;

		// 함정면 충돌 시 캐릭터 이속 및 카메라 앵글 처리
		// 레이어 체크로 바꿔야 함
		if(strMaterial == "Tile Fast Mat (Instance)")
		{
			if(runSpeed < 15)
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
		
		// 좌우 키 입력
		float keySide = Input.GetAxis("Horizontal");
		transform.Translate(Vector3.right * (runSpeed / keySensitivity) * Time.deltaTime * keySide);
		
		// 스마트폰 터치 좌우 이동
		// 조작 방식에 대한 추가 및 수정 필요
		if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
			transform.Translate(Vector3.right * touchDeltaPosition.x * Time.deltaTime * (runSpeed / keySensitivity * 0.1f));
		}
		
		if(touchingTile && Input.GetButtonDown("Jump") && transform.localPosition.y > -2)
		{
			//rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange);
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
			touchingTile = false;
			jumpCount++;
		}
		
		// 캐릭터 따라가는 카메라
		//MainCamera.transform.position = new Vector3(MainCamera.transform.position.x - (MainCamera.transform.position.x - transform.position.x) * 0.1f, transform.position.y + 1.7f, transform.position.z - 4);
		MainCamera.transform.position = new Vector3(MainCamera.transform.position.x - (MainCamera.transform.position.x - transform.position.x), transform.position.y + 1.7f, transform.position.z - 4);
	}
}