﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarRadiationSpawner : MonoBehaviour {
	private bool canEmit = true;
	public float ballEmitWaitSeconds = 1f;
	public GameObject ballPrefab;
	[SerializeField] int numBalls = 3;

	Transform radiationParent;
	// Start is called before the first frame update
	void Start()
	{
		StartCoroutine(Enter());
		radiationParent = new GameObject().transform;
		radiationParent.name = "Solar Radiation";
	}

	// Update is called once per frame
	void Update() {
		if (canEmit)
			EmitBall();
	}
	private void EmitBall() {
		//for (int i = 0; i < numBalls; i++)
		Instantiate(ballPrefab, transform.position + Vector3.right * Random.Range(5, -5), Quaternion.identity, radiationParent);
		StartCoroutine(EmitBallWait());
	}

	IEnumerator EmitBallWait() {
		canEmit = false;
		yield return new WaitForSeconds(ballEmitWaitSeconds);
		canEmit = true;
	}
	IEnumerator Enter() {
		canEmit = false;
		yield return new WaitForSeconds(3f);
		canEmit = true;
	}
}