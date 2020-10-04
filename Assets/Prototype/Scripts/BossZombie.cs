using UnityEngine;

public class BossZombie : Zombie
{
	[Header("Boss Zombie Settings")]
	public BossZombie nextFormPrefab;
	public int nextFormCount;

	protected override void Die()
	{
		for (int i = 0; i < nextFormCount; i++)
		{
			var nextForm = Instantiate(nextFormPrefab, transform.position + Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized * 3f * transform.localScale.x, Quaternion.identity);
			nextForm.Initialize(levelManager, 0);
		}

		base.Die();
	}
}
