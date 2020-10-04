using UnityEngine;

public class NormalZombie : Zombie
{
	[Header("Normal Zombie Settings")]
	public int[] startHitpoints;

	public override void Initialize(LevelManager levelManager, int extraHitpoints)
	{
		base.Initialize(levelManager, extraHitpoints);
		hitpoint = startHitpoints[Random.Range(0, startHitpoints.Length)] + extraHitpoints;
	}
}
