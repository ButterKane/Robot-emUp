using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerUI))]
public class PlayerUIEditor : Editor
{
	PlayerUI playerUI;
	GUIStyle headerStyle;
	GUIStyle titleStyle;

	private void OnEnable ()
	{
		playerUI = (PlayerUI)target;
	}

	public override void OnInspectorGUI ()
	{
		this.serializedObject.Update();

		headerStyle = new GUIStyle(EditorStyles.helpBox);
		headerStyle.alignment = TextAnchor.MiddleCenter;
		headerStyle.fontSize = 20;
		headerStyle.fontStyle = FontStyle.Bold;

		titleStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.fontSize = 15;
		titleStyle.fontStyle = FontStyle.Bold;

		//Global settings
		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			GUI.color = Color.gray;
			GUILayout.Box("Display settings", headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				SerializedProperty m_displayHealth = serializedObject.FindProperty("displayHealth");
				GUILayout.Label("Display health", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_displayHealth, GUIContent.none, GUILayout.Width(70));
				GUILayout.FlexibleSpace();
				SerializedProperty m_displayDashes = serializedObject.FindProperty("displayDashes");
				GUILayout.Label("Display dashes", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_displayDashes, GUIContent.none, GUILayout.Width(70));
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(20);

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				SerializedProperty m_panelWidth = serializedObject.FindProperty("panelWidth");
				GUILayout.Label("Panel size", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_panelWidth, GUIContent.none, GUILayout.Width(70));

				SerializedProperty m_panelHeight = serializedObject.FindProperty("panelHeight");
				EditorGUILayout.PropertyField(m_panelHeight, GUIContent.none, GUILayout.Width(70));
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				SerializedProperty m_panelDistanceToPlayer = serializedObject.FindProperty("panelDistanceToPlayer");
				GUILayout.Label("Distance to player", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_panelDistanceToPlayer, GUIContent.none, GUILayout.Width(70));
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
		}
		GUILayout.EndVertical();

		if (playerUI.displayHealth)
		{
			//Health display
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUILayout.Box("Health settings", headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;

			GUILayout.Label("Health gain settings", titleStyle);

			GUILayout.BeginVertical();
			{
				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthGainFadeInSpeed = serializedObject.FindProperty("healthGainFadeInSpeed");
					GUILayout.Label("Fade-in speed", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthGainFadeInSpeed, GUIContent.none, GUILayout.Width(70));
					GUILayout.FlexibleSpace();
					SerializedProperty m_healthGainFadeOutSpeed = serializedObject.FindProperty("healthGainFadeOutSpeed");
					GUILayout.Label("Fade-out speed", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthGainFadeOutSpeed, GUIContent.none, GUILayout.Width(70));
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthGainDuration = serializedObject.FindProperty("healthGainShowDuration");
					GUILayout.Label("Show duration", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthGainDuration, GUIContent.none, GUILayout.Width(70));
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthGainStartScale = serializedObject.FindProperty("healthGainStartScale");
					GUILayout.Label("Start scale", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthGainStartScale, GUIContent.none, GUILayout.Width(70));
					GUILayout.FlexibleSpace();
					SerializedProperty m_healthGainEndScale = serializedObject.FindProperty("healthGainEndScale");
					GUILayout.Label("End scale", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthGainEndScale, GUIContent.none, GUILayout.Width(70));
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthGainAnimationCurve = serializedObject.FindProperty("healthGainAnimationCurve");
					GUILayout.Label("Animation curve", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthGainAnimationCurve, GUIContent.none, GUILayout.Width(200), GUILayout.Height(30));
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthGainLerpSpeed = serializedObject.FindProperty("healthGainLerpSpeed");
					GUILayout.Label("Lerp speed", GUILayout.Width(200));
					EditorGUILayout.PropertyField(m_healthGainLerpSpeed, GUIContent.none, GUILayout.Width(200));
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Health loss settings", titleStyle);

			GUILayout.BeginVertical();
			{
				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthLossFadeInSpeed = serializedObject.FindProperty("healthLossFadeInSpeed");
					GUILayout.Label("Fade-in speed", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthLossFadeInSpeed, GUIContent.none, GUILayout.Width(70));
					GUILayout.FlexibleSpace();
					SerializedProperty m_healthLossFadeOutSpeed = serializedObject.FindProperty("healthLossFadeOutSpeed");
					GUILayout.Label("Fade-out speed", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthLossFadeOutSpeed, GUIContent.none, GUILayout.Width(70));
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthLossDuration = serializedObject.FindProperty("healthLossShowDuration");
					GUILayout.Label("Show duration", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthLossDuration, GUIContent.none, GUILayout.Width(70));
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthLossStartScale = serializedObject.FindProperty("healthLossStartScale");
					GUILayout.Label("Start scale", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthLossStartScale, GUIContent.none, GUILayout.Width(70));
					GUILayout.FlexibleSpace();
					SerializedProperty m_healthLossEndScale = serializedObject.FindProperty("healthLossEndScale");
					GUILayout.Label("End scale", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthLossEndScale, GUIContent.none, GUILayout.Width(70));
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthLossAnimationCurve = serializedObject.FindProperty("healthLossAnimationCurve");
					GUILayout.Label("Animation curve", GUILayout.Width(100));
					EditorGUILayout.PropertyField(m_healthLossAnimationCurve, GUIContent.none, GUILayout.Width(200), GUILayout.Height(30));
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					SerializedProperty m_healthLossLerpSpeed = serializedObject.FindProperty("healthLossLerpSpeed");
					GUILayout.Label("Lerp speed", GUILayout.Width(200));
					EditorGUILayout.PropertyField(m_healthLossLerpSpeed, GUIContent.none, GUILayout.Width(200));
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.Space(10);

			GUILayout.Label("Health other settings", titleStyle);
			GUILayout.BeginHorizontal();
			{
				SerializedProperty m_healthFontSize = serializedObject.FindProperty("healthFontSize");
				GUILayout.Label("Font size", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_healthFontSize, GUIContent.none, GUILayout.Width(70));
				GUILayout.FlexibleSpace();
				SerializedProperty m_healthFont = serializedObject.FindProperty("healthFont");
				GUILayout.Label("Font", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_healthFont, GUIContent.none, GUILayout.Width(150));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				SerializedProperty m_healthColorGradient = serializedObject.FindProperty("healthColorGradient");
				GUILayout.Label("Gradient", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_healthColorGradient, GUIContent.none, GUILayout.Width(200));
				GUILayout.FlexibleSpace();
				SerializedProperty m_healthGradientInterpolationRate = serializedObject.FindProperty("healthGradientInterpolationRate");
				GUILayout.Label("Gradient interpolation rate", GUILayout.Width(200));
				EditorGUILayout.PropertyField(m_healthGradientInterpolationRate, GUIContent.none, GUILayout.Width(50));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				SerializedProperty m_healthAlwaysDisplayedTreshold = serializedObject.FindProperty("healthAlwaysDisplayedTreshold");
				GUILayout.Label("Always display health below x%", GUILayout.Width(200));
				EditorGUILayout.PropertyField(m_healthAlwaysDisplayedTreshold, GUIContent.none, GUILayout.Width(175));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				SerializedProperty m_healthBarPrefab = serializedObject.FindProperty("healthBarPrefab");
				GUILayout.Label("Health bar prefab", GUILayout.Width(200));
				EditorGUILayout.PropertyField(m_healthBarPrefab, GUIContent.none, GUILayout.Width(175));

				SerializedProperty m_healthBarHeight = serializedObject.FindProperty("healthBarHeight");
				GUILayout.Label("Health bar height", GUILayout.Width(200));
				EditorGUILayout.PropertyField(m_healthBarHeight, GUIContent.none, GUILayout.Width(175));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
			GUILayout.EndVertical();
		}


		if (playerUI.displayDashes)
		{
			//Health display
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUILayout.Box("Dashes settings", headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			SerializedProperty m_dashFadeInSpeed = serializedObject.FindProperty("dashFadeInSpeed");
			GUILayout.Label("Fade-in speed", GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_dashFadeInSpeed, GUIContent.none, GUILayout.Width(70));
			GUILayout.FlexibleSpace();
			SerializedProperty m_dashFadeOutSpeed = serializedObject.FindProperty("dashFadeOutSpeed");
			GUILayout.Label("Fade-out speed", GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_dashFadeOutSpeed, GUIContent.none, GUILayout.Width(70));
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			SerializedProperty m_dashShowDuration = serializedObject.FindProperty("dashShowDuration");
			GUILayout.Label("Show duration", GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_dashShowDuration, GUIContent.none, GUILayout.Width(70));
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			SerializedProperty m_dashStartScale = serializedObject.FindProperty("dashStartScale");
			GUILayout.Label("Start scale", GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_dashStartScale, GUIContent.none, GUILayout.Width(70));
			GUILayout.FlexibleSpace();
			SerializedProperty m_dashEndScale = serializedObject.FindProperty("dashEndScale");
			GUILayout.Label("End scale", GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_dashEndScale, GUIContent.none, GUILayout.Width(70));
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			SerializedProperty m_dashAnimationCurve = serializedObject.FindProperty("dashAnimationCurve");
			GUILayout.Label("Animation curve", GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_dashAnimationCurve, GUIContent.none, GUILayout.Width(200), GUILayout.Height(30));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			SerializedProperty m_dashBarColor = serializedObject.FindProperty("dashBarColor");
			GUILayout.Label("Dash bar color", GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_dashBarColor, GUIContent.none, GUILayout.Width(100), GUILayout.Height(15));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			SerializedProperty m_dashBarSpriteBackground = serializedObject.FindProperty("dashBarSpriteBackground");
			GUILayout.Label("Dash bar sprite background", GUILayout.Width(200));
			EditorGUILayout.PropertyField(m_dashBarSpriteBackground, GUIContent.none, GUILayout.Width(200), GUILayout.Height(15));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			SerializedProperty m_dashBarSpriteFill = serializedObject.FindProperty("dashBarSpriteFill");
			GUILayout.Label("Dash bar sprite fill", GUILayout.Width(200));
			EditorGUILayout.PropertyField(m_dashBarSpriteFill, GUIContent.none, GUILayout.Width(200), GUILayout.Height(15));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUILayout.Space(10);
			GUILayout.EndVertical();
		}

		this.serializedObject.ApplyModifiedProperties();
	}
}
