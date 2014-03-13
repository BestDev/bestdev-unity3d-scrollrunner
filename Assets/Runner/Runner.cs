using UnityEngine;
using System.Collections;

public class Runner : MonoBehaviour
{
	public static int nScrollFlag = 0;	// 0 : 횡스크롤, 1 : 종스크롤
	public static float distanceTraveled;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(nScrollFlag == 0)
		{
			// 횡스크롤
			transform.Translate(5f * Time.deltaTime, 0f, 0f);

			distanceTraveled = transform.localPosition.x;
		}
		else
		{
			// 종스크롤
			transform.Translate(0f, 0f, 5f * Time.deltaTime);

			distanceTraveled = transform.localPosition.z;
		}
	}
}
