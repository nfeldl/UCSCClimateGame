﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathClient : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		EBM.printTest();
		// float[] sol = RunTemp((new float[ClimateMath.ebmv.bands]).Select(x => 10f).ToArray(), 5, true);
		// Debug.Log(String.Join(" ", sol));
		// sol = RunTemp((new float[EBM.bands]).Select(x => 10f).ToArray(), 5, true, true);

		// Debug.Log(String.Join(" ", EBM.simple.odeint((new float[EBM.bands]).Select(x => 10f).ToArray(), 5, true)));

		// RunTempF();
		// float[] temp = new float[3] { 10, 10, 10 };
		// ClimateMath.odeIce(out temp);
	}

	// Update is called once per frame
	void Update()
	{
	}

	float[] RunTemp(float[] temp, int steps, bool useMoisture = false, bool ebm = false)
	{
		float[] time = new float[steps];
		for (int i = 0; i < steps; i++)
			time[i] = i * 30f / steps;
		float[] t = temp;
		// Debug.Log("t: " + String.Join(" ", t));
		for (int i = 0; i < steps; i++)
		{
			if (!ebm)
				if (useMoisture)
					t = t.Zip(ClimateMath.odeMoist(t, time[i]), (a, b) => a + b / (float)steps * (10)).ToArray();
				else
					t = t.Zip(ClimateMath.ode(t, time[i]), (a, b) => a + b / (float)steps * (10)).ToArray();
			// else
			// 	t = t.Zip(EBM.simple.odefunc(t, time[i], useMoisture), (a, b) => a + b / (float)steps * (10)).ToArray();
			// Debug.Log("t" + (i + 1) + ": " + String.Join(" ", t));
		}
		return t;
	}

	float[] RunTempF()
	{
		ClimateMath.odeFast(new float[0], 0);
		return new float[0];
	}
}
