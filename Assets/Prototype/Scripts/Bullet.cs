using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public Rigidbody body;
	public float speed = 50f;
	public int damage = 40;

	private List<Bullet> bulletsPool;
	private List<Sparks> sparksPool;

	public void Activate(List<Bullet> bulletsPool, List<Sparks> sparksPool)
	{
		this.sparksPool = sparksPool;
		this.bulletsPool = bulletsPool;
		bulletsPool.Remove(this);

		gameObject.SetActive(true);
		body.velocity = transform.forward * speed;

		Invoke("ReturnToPool", 10);
	}

	private void ReturnToPool()
	{
		body.velocity = Vector3.zero;
		gameObject.SetActive(false);
		bulletsPool.Add(this);
	}

	private void OnCollisionEnter(Collision collision)
	{
		var sparks = sparksPool[0];
		var sparksParticleMain = sparks.particleSystem.main;
		sparks.transform.position = collision.GetContact(0).point;

		var colliderObject = collision.collider.gameObject;
		if (colliderObject.CompareTag("MonsterHead"))
		{
			colliderObject.GetComponentInParent<Zombie>().Damage(damage * 2);
			sparksParticleMain.startColor = sparks.headShotColor;
		}
		else if (colliderObject.CompareTag("MonsterBody"))
		{
			colliderObject.GetComponentInParent<Zombie>().Damage(damage);
			sparksParticleMain.startColor = sparks.bodyShotColor;
		}
		else
		{
			sparksParticleMain.startColor = sparks.envShotColor;
		}

		sparks.Activate(sparksPool);

		ReturnToPool();
	}
}
