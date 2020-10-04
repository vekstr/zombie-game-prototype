using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	public TextMeshProUGUI fpsText;
	public int bufferSize;

	private float[] buffer;
	private int bufferIndex;

	private void Awake()
	{
		buffer = new float[bufferSize];
	}

	private void LateUpdate()
	{
		buffer[bufferIndex] = Time.deltaTime;
		bufferIndex = (bufferIndex + 1) % bufferSize;
		fpsText.text = $"{Mathf.FloorToInt(1f / buffer.Average())} FPS";
	}
}
