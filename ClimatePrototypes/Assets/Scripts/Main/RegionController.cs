using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class RegionController : MonoBehaviour {
	// timer logic?

	protected bool paused = false;
	protected bool updated = false;

	protected void Pause( /*string scene/text */ ) {
		if (!paused) {
			paused = true;
			Time.timeScale = 0;
			UIController.Instance.SetPrompt(true);
			updated = false;
		}
	}

	protected void TriggerUpdate(System.Action updateEBM) {
		if (!updated) {
			updateEBM();
			updated = true;
		}
	}
}