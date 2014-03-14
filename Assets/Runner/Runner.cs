using UnityEngine;
using System.Collections;

public class Runner : MonoBehaviour
{
	public static int nScrollFlag = 1;	// 0 : 횡스크롤, 1 : 종스크롤
	public static float distanceTraveled;
	public Vector3 chrSize;
	public float runSpeed = 30;

	public float acceleration = 30;
	public Vector3 jumpVelocity;

	public GUIText guitext;

	private bool touchingTile;

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
			runSpeed += 10;
			acceleration += 10;
		}

		// if(Input.GetKeyUp(KeyCode.mou.PageDown))
		if(Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			runSpeed -= 10;
			acceleration -= 10;
		}

		//Debug.Log("RunSpeed : " + runSpeed);
		//guitext.text = "RunSpeed : " + runSpeed;
		guitext.text = "acceleration : " + acceleration;

		// 페이지 업 / 다운 타일 난이도 설정 (1 ~ 10)
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

		// 좌우 키 입력
		float keySide = Input.GetAxis("Horizontal");
		transform.Translate(Vector3.right * (runSpeed / 2) * Time.deltaTime * keySide);
		
		//Debug.Log("gapOfRandom : " + TileManager.gapOfRandom);

		/*
		if(nScrollFlag == 0)
		{
			// 횡스크롤
			transform.Translate(runSpeed * Time.deltaTime, 0f, 0f);

			distanceTraveled = transform.localPosition.x;
		}
		else
		{
		*/
			// 종스크롤
			transform.Translate(0f, 0f, runSpeed * Time.deltaTime);
		if(touchingTile && Input.GetButton("Jump"))
		{
			rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange);
			touchingTile = false;
		}

		distanceTraveled = transform.localPosition.z;
		/*
		}
		*/
	}

	void FixedUpdate()
	{
		if(touchingTile)
		{
			//rigidbody.AddForce(0f, 0f, acceleration, ForceMode.Acceleration);
			//Debug.Log("뭐임?");
		}
	}

	void OnCollisionEnter()
	{
		touchingTile = true;
		//Debug.Log("true");
	}

	void OnCollisionExit()
	{
		touchingTile = false;
		//Debug.Log("false");
	}
}