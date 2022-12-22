using TMPro;
using UnityEngine;
using System;

public class FPSCount : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsCounter;

	private void Start()
	{
		Application.targetFrameRate = 60;		
	}

	// Update is called once per frame
	void Update()
    {
		fpsCounter.SetText($"FPS : {Math.Round(1 / Time.deltaTime, 2)} ({Math.Round(Time.deltaTime * 1000, 2)}ms). \nPhysics call per frame : {Math.Round(Time.deltaTime / Time.fixedDeltaTime, 2)}");
	}
}
