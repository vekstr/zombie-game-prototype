using UnityEngine;

public abstract class SpawnZombieSettings : ScriptableObject
{
	public int extraHitpoints;

	public abstract Zombie GetZombie();
}
