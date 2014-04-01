﻿using UnityEngine;
using System.Collections;

public class Runner : MonoBehaviour
{
	public static float distanceTraveled;
	public static int jumpCount = 0;
	public static int turnCount = 0;
	public Vector3 chrSize;
	public float runSpeed = 10;
	public float keySensitivity = 3;	// 1 : Fast, 3 : Slow
	public float jumpPower = 10;

	public float deadPosY = -25;

	public GUIText guitext;
	public GUIText guitext2;

	//public PlatformManager platMan;

	private bool touchingTile = true;
	private int oldLayer = 8;//LayerMask.NameToLayer("Bottom");
	private GameObject MainCamera;
	private float oldRunSpeed = 10;

	private Vector3 startPosition;
	private float fCameraRot = 0;

	// Use this for initialization
	void Start ()
	{
		MainCamera = GameObject.Find("MainCamera");
		GameEventManager.RunStart += RunStart;
		GameEventManager.RunEnd += RunEnd;

		startPosition = transform.localPosition;
		//Debug.Log("run st pos " + startPosition);
		renderer.enabled = false;
		rigidbody.isKinematic = true;
		enabled = false;
	}

	private void RunStart()
	{
		//Debug.Log("stpos " +startPosition);
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
		//runSpeed = 0; // Set the speed to 0
		renderer.enabled = false;
		rigidbody.isKinematic = true;
		enabled = false;
	}

	// Update is called once per frame
	void Update () 
	{
		Running();
	}

	public void OnCollisionEnter(Collision col)
	{
		//Debug.Log("old : " + oldLayer + " now : " + LayerMask.LayerToName(col.gameObject.layer));
		guitext2.text = "wallpos : " + col.gameObject.transform.localPosition + " now bottom : " + LayerMask.LayerToName(col.gameObject.layer);

		if(oldLayer != 0 && oldLayer != col.gameObject.layer && col.gameObject.layer != 0)
		{
			oldRunSpeed = runSpeed;
			runSpeed = 0;
			
			int[] nLayer = {oldLayer, col.gameObject.layer};
			//platMan.ApplyPlatTurn(nLayer);
			//col.gameObject.SendMessage("ApplyPlatTurn", nLayer, SendMessageOptions.DontRequireReceiver);
			GameObject.Find("PlatformManager").SendMessage("ApplyPlatTurn", nLayer, SendMessageOptions.RequireReceiver);
			Vector3 fixPos = transform.localPosition;

			// 회전 이후 계속 무한 회전 걸림 방지를 위해서 x좌표 보정
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

		/*
		if(oldLayer == LayerMask.NameToLayer("Bottom"))
		{
			if(col.gameObject.layer == LayerMask.NameToLayer("Left"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 0;	// BL
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -90);//Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//subCamera.transform.Rotate();
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
				Vector3 tempTran = transform.localPosition;
				//tempTran.x += 3.0f;
				
				transform.localPosition = tempTran;
			}
			else if(col.gameObject.layer == LayerMask.NameToLayer("Right"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 4;	// BR
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, 90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, 90f);
				//LateUpdate();
				//camera.transform.rotation = Quaternion.Euler(0, 0, 90f);
				Vector3 tempTran = transform.localPosition;
				//tempTran.x -= 3.0f;
				
				transform.localPosition = tempTran;
			}
		}
		else if(oldLayer == LayerMask.NameToLayer("Left"))
		{
			if(col.gameObject.layer == LayerMask.NameToLayer("Top"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 1;	// LT
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
				Vector3 tempTran = transform.localPosition;
				//tempTran.x += 3.0f;
				
				transform.localPosition = tempTran;
			}
			else if(col.gameObject.layer == LayerMask.NameToLayer("Bottom"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 7;	// LB
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, 90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, 90f);
				//LateUpdate();
				//camera.transform.rotation = Quaternion.Euler(0, 0, 90f);
				Vector3 tempTran = transform.localPosition;
				//tempTran.x -= 3.0f;
				
				transform.localPosition = tempTran;
			}
			
		}
		else if(oldLayer == LayerMask.NameToLayer("Top"))
		{
			if(col.gameObject.layer == LayerMask.NameToLayer("Right"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 2;	// TR
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
				Vector3 tempTran = transform.localPosition;
				//tempTran.x += 3.0f;
				
				transform.localPosition = tempTran;
			}
			else if(col.gameObject.layer == LayerMask.NameToLayer("Left"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 6;	// TL
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, 90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, 90f);
				Vector3 tempTran = transform.localPosition;
				//tempTran.x -= 3.0f;
				
				transform.localPosition = tempTran;
			}
		}
		else if(oldLayer == LayerMask.NameToLayer("Right"))
		{
			if(col.gameObject.layer == LayerMask.NameToLayer("Bottom"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 3;	// RB
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
				Vector3 tempTran = transform.localPosition;
				//tempTran.x += 3.0f;
				
				transform.localPosition = tempTran;
			}
			else if(col.gameObject.layer == LayerMask.NameToLayer("Top"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 5;	// RT
				//System.Threading.Thread.Sleep(500);
				runSpeed = oldRunSpeed;		
				
				Vector3 tempTran = transform.localPosition;
				//tempTran.x -= 3.0f;
				
				transform.localPosition = tempTran;
			}
		}
		*/

		oldLayer = col.gameObject.layer;

		//string strTile = col.gameObject.name;
		string strMaterial = col.gameObject.renderer.material.name;

		//float fCameraRot = MainCamera.transform.localRotation.z;
		//Debug.Log("mat " + strMaterial + " cp " + fCameraRot);
		//Debug.Log(strMaterial == "Tile Fast Mat (Instance)");
		// 레이어 체크로 바꿔야 함
		//Debug.Log(fCameraRot);
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
			//Debug.Log("next " + fCameraRot);
			MainCamera.transform.localRotation = Quaternion.Euler(0,0,fCameraRot);
		}
		else if(strMaterial == "Tile RightRot Mat (Instance)")
		{
			fCameraRot += 90f;
			//Debug.Log("next " + fCameraRot);
			MainCamera.transform.localRotation = Quaternion.Euler(0,0,fCameraRot);
		}
		else if(strMaterial == "Tile TopRot Mat (Instance)")
		{
			fCameraRot += 180f;
			//Debug.Log("next " + fCameraRot);
			MainCamera.transform.localRotation = Quaternion.Euler(0,0,fCameraRot);
		}

		touchingTile = true;
	}

