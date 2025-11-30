using UnityEngine;

public class QuizTrigger : MonoBehaviour
{
	// Drag the QuizManager from the scene into this field
	public QuizManager quizManager;
	// Optional: assign the Canvas (or parent GameObject) that holds the quiz UI.
	// If left null, the script will try to find canvases under QuizManager and then search the scene.
	public GameObject quizCanvas;
	// If true, deactivate the quiz UI when the player leaves the trigger
	public bool deactivateUIOnExit = true;
	// Optional name or tag to help scene search if no explicit canvas assigned.
	public string quizCanvasName = "QuizCanvas";
	public string quizCanvasTag = "QuizUI";

	// Optional: require player to be tagged "Player" OR have one of these components:
	// - CharacterController (Unity character controller)
	// - Rigidbody (physics-based player)
	// - a custom movement script (example name: "PlayerMovement")
	// Make sure the Player GameObject also has a non-trigger Collider.
	// Make sure this GameObject has a Collider with isTrigger = true.

	// optional runtime settings
	public bool disableAfterTrigger = true;
	public string customPlayerScriptName = "PlayerMovement";
	
	// Behaviour: while the quiz is unanswered, trigger works every time player enters.
	// After the quiz is answered, allow 'activationsAfterAnswer' further activations (default 0).
	// When remaining activations are consumed (or activationsAfterAnswer==0), the trigger is disabled if disableAfterTrigger is true.
	public int activationsAfterAnswer = 0;
	private int remainingActivationsAfterAnswer = 0;
	private bool quizAnswered = false;

	void OnTriggerEnter(Collider other)
	{
		Debug.Log($"QuizTrigger: OnTriggerEnter with '{other.gameObject.name}' (tag='{other.gameObject.tag}')");

		// Only respond to player (tag, CharacterController, Rigidbody, or custom script)
		if (!(other.CompareTag("Player")
		      || other.GetComponent<CharacterController>() != null
		      || other.GetComponent<Rigidbody>() != null
		      || other.GetComponent("PlayerMovement") != null)) // replace with your script name if different
			return;

		// Try to recover a missing reference at runtime
		if (quizManager == null)
		{
			quizManager = FindObjectOfType<QuizManager>();
			if (quizManager == null)
			{
				Debug.LogWarning("QuizTrigger: No QuizManager found in scene.");
				return;
			}
		}

		// Ensure the manager GameObject is active so we can search its children.
		if (!quizManager.gameObject.activeInHierarchy)
			quizManager.gameObject.SetActive(true);
		
		// If quiz already answered, only allow further activations according to remainingActivationsAfterAnswer.
		if (quizAnswered)
		{
			if (remainingActivationsAfterAnswer <= 0)
			{
				Debug.Log("QuizTrigger: quiz already answered and no remaining activations. Ignoring trigger.");
				return;
			}

			remainingActivationsAfterAnswer--;
			Debug.Log($"QuizTrigger: quiz answered previously — using one remaining activation (left={remainingActivationsAfterAnswer}).");
		}
		
		// Try to activate the UI before calling ShowQuestion.
		bool uiActivated = false;

		// 1) If explicit quizCanvas assigned, activate it.
		if (quizCanvas != null)
		{
			ActivateUI(quizCanvas);
			Debug.Log($"QuizTrigger: Activated explicit quizCanvas '{quizCanvas.name}'.");
			uiActivated = true;
		}

		// 2) Try canvases under the QuizManager (covers typical setups).
		if (!uiActivated)
		{
			var canvases = quizManager.GetComponentsInChildren<Canvas>(true);
			if (canvases != null && canvases.Length > 0)
			{
				foreach (var c in canvases) ActivateUI(c.gameObject);
				Debug.Log($"QuizTrigger: Activated {canvases.Length} Canvas(es) under QuizManager.");
				uiActivated = true;
			}
		}

		// 3) Scene-wide search (including inactive) by name, tag, or name containing "quiz".
		if (!uiActivated)
		{
			// Resources.FindObjectsOfTypeAll returns inactive objects too (works in editor/runtime for UI objects in scene).
			var allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
			foreach (var c in allCanvases)
			{
				// skip canvases that are assets (not in scene) by requiring a scene root
				if (c.gameObject.scene.name == null) continue;
				var go = c.gameObject;
				bool match = false;
				if (!string.IsNullOrEmpty(quizCanvasName) && go.name == quizCanvasName) match = true;
				if (!match && !string.IsNullOrEmpty(quizCanvasTag) && go.CompareTagSafe(quizCanvasTag)) match = true;
				if (!match && go.name.ToLowerInvariant().Contains("quiz")) match = true;
				if (match)
				{
					ActivateUI(go);
					Debug.Log($"QuizTrigger: Activated scene Canvas '{go.name}' (matched by search).");
					uiActivated = true;
					// do not break: activate all matching canvases
				}
			}
		}

		if (!uiActivated)
		{
			Debug.LogWarning("QuizTrigger: No quiz Canvas found/activated. Check QuizManager or assign quizCanvas in the inspector.");
		}

		// Call the manager to show the question
		quizManager.ShowQuestion();
		
		// If quiz was answered and there are no remaining activations and disableAfterTrigger is true, disable trigger now.
		if (quizAnswered && remainingActivationsAfterAnswer <= 0 && disableAfterTrigger)
		{
			DisableTrigger();
		}
	}

