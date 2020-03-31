﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {
	public bool runModel = true;
	bool exitSure = false;
	GameObject loadingScreen;
	Slider loadingBar;

	public override void Awake() {
		base.Awake();
		if (runModel)
			if (World.averageTemp == 0)
				World.Init();
		// SpeedTest.VectorAllocTest();
	}

	void Start() {
		loadingScreen = transform.GetChild(0).GetChild(0).gameObject; //do better
		loadingScreen.SetActive(false);
		SceneManager.activeSceneChanged += instance.InitScene;
		// loadingBar = loadingScreen.GetComponentInChildren<Slider>();
	}

	public static void QuitGame() {
		// prompt
		if (!instance.exitSure) {
			Debug.Log("Are you sure");
			instance.exitSure = true;
		} else
			Application.Quit();
		// instance.exitSure = false;
	}

	void InitScene(Scene to, Scene from) {
		instance.loadingScreen.SetActive(false);
		UIController.Instance.ToggleBackButton(to.name != "Overworld");
	}

	public static void Transition(string scene) => instance.StartCoroutine(LoadScene(scene));

	static IEnumerator LoadScene(string name) {
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
		asyncLoad.allowSceneActivation = false;
		float start = Time.realtimeSinceStartup;

		bool calcDone = true;
		if (name == "Overworld") {
			calcDone = false;
			Thread calcThread = new Thread(() => { World.Calc(); calcDone = true; });
			calcThread.Priority = System.Threading.ThreadPriority.AboveNormal;
			calcThread.Start();
		}

		Instance.loadingScreen.SetActive(true);

		while (!asyncLoad.isDone || !calcDone) {
			yield return null;
			// instance.loadingBar.normalizedValue = asyncLoad.progress / .9f;

			if (asyncLoad.progress >= .9f && Time.realtimeSinceStartup - start > 1 && calcDone) {
				Time.timeScale = 1;
				asyncLoad.allowSceneActivation = true;
				if (name == "Overworld")
					UIController.Instance.IncrementTurn();
				UIController.Instance.SetPrompt(false);
				yield break;
			}
		}
	}
}
