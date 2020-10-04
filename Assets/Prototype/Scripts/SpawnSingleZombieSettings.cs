using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSingleZombieSettings", menuName = "ZombieTest/Spawn Single Zombie Settings")]
public class SpawnSingleZombieSettings : SpawnZombieSettings
{
	[SerializeField]
	private Zombie spawnPrefab;

	public override Zombie GetZombie() => spawnPrefab;
}