	// helper: activate UI GameObject and ensure Canvas/CanvasGroup are usable
	private void ActivateUI(GameObject uiRoot)
	{
		if (uiRoot == null) return;
		if (!uiRoot.activeInHierarchy) uiRoot.SetActive(true);

		var canvas = uiRoot.GetComponent<Canvas>();
		if (canvas != null && !canvas.enabled) canvas.enabled = true;

		// enable any Canvas components in children
		var canvases = uiRoot.GetComponentsInChildren<Canvas>(true);
		foreach (var c in canvases)
		{
			if (!c.gameObject.activeInHierarchy) c.gameObject.SetActive(true);
			if (!c.enabled) c.enabled = true;
		}

		// enable CanvasGroup if present
		var groups = uiRoot.GetComponentsInChildren<CanvasGroup>(true);
		foreach (var g in groups)
		{
			if (!g.gameObject.activeInHierarchy) g.gameObject.SetActive(true);
			g.alpha = 1f;
			g.interactable = true;
			g.blocksRaycasts = true;
		}
	}
	
	// mirror of ActivateUI: disable/hide UI root (Canvas + CanvasGroup)
	private void DeactivateUI(GameObject uiRoot)
	{
		if (uiRoot == null) return;

		// disable Canvas components in the root and children
		var canvas = uiRoot.GetComponent<Canvas>();
		if (canvas != null) canvas.enabled = false;

		var canvases = uiRoot.GetComponentsInChildren<Canvas>(true);
		foreach (var c in canvases)
		{
			if (c == null) continue;
			c.enabled = false;
			// optionally collapse GameObject
			if (c.gameObject.activeInHierarchy) c.gameObject.SetActive(false);
		}

		// adjust CanvasGroup to block interaction and hide
		var groups = uiRoot.GetComponentsInChildren<CanvasGroup>(true);
		foreach (var g in groups)
		{
			if (g == null) continue;
			g.alpha = 0f;
			g.interactable = false;
			g.blocksRaycasts = false;
			if (g.gameObject.activeInHierarchy) g.gameObject.SetActive(false);
		}
	}

	void OnTriggerExit(Collider other)
	{
		Debug.Log($"QuizTrigger: OnTriggerExit with '{other.gameObject.name}' (tag='{other.gameObject.tag}')");

		// Only respond to player-like objects
		bool isPlayerLike = other.CompareTag("Player")
		                    || other.GetComponent<CharacterController>() != null
		                    || other.GetComponent<Rigidbody>() != null
		                    || other.GetComponent("PlayerMovement") != null;
		if (!isPlayerLike) return;

		// Try to recover missing manager reference
		if (quizManager == null)
			quizManager = FindObjectOfType<QuizManager>();

		if (deactivateUIOnExit)
		{
			bool uiDeactivated = false;

			// 1) explicit assigned canvas
			if (quizCanvas != null)
			{
				DeactivateUI(quizCanvas);
				Debug.Log($"QuizTrigger: Deactivated explicit quizCanvas '{quizCanvas.name}'.");
				uiDeactivated = true;
			}

			// 2) canvases under QuizManager
			if (!uiDeactivated && quizManager != null)
			{
				var canvases = quizManager.GetComponentsInChildren<Canvas>(true);
				if (canvases != null && canvases.Length > 0)
				{
					foreach (var c in canvases) DeactivateUI(c.gameObject);
					Debug.Log($"QuizTrigger: Deactivated {canvases.Length} Canvas(es) under QuizManager.");
					uiDeactivated = true;
				}
			}

			// 3) scene-wide search as fallback
			if (!uiDeactivated)
			{
				var allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
				foreach (var c in allCanvases)
				{
					if (c.gameObject.scene.name == null) continue;
					var go = c.gameObject;
					bool match = false;
					if (!string.IsNullOrEmpty(quizCanvasName) && go.name == quizCanvasName) match = true;
					if (!match && !string.IsNullOrEmpty(quizCanvasTag) && go.CompareTagSafe(quizCanvasTag)) match = true;
					if (!match && go.name.ToLowerInvariant().Contains("quiz")) match = true;
					if (match)
					{
						DeactivateUI(go);
						Debug.Log($"QuizTrigger: Deactivated scene Canvas '{go.name}' (matched by search).");
						uiDeactivated = true;
					}
				}
			}

			if (!uiDeactivated)
				Debug.LogWarning("QuizTrigger: No quiz Canvas found to deactivate on exit.");
		}

		// do not disable here; disabling is handled when quiz is answered / remaining activations consumed
	}

	// Called by QuizManager (or UI) when the player answers the quiz.
	public void QuizAnswered()
	{
		quizAnswered = true;
		remainingActivationsAfterAnswer = activationsAfterAnswer;
		Debug.Log($"QuizTrigger: QuizAnswered called. activationsAfterAnswer={activationsAfterAnswer}");

		// If no further activations are allowed, disable immediately if requested.
		if (remainingActivationsAfterAnswer <= 0 && disableAfterTrigger)
		{
			DisableTrigger();
		}
	}

	private void DisableTrigger()
	{
		var col = GetComponent<Collider>();
		if (col != null) col.enabled = false;
		else gameObject.SetActive(false);
		Debug.Log("QuizTrigger: Trigger disabled.");
	}
}

// small helper extension to safely CompareTag without exception when tag not defined on object
static class QuizTriggerExtensions
{
	public static bool CompareTagSafe(this GameObject go, string tag)
	{
		if (go == null || string.IsNullOrEmpty(tag)) return false;
		// if tag doesn't exist in project this would throw; wrap in try
		try { return go.CompareTag(tag); } catch { return false; }
	}
}
