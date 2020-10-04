using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
	public float delay;

	private void Start()
	{
		Invoke("Destroy", delay);
	}

	private void Destroy()
	{
		Destroy(gameObject);
	}
}
