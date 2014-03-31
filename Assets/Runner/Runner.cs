using UnityEngine;
using System.Collections;

public class Runner : MonoBehaviour
{
	public static float distanceTraveled;
	public Vector3 chrSize;
	public float runSpeed = 10;
	public float keySensitivity = 3;	// 1 : Fast, 3 : Slow
	public float jumpPower = 10;

	public GUIText guitext;
	public GUIText guitext2;

	//public PlatformManager platMan;

	private bool touchingTile = true;
	private int oldLayer = 8;//LayerMask.NameToLayer("Bottom");
	private Transform subCamera;
	private float oldRunSpeed = 10;

	// Use this for initialization
	void Start ()
	{
		//subCamera = transform.FindChild("MainCamera");
		//subCamera.transform.LookAt(transform);
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

		guitext.text = "runSpeed : " + runSpeed + " Difficulty : " + PlatformManager.platformDifficulty + " pos : " + transform.localPosition;

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
		transform.Translate(0f, 0f, runSpeed * Time.deltaTime);
		if(touchingTile && Input.GetButtonDown("Jump") && transform.localPosition.y > -2)
		{
            //rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange);
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
			touchingTile = false;
		}

		distanceTraveled = transform.localPosition.z;

		if(transform.position.y < -25)
		{
			runSpeed = 0; // Set the speed to 0

			Destroy(gameObject);
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		touchingTile = true;
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

		//Debug.Log(strMaterial);
		//Debug.Log(strMaterial == "Tile Fast Mat (Instance)");
		// 레이어 체크로 바꿔야 함
		if(strMaterial == "Tile Fast Mat (Instance)")
		{
			if(runSpeed < 10)
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
	}
}