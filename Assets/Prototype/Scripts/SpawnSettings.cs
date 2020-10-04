using UnityEngine;

[System.Serializable]
public class ExtraSpawnSettings
{
	public SpawnZombieSettings spawnZombieSettings;
	public int spawnCount;
}

[System.Serializable]
public class SpawnSettings
{
	public SpawnZombieSettings spawnZombieSettings;
	public int spawnCount;
	[Space]
	public ExtraSpawnSettings[] extraSpawnSettings;
	public bool spawnTogether;
	[Space]
	public int spawnBottleneck;
	public float spawnInterval;
	[Space]
	public bool isBoss;

	public int numberOfZombiesSpawned { get; set; }
	public float lastSpawnTime { get; set; }

	public void Reset()
	{
		lastSpawnTime = 0f;
		numberOfZombiesSpawned = 0;
	}
}
