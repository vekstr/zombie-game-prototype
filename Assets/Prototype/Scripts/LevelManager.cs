using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	[Header("Player Reference")]
	public PlayerController player;
	public Transform playerTransform;

	[Header("Wave Settings")]
	public LevelWaveSettings waveSettings;

	[Header("Spawn Settings")]
	public float minSpawnPointDistanceFromPlayer;
	public Transform bossSpawnPoint;
	public List<Transform> spawnPoints;

	[Header("UI Objects")]
	public GameObject endUIObject;
	public GameObject winUIObject;
	public TextMeshProUGUI waveStatusText;

	private List<Zombie> activeZombies = new List<Zombie>();
	private int totalZombiesInCurrentWave;
	private int killedZombiesInCurrentWave;
	private int currentWaveNumber;
	private float currentLevelStartTime;
	private bool currentWaveIsBossWave;

	private void Start()
	{
		StartCoroutine(ManageLevel());
	}

	private IEnumerator ManageLevel()
	{
		currentLevelStartTime = Time.time;
		currentWaveNumber = 0;

		while (Time.time - currentLevelStartTime < waveSettings.startDelay)
		{
			UpdateWaveStatusUI();
			yield return null;
		}

		foreach (var wave in waveSettings.waves)
		{
			var breakLoop = false;

			currentWaveNumber += 1;
			totalZombiesInCurrentWave = 0;
			killedZombiesInCurrentWave = 0;
			currentWaveIsBossWave = false;

			foreach (var settings in wave.spawnSettings)
			{
				settings.Reset();
				currentWaveIsBossWave = currentWaveIsBossWave || settings.isBoss;

				totalZombiesInCurrentWave += settings.spawnCount * (settings.spawnZombieSettings.GetZombie() is BossZombie ? 9 : 1);

				if (settings.spawnTogether)
				{
					foreach (var extraSettings in settings.extraSpawnSettings)
					{
						totalZombiesInCurrentWave += extraSettings.spawnCount * (extraSettings.spawnZombieSettings.GetZombie() is BossZombie ? 9 : 1);
					}
				}
			}

			UpdateWaveStatusUI();

			while (wave.spawnSettings.Exists(x => x.numberOfZombiesSpawned < x.spawnCount))
			{
				var eligibleSpawnPoints = spawnPoints.FindAll(x => Vector3.Magnitude(x.position - playerTransform.position) > minSpawnPointDistanceFromPlayer);

				foreach (var spawnSettings in wave.spawnSettings)
				{
					if (spawnSettings.numberOfZombiesSpawned < spawnSettings.spawnCount)
					{
						var spawnPoint = eligibleSpawnPoints[Random.Range(0, eligibleSpawnPoints.Count)];
						eligibleSpawnPoints.Remove(spawnPoint);

						if (spawnSettings.spawnTogether)
						{
							for (int i = 0; i < spawnSettings.spawnCount; i++)
							{
								SpawnZombie(spawnSettings, spawnSettings.spawnZombieSettings, spawnPoint);
							}

							foreach (var extraSettings in spawnSettings.extraSpawnSettings)
							{
								for (int i = 0; i < extraSettings.spawnCount; i++)
								{
									SpawnZombie(spawnSettings, extraSettings.spawnZombieSettings, spawnPoint);
									spawnSettings.numberOfZombiesSpawned -= 1; // don't count extras sicne they're always grouped anyway
								}
							}
						}
						else
						{
							if (spawnSettings.numberOfZombiesSpawned < spawnSettings.spawnCount && activeZombies.Count < spawnSettings.spawnBottleneck)
							{
								if (Time.time > spawnSettings.lastSpawnTime + spawnSettings.spawnInterval)
								{
									if (spawnSettings.spawnZombieSettings.GetZombie() is BossZombie)
									{
										eligibleSpawnPoints.Add(spawnPoint);
										spawnPoint = bossSpawnPoint;
									}

									SpawnZombie(spawnSettings, spawnSettings.spawnZombieSettings, spawnPoint);
									UpdateWaveStatusUI();
								}
							}
						}
					}
					else if (spawnSettings.isBoss)
					{
						breakLoop = !activeZombies.Exists(x => x is BossZombie);
					}
				}

				if (breakLoop)
				{
					break;
				}
				else
				{
					yield return null;
				}
			}

			yield return new WaitUntil(() => activeZombies.Count == 0);
		}

		endUIObject.SetActive(true);
		winUIObject.SetActive(true);
		waveStatusText.gameObject.SetActive(false);

		player.EndGame();
	}

	private void UpdateWaveStatusUI()
	{
		if (currentWaveNumber == 0)
		{
			waveStatusText.SetText($"Starting in {Mathf.Max(0, waveSettings.startDelay - (Time.time - currentLevelStartTime)):0}");
		}
		else if (currentWaveIsBossWave)
		{
			if (activeZombies.Exists(x => x is BossZombie))
			{
				waveStatusText.SetText($"Final Wave [kill the boss zombie]");
			}
			else
			{
				waveStatusText.SetText($"Final Wave [kill remaining zombies ({activeZombies.Count} left)]");
			}
		}
		else
		{
			waveStatusText.SetText($"Wave {currentWaveNumber} of {waveSettings.waves.Length} [killed {killedZombiesInCurrentWave} of {totalZombiesInCurrentWave} zombies]");
		}
	}

	public void SpawnZombie(SpawnSettings settings, SpawnZombieSettings zombieSettings, Transform spawnPoint)
	{
		var zombie = Instantiate(zombieSettings.GetZombie(), spawnPoint.position, Quaternion.identity);
		zombie.Initialize(this, zombieSettings.extraHitpoints);

		settings.numberOfZombiesSpawned += 1;
		settings.lastSpawnTime = Time.time;
	}

	public void AddZombie(Zombie zombie)
	{
		activeZombies.Add(zombie);
	}

	public void RemoveZombie(Zombie zombie)
	{
		activeZombies.Remove(zombie);
		killedZombiesInCurrentWave += 1;
		UpdateWaveStatusUI();
	}

	public void ReloadLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
