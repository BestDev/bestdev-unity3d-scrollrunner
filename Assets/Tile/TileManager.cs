using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
	public Transform prefab;
	public int numberOfObjects;
	public float recycleOffset;
	public Vector3 startPosition;
	public Vector3 minSize, maxSize;
	//public int minRnd = 1, maxRnd;
	public static int gapOfRandom = 10;

	public Material roadMaterials;
	public Material[] trapMaterials;
	public float[] trapRegenPercent;

	public static Material[] staticTrapMaterials;
	public static float rotationZ;
	//public float[] trapSpeed;
	//public Material[] materials;
	//public PhysicMaterial[] physicMaterials;

	private Vector3 nextPosition;
	private Queue<Transform> objectQueue;

	private float fQueuePosition;
	private int nRnd;

	// Use this for initialization
	void Start ()
	{
		objectQueue = new Queue<Transform>(numberOfObjects);
		for(int i = 0; i < numberOfObjects; i++)
		{
			objectQueue.Enqueue((Transform)Instantiate(prefab));
		}

		nextPosition = startPosition;

		for(int i = 0; i < numberOfObjects; i++)
		{
			Recycle();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		/*
		if(Runner.nScrollFlag == 0)
		{
			fQueuePosition = objectQueue.Peek().localPosition.x;
		/*
		}
		else
		{
		*/
			fQueuePosition = objectQueue.Peek().localPosition.z;
		/*
		}
		*/

		if(fQueuePosition + recycleOffset < Runner.distanceTraveled -10)
		{
			Recycle();
		}
	}

	private void Recycle()
	{
		Vector3 scale = new Vector3(
			Random.Range(minSize.x, maxSize.x),
			Random.Range(minSize.y, maxSize.y),
			Random.Range(minSize.z, maxSize.z));

		Vector3 position = nextPosition;

		/*
		if(Runner.nScrollFlag == 0)
		{
			position.x += scale.x * 0.5f;
			position.y += scale.y * 0.5f;
		}
		else
		{
		*/
			position.z += scale.z * 0.5f;
			position.y += scale.y * 0.5f;
		/*
		}
		*/

		Transform o = objectQueue.Dequeue();
		o.localScale = scale;
		o.localPosition = position;

		int nRegenIndex = Random.Range(0, trapMaterials.Length);
		int nTrapPercent = Random.Range(0, 100);
		//Debug.Log("regen : " + nRegenIndex);
		//Debug.Log("percent : " + nTrapPercent);
		if(trapRegenPercent[nRegenIndex] > nTrapPercent)
		{
			o.renderer.material = trapMaterials[nRegenIndex];

		}
		else
		{
			o.renderer.material = roadMaterials;
		}

		nRnd = Random.Range(1, 100);
		//Debug.Log("Random : " + nRnd);

		nRnd = nRnd % gapOfRandom;

		//Debug.Log ("rnd % : " + nRnd);

		if(nRnd == 0)
		{
			nRnd = 2;
		}
		/*else if(nRnd < 2)
		{
			nRnd = 3;
		}*/
		else
		{
			nRnd = 1;
		}

		//Debug.Log ("Result : " + nRnd);

		/*
		if(Runner.nScrollFlag == 0)
		{
			nextPosition.x += (scale.x * nRnd);
		}
		else
		{
		*/
			nextPosition.z += (scale.z * nRnd);
		/*
		}
		*/
		/*
		Transform o = objectQueue.Dequeue();
		o.localPosition = nextPosition;
		
		if(Runner.nScrollFlag == 0)
		{
			nextPosition.x += o.localScale.x;
		}
		else
		{
			nextPosition.z += o.localScale.z;
		}
		*/

		objectQueue.Enqueue(o);
	}
}
