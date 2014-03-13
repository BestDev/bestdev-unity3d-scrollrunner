using UnityEngine;
using System.Collections.Generic;

public class SkylineManager : MonoBehaviour
{
	public Transform prefab;
	public int numberOfObjects;
	public float recycleOffset;
	public Vector3 startPosition;
	public Vector3 minSize, maxSize;

	private Vector3 nextPosition;
	private Queue<Transform> objectQueue;

	private float fQueuePosition;

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
		if(Runner.nScrollFlag == 0)
		{
			fQueuePosition = objectQueue.Peek().localPosition.x;
		}
		else
		{
			fQueuePosition = objectQueue.Peek().localPosition.z;
		}

		if(fQueuePosition + recycleOffset < Runner.distanceTraveled)
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

		if(Runner.nScrollFlag == 0)
		{
			position.x += scale.x * 0.5f;
			position.y += scale.y * 0.5f;
		}
		else
		{
			position.z += scale.z * 0.5f;
		}

		Transform o = objectQueue.Dequeue();
		o.localScale = scale;
		o.localPosition = position;

		if(Runner.nScrollFlag == 0)
		{
			nextPosition.x += scale.x;
		}
		else
		{
			nextPosition.z += scale.z;
		}
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
