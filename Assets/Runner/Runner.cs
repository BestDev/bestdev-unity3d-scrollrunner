using UnityEngine;
using System.Collections;
using PlatManager;

public class Runner : MonoBehaviour
{
	public static float distanceTraveled;
	public Vector3 chrSize;
	public float runSpeed = 10;
	public float keySensitivity = 3;	// 1 : Fast, 3 : Slow
	public float jumpPower = 10;

	public GUIText guitext;

	private bool touchingTile = true;
	private int oldLayer = LayerMask.NameToLayer("Bottom");
	private Transform subCamera;
	private float oldRunSpeed = 10;

	// Use this for initialization
	void Start ()
	{
		subCamera = transform.FindChild("MainCamera");
		subCamera.transform.LookAt(transform.position);
	}
	
	// Update is called once per frame
	void Update () 
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

		// 페이지 업 / 다운 타일 난이도 설정 (1 Hard ~ 100 Easy)
		/*
		if(Input.GetKeyUp(KeyCode.PageUp))
		{
			if(TileManager.gapOfRandom < 10)
			{
				TileManager.gapOfRandom += 1;
			}
		}
		else if(Input.GetKeyUp(KeyCode.PageDown))
		{
			if(TileManager.gapOfRandom > 1)
			{
				TileManager.gapOfRandom -= 1;
			}
		}
		*/
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

		guitext.text = "runSpeed : " + runSpeed + " Difficulty : " + PlatformManager.platformDifficulty;

		// 좌우 키 입력
		float keySide = Input.GetAxis("Horizontal");
		transform.Translate(Vector3.right * (runSpeed / keySensitivity) * Time.deltaTime * keySide);
		
		// 종스크롤
		transform.Translate(0f, 0f, runSpeed * Time.deltaTime);
		if(touchingTile && Input.GetButtonDown("Jump"))
		{
            //rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange);
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
			touchingTile = false;
		}

		distanceTraveled = transform.localPosition.z;

		if(transform.position.y < -15)
		{
			runSpeed = 0; // Set the speed to 0

			Destroy(gameObject);
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		touchingTile = true;
		//Debug.Log("old : " + oldLayer + " now : " + LayerMask.LayerToName(col.gameObject.layer));

		//col.gameObject.layer.GetTypeCode
		if(col.gameObject.layer == LayerMask.NameToLayer("Bottom"))
		{
			//Debug.Log("Bot True" + oldLayer);
			if(oldLayer == LayerMask.NameToLayer("Left"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 7;	// LB
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, 90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, 90f);
				//LateUpdate();
				//camera.transform.rotation = Quaternion.Euler(0, 0, 90f);
			}
			else if(oldLayer == LayerMask.NameToLayer("Right"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 3;	// RB
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
			}
		}
		else if(col.gameObject.layer == LayerMask.NameToLayer("Left"))
		{
			Debug.Log("Left " + oldLayer);
			if(oldLayer == LayerMask.NameToLayer("Bottom"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 0;	// BL
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -90);//Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//subCamera.transform.Rotate();
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
			}
			else if(oldLayer == LayerMask.NameToLayer("Top"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 6;	// TL
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, 90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, 90f);
			}
		}
		else if(col.gameObject.layer == LayerMask.NameToLayer("Right"))
		{
			if(oldLayer == LayerMask.NameToLayer("Bottom"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 4;	// BR
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, 90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, 90f);
				//LateUpdate();
				//camera.transform.rotation = Quaternion.Euler(0, 0, 90f);
			}
			else if(oldLayer == LayerMask.NameToLayer("Top"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 2;	// TR
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
			}
		}
		else if(col.gameObject.layer == LayerMask.NameToLayer("Top"))
		{
			if(oldLayer == LayerMask.NameToLayer("Left"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 1;	// LT
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, -90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, -90f);
			}
			else if(oldLayer == LayerMask.NameToLayer("Right"))
			{
				oldRunSpeed = runSpeed;
				runSpeed = 0;
				PlatformManager.turnType = 5;	// RT
				runSpeed = oldRunSpeed;
				//subCamera.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Lerp(transform.rotation.z, 90, Time.deltaTime / 10.0f));
				//transform.rotation = Quaternion.Euler(0, 0, 90f);
			}
		}

		oldLayer = col.gameObject.layer;

		//string strTile = col.gameObject.name;
		string strMaterial = col.gameObject.renderer.material.name;

		/*
		if(strTile == "TileLeft Right(Clone)" || strTile == "TileBottom Right(Clone)" || strTile == "TileTop Right(Clone)")
		{
			//transform.rotation = Quaternion.Euler(0, 0, -90f);
			//LateUpdate();
			//camera.transform.rotation = Quaternion.Euler(0, 0, -90f);
		}

		if(strTile == "TileRight Left(Clone)" || strTile == "TileBottom Left(Clone)" || strTile == "TileTop Left(Clone)")
		{
			//transform.rotation = Quaternion.Euler(0, 0, 90f);
			//LateUpdate();
			//camera.transform.rotation = Quaternion.Euler(0, 0, 90f);
		}
		*/

		//Debug.Log(strMaterial);
		//Debug.Log(strMaterial == "Tile Fast Mat (Instance)");
		// 레이어 체크로 바꿔야 함
		if(strMaterial == "Tile Fast Mat (Instance)")
		{
			runSpeed += 1;
		}
		else if(strMaterial == "Tile Slow Mat (Instance)")
		{
			runSpeed -= 1;
		}
	}
}