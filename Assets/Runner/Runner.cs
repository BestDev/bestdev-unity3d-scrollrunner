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

	private bool touchingTile = true;

	// Use this for initialization
	void Start ()
	{
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
				PlatformManager.platformDifficulty += 1;
			}
		}
		else if(Input.GetKeyUp(KeyCode.PageDown))
		{
			if(PlatformManager.platformDifficulty > 1)
			{
				PlatformManager.platformDifficulty -= 1;
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
		//Debug.Log(col.gameObject.name);

		//int nTrapCount = TileManager.staticTrapMaterials.Length;
		string strTile = col.gameObject.name;
		string strMaterial = col.gameObject.renderer.material.name;

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

		//Debug.Log(strMaterial);
		//Debug.Log(strMaterial == "Tile Fast Mat (Instance)");
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