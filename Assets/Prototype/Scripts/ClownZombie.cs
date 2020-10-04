using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClownZombie : Zombie
{
	[Header("Clown Zombie Settings")]
	public float speedWhileExploding;
	public float explosionDamageToOtherZombies;
	public float explosionRadius;
	public GameObject explosionPrefab;

	protected override void Die()
	{
		StartCoroutine(Explode());
	}

	private IEnumerator Explode()
	{
		agent.speed = speedWhileExploding;
		yield return new WaitForSeconds(3f);

		if (CheckPlayerInsideRadius(explosionRadius))
		{
			player.Damage();
		}

		var damagedZombieColliders = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Zombies"));
		foreach (var collider in damagedZombieColliders)
		{
			if (collider.CompareTag("MonsterBody"))
			{
				var zombie = collider.GetComponentInParent<Zombie>();
				if (zombie != this)
				{
					zombie.Damage(125);
				}
			}
		}

		Instantiate(explosionPrefab, transform.position, Quaternion.identity);

		base.Die();
	}
}
