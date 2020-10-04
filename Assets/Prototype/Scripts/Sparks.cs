using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Sparks : MonoBehaviour
{
	public float lifeTime;
	[Space]
	new public ParticleSystem particleSystem;	
	public MinMaxGradient envShotColor;
	public MinMaxGradient bodyShotColor;
	public MinMaxGradient headShotColor;

	private List<Sparks> pool;

	public void Activate(List<Sparks> pool)
	{
		this.pool = pool;
		gameObject.SetActive(true);
		Invoke("Disable", lifeTime);
	}

	private void Disable()
	{
		gameObject.SetActive(false);
		pool.Add(this);
	}
}
