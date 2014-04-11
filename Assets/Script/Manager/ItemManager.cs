using UnityEngine;
using System.Collections;

public class ItemManager : MonoBehaviour
{
	public Vector3 rotVelocity;
	public float rycycleOffset;

	public float timerForDel = 0;	///< 오브젝트 관리 타이머
	public float timerForDelLimit = 60;	///< 오브젝트 생성 후 삭제할 시간

	// Use this for initialization
	void Start ()
	{
		//GameEventManager.RunEnd += RunEnd;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//if (transform.localPosition.z + rycycleOffset < Runner.distanceTraveled - 3)
		//{
		//	return;
		//}

		transform.Rotate(rotVelocity * Time.deltaTime);

		timerForDel += Time.deltaTime;
		if (timerForDel > timerForDelLimit)
		{
			//Destroy(gameObject);
		}
	}

	void OnTriggerEnter()
	{
		Runner.AddItem();

		Destroy(gameObject);
	}

	//private void RunEnd()
	//{
	//}
}