	public void Running()
	{
		// 캐릭터 사이즈 설정
		Vector3 scale = new Vector3(
			Random.Range(chrSize.x, chrSize.x),
			Random.Range(chrSize.y, chrSize.y),
			Random.Range(chrSize.z, chrSize.z));
		
		transform.localScale = scale;
		
		// 마우스 스크롤 스피드 업 / 다운
		// if(Input.GetKeyUp(KeyCode.PageUp))
		if(Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			runSpeed += 1;
		}
		
		// if(Input.GetKeyUp(KeyCode.mou.PageDown))
		if(Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			runSpeed -= 1;
		}
		
		if(Input.GetKeyUp(KeyCode.PageUp))
		{
			if(PlatformManager.platformDifficulty < 100)
			{
				PlatformManager.platformDifficulty += 5;
			}
		}
		else if(Input.GetKeyUp(KeyCode.PageDown))
		{
			if(PlatformManager.platformDifficulty > 1)
			{
				PlatformManager.platformDifficulty -= 5;
			}
		}
		
		guitext.text = "runSpeed : " + runSpeed + " Difficulty : " + PlatformManager.platformDifficulty + " locpos : " + transform.localPosition + " gbpos : " + transform.position;
		
		// 좌우 키 입력
		float keySide = Input.GetAxis("Horizontal");
		transform.Translate(Vector3.right * (runSpeed / keySensitivity) * Time.deltaTime * keySide);
		
		// 스마트폰 터치 좌우 이동
		if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
			transform.Translate(Vector3.right * touchDeltaPosition.x * Time.deltaTime * (runSpeed / keySensitivity * 0.1f));
		}
		
		// 종스크롤
		transform.Translate(0.0f, 0.0f, runSpeed * Time.deltaTime);
		if(touchingTile && Input.GetButtonDown("Jump") && transform.localPosition.y > -2)
		{
			//rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange);
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
			touchingTile = false;
			jumpCount++;
		}
		
		distanceTraveled = transform.localPosition.z;
		
		if(transform.position.y < deadPosY)
		{
			GameEventManager.TriggerRunEnd();
		}
		
		// 캐릭터 따라가는 카메라
		//MainCamera.transform.position = new Vector3(MainCamera.transform.position.x - (MainCamera.transform.position.x - transform.position.x) * 0.1f, transform.position.y + 1.7f, transform.position.z - 4);
		MainCamera.transform.position = new Vector3(MainCamera.transform.position.x - (MainCamera.transform.position.x - transform.position.x), transform.position.y + 1.7f, transform.position.z - 4);
	}
}