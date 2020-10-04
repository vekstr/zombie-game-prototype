using UnityEngine;

[CreateAssetMenu(fileName = "SpawnRandomZombieSettings", menuName = "ZombieTest/Spawn Random Zombie Settings")]
public class SpawnRandomZombieSettings : SpawnZombieSettings
{
	[SerializeField]
	private Zombie[] randomPrefabPool;

	public override Zombie GetZombie() => randomPrefabPool.Length == 0 ? null : randomPrefabPool[Random.Range(0, randomPrefabPool.Length)];
}
