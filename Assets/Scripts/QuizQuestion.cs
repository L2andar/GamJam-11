using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "GameJam/Quiz Question", fileName = "NewQuizQuestion")]
public class QuizQuestion : ScriptableObject
{
	// The question shown to the player
	[TextArea(2, 5)]
	public string questionText;

	// The possible answers, in order
	public List<string> answers = new List<string>();

	// Index into answers that is correct (0-based). Keep within range.
	public int correctIndex = 0;

	// Events designers can hook up in the inspector.
	// Example uses: open a door, play a sound, trigger dialogue, etc.
	public UnityEvent onCorrect = new UnityEvent();
	public UnityEvent onWrong = new UnityEvent();

	// Usage notes (inspectors/readme):
	// - Create a new question: Right click in Project window -> Create -> GameJam -> Quiz Question
	// - Fill questionText and add answers in the Inspector.
	// - Set correctIndex to the correct answer (0-based).
	// - Drag functions into onCorrect / onWrong to define behavior when answered.

	// Ensure answers exists and correctIndex stays within bounds (helps avoid issues
	// that could prevent asset creation or cause unexpected inspector behaviour).
	private void OnValidate()
	{
		if (answers == null) answers = new List<string>();
		if (answers.Count == 0)
		{
			correctIndex = 0;
			return;
		}
		if (correctIndex < 0) correctIndex = 0;
		if (correctIndex >= answers.Count) correctIndex = answers.Count - 1;
	}

	#if UNITY_EDITOR
	// Fallback: explicit Assets/Create menu entry for creating this ScriptableObject.
	// This appears under Assets -> Create -> GameJam -> Quiz Question.
	[MenuItem("Assets/Create/GameJam/Quiz Question", priority = 51)]
	private static void CreateQuizQuestionAsset()
	{
		var asset = ScriptableObject.CreateInstance<QuizQuestion>();
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (string.IsNullOrEmpty(path)) path = "Assets";
		else if (System.IO.Path.GetExtension(path) != "") path = System.IO.Path.GetDirectoryName(path);
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(path, "NewQuizQuestion.asset"));
		AssetDatabase.CreateAsset(asset, assetPathAndName);
		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}
	#endif
}
