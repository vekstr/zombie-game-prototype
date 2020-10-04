using UnityEngine;

[CreateAssetMenu(fileName = "LevelWaveSettings", menuName = "ZombieTest/Level Wave Settings")]
public class LevelWaveSettings : ScriptableObject
{
	public float startDelay;
	public WaveSettings[] waves;
}
