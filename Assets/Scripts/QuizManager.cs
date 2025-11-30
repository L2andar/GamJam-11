using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuizManager : MonoBehaviour
{
	// Assign a QuizQuestion asset in the Inspector
	public QuizQuestion currentQuestion;

	// UI: assign your TextMeshPro UI elements in the Inspector.
	// For runtime, set questionText and one TMP element per answer button (answerTexts).
	// You can use TextMeshPro - Text (UI) components (they inherit TMP_Text).
	public TMP_Text questionText;
	public List<TMP_Text> answerTexts = new List<TMP_Text>();

	// Call this to display/read the current question on UI.
	// Setup:
	// - Create 4 buttons (or as many as you want).
	// - Each button should have a child TextMeshProUGUI component; assign those to answerTexts.
	// - In each Button.OnClick() add QuizManager.SubmitAnswer and set the integer parameter (0,1,2,3).
	// - Assign the QuizManager component and the QuizQuestion asset in the Inspector.
	public void ShowQuestion()
	{
		if (currentQuestion == null)
		{
			Debug.LogWarning("QuizManager: No currentQuestion assigned.");
			return;
		}

		// Display question text if UI assigned
		if (questionText != null)
			questionText.text = currentQuestion.questionText;
		else
			Debug.Log($"Question: {currentQuestion.questionText}");

		// Populate answers into provided TMP elements.
		// If there are more answers than UI slots, only show available slots.
		// If there are fewer answers than slots, clear extra slots.
		if (answerTexts != null && answerTexts.Count > 0)
		{
			for (int i = 0; i < answerTexts.Count; i++)
			{
				var slot = answerTexts[i];
				if (slot == null) continue;

				if (i < currentQuestion.answers.Count)
				{
					slot.text = currentQuestion.answers[i];
					slot.gameObject.SetActive(true);
				}
				else
				{
					// no answer for this slot
					slot.text = "";
					slot.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			// No UI slots: fallback to logging answers
			for (int i = 0; i < currentQuestion.answers.Count; i++)
				Debug.Log($"{i}: {currentQuestion.answers[i]}");
		}
	}

	// Call this from UI buttons (e.g. Button OnClick) or code.
	// Example UI setup:
	// - Create N buttons, for each button set OnClick -> QuizManager.SubmitAnswer(int)
	// - In the Button inspector set the integer parameter (0..N-1)
	public void SubmitAnswer(int index)
	{
		if (currentQuestion == null)
		{
			Debug.LogWarning("QuizManager: No currentQuestion assigned.");
			return;
		}

		if (index < 0 || index >= currentQuestion.answers.Count)
		{
			Debug.LogWarning($"QuizManager: Submitted index {index} out of range.");
			return;
		}

		if (index == currentQuestion.correctIndex)
		{
			Debug.Log("QuizManager: Correct answer.");
			currentQuestion.onCorrect?.Invoke();
		}
		else
		{
			Debug.Log("QuizManager: Wrong answer.");
			currentQuestion.onWrong?.Invoke();
		}
	}

	// Quick helper: call this to clear/unassign the current question
	public void ClearQuestion()
	{
		currentQuestion = null;

		// option: clear UI
		if (questionText != null) questionText.text = "";
		if (answerTexts != null)
		{
			foreach (var t in answerTexts)
				if (t != null) { t.text = ""; t.gameObject.SetActive(false); }
		}
	}
}
