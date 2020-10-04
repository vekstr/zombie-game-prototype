using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
	public Transform target;
	public bool unparentAfterInit;

	private Vector3 offset;

	private void Start()
	{
		offset = transform.position - target.position;

		if (unparentAfterInit)
		{
			transform.SetParent(null);
		}
	}

	private void LateUpdate()
	{
		transform.position = target.position + offset;
	}
}
