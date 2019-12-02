using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System;
using System.Reflection;

[CustomEditor(typeof(WaveController))]
public class WaveEditor : Editor
{
	EnemyDatas enemyDatas;
	public Dictionary<WaveData, int> enemyFoldoutIndex;
	public Dictionary<WaveData, EnemyData[]> enemyAvailable;
	WaveController waveEditor;

	private void OnEnable ()
	{
		waveEditor = (WaveController)target;
		enemyDatas = Resources.Load<EnemyDatas>("EnemyDatas");

		//Generates dictionaries
		enemyFoldoutIndex = new Dictionary<WaveData, int>();
		enemyAvailable = new Dictionary<WaveData, EnemyData[]>();

		//Fill dictionaries;
		foreach (WaveData wave in waveEditor.waveList)
		{
			enemyFoldoutIndex.Add(wave, 0);
			enemyAvailable.Add(wave, new EnemyData[0]);
			UpdateAvailableEnemies(wave);
		}
	}

	public override void OnInspectorGUI ()
	{
		if (waveEditor.waveList.Count > 0)
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUIStyle headerStyle = new GUIStyle(EditorStyles.helpBox);
			headerStyle.alignment = TextAnchor.MiddleCenter;
			headerStyle.fontSize = 20;
			headerStyle.fontStyle = FontStyle.Bold;
			GUILayout.Box("Global settings", headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			SerializedProperty m_startOnTriggerEnter = serializedObject.FindProperty("startOnTriggerEnter");
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(m_startOnTriggerEnter, new GUIContent("Start automatically when players enters zone ?"));
			this.serializedObject.ApplyModifiedProperties();
			GUILayout.Space(10);
			GUILayout.EndVertical();
			GUILayout.Space(20);


			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUILayout.Box("Spawns", headerStyle);
			GUI.color = Color.white;
			GUILayout.Space(10);
			List<SpawnInformation> updatedList = new List<SpawnInformation>();
			for (int i = 0; i < waveEditor.spawnList.Count; i++)
			{
				if (waveEditor.spawnList[i].transform != null) 
				{
					updatedList.Add(waveEditor.spawnList[i]); 
				}
				GUILayout.BeginHorizontal();
				waveEditor.spawnList[i].color = EditorGUILayout.ColorField(waveEditor.spawnList[i].color, GUILayout.Width(50), GUILayout.Height(20));
				SerializedProperty m_spawnName = serializedObject.FindProperty("spawnList.Array.data[" + i + "].customName");
				this.serializedObject.Update();
				EditorGUILayout.PropertyField(m_spawnName, new GUIContent(""));
				this.serializedObject.ApplyModifiedProperties();
				if (waveEditor.spawnList[i].transform != null)
				{
					waveEditor.spawnList[i].transform.name = "WaveSpawn[" + waveEditor.spawnList[i].customName + "]";
					waveEditor.spawnList[i].transform = EditorGUILayout.ObjectField(waveEditor.spawnList[i].transform, typeof(Transform), true) as Transform;
				}
				if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
				{
					SpawnInformation deletedObject = updatedList[i];
					foreach (WaveData wave in waveEditor.waveList)
					{
						foreach (WaveEnemy enemy in wave.currentEnemies)
						{
							if (enemy.spawnIndexes.Contains(i))
							{
								enemy.spawnIndexes.Remove(i);
							}
						}
					}
					updatedList.Remove(updatedList[i]);
					DestroyImmediate(deletedObject.transform.gameObject);
				}
				GUILayout.EndHorizontal();
			}
			waveEditor.spawnList = updatedList;

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Add spawn", GUILayout.Width(100), GUILayout.Height(30))) 
			{
				AddSpawn();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			GUILayout.EndVertical();


			GUILayout.Label("Actions on wave start", EditorStyles.centeredGreyMiniLabel);
			SerializedProperty m_events = serializedObject.FindProperty("onStartEvents");
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(m_events, true);
			this.serializedObject.ApplyModifiedProperties();
		}
		for (int i = 0; i < waveEditor.waveList.Count; i++)
		{
			WaveData waveData = waveEditor.waveList[i];
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUILayout.BeginHorizontal();
			GUIStyle headerStyle = new GUIStyle(EditorStyles.helpBox);
			headerStyle.alignment = TextAnchor.MiddleCenter;
			headerStyle.fontSize = 20;
			headerStyle.fontStyle = FontStyle.Bold;
			GUILayout.Box("Wave " + (i + 1).ToString(), headerStyle);
			GUI.color = Color.white;
			if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(30), GUILayout.Height(30)))
			{
				waveEditor.waveList.Remove(waveData);
			}
			GUILayout.EndHorizontal();

			//"Wave global settings" panel
			GUILayout.Label("Global settings", EditorStyles.centeredGreyMiniLabel);
			waveData.maxPowerLevel = EditorGUILayout.CurveField("Max power level", waveData.maxPowerLevel);
			waveData.duration = EditorGUILayout.FloatField("Duration", waveData.duration) ;
			waveData.pauseBeforeNextWave = EditorGUILayout.FloatField("Pause before next wave", waveData.pauseBeforeNextWave);

			//"Wave enemy ratio visualizers" panel
			GUILayout.Space(10);
			GUILayout.Label("Enemy spawn chances", EditorStyles.centeredGreyMiniLabel);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			for (int y = 0; y < waveData.currentEnemies.Count; y++)
			{
				GUI.color = waveData.currentEnemies[y].enemyType.color;
				GUIStyle visualizerStyle = new GUIStyle(GUI.skin.textField);
				visualizerStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.TextField(waveData.currentEnemies[y].enemyType.name, visualizerStyle, GUILayout.Width(300 * waveData.currentEnemies[y].probability), GUILayout.Height(30));
				GUI.color = waveData.currentEnemies[y].enemyType.color;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			//"Current enemies" panel
			GUI.color = Color.white;
			GUILayout.BeginVertical();
			for (int y = 0; y < waveData.currentEnemies.Count; y++)
			{
				GUILayout.BeginHorizontal();
				GUI.color = waveData.currentEnemies[y].enemyType.color;
				GUILayout.Label(EditorGUIUtility.IconContent("Canvas Icon"), GUILayout.Width(30), GUILayout.Height(30));
				GUI.color = Color.white;
				GUILayout.BeginVertical();
				GUILayout.Box(waveEditor.waveList[i].currentEnemies[y].enemyType.name, EditorStyles.miniButton);
				GUILayout.Box("Power level: "+waveEditor.waveList[i].currentEnemies[y].enemyType.prefab.GetComponent<EnemyBehaviour>().powerLevel, EditorStyles.centeredGreyMiniLabel);
				GUILayout.EndVertical();
				EditorGUI.BeginChangeCheck();
				float probaSliderValue = EditorGUILayout.Slider(Mathf.Round(waveData.currentEnemies[y].probability * 100f) / 100f, 0f, 1f);
				if (EditorGUI.EndChangeCheck())
				{
					waveData.SetEnemySpawnProbability(waveData.currentEnemies[y].enemyType, probaSliderValue);
				}
				if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
				{
					waveData.RemoveEnemySpawnProbability(waveData.currentEnemies[y]);
					UpdateAvailableEnemies(waveData);
				}
				for (int z = 0; z < waveEditor.spawnList.Count; z++) {
					GUI.color = waveEditor.spawnList[z].color;
					GUIStyle buttonStyle = new GUIStyle(EditorStyles.boldLabel);
					if (GUILayout.Button(Resources.Load<Texture2D>("WaveToolSprites/WhiteSquareTransparentBorder"), buttonStyle, GUILayout.Width(30), GUILayout.Height(30)))
					{
						if (waveData.currentEnemies[y].spawnIndexes.Contains(z))
						{
							waveData.currentEnemies[y].spawnIndexes.Remove(z);
						} else
						{
							waveData.currentEnemies[y].spawnIndexes.Add(z);
						}
					}
					GUI.color = Color.white;
					if (waveData.currentEnemies[y].spawnIndexes.Contains(z))
					{
						GUI.Label(GUILayoutUtility.GetLastRect(), Resources.Load<Texture2D>("WaveToolSprites/WhiteOutline"));
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
			}
			GUILayout.EndVertical();

			//"Add enemy" panel
			if (enemyAvailable[waveData] != null)
			{
				if (enemyAvailable[waveData].Length > 0)
				{
					GUILayout.Space(10);
					GUILayout.Label("Add enemy type", EditorStyles.centeredGreyMiniLabel );
					GUILayout.BeginHorizontal();
					enemyFoldoutIndex[waveData] = EditorGUILayout.Popup(enemyFoldoutIndex[waveData], EnemyListToString(enemyAvailable[waveData]));
					if (GUILayout.Button("Add"))
					{
						AddEnemy(waveData, enemyFoldoutIndex[waveData]);
					}
					GUILayout.EndHorizontal();
				}
			}

			//"onEndActions" panel
			GUILayout.Space(10);
			GUILayout.Label("Actions on wave end", EditorStyles.centeredGreyMiniLabel);
			SerializedProperty m_events = serializedObject.FindProperty("waveList.Array.data[" + i + "].onEndEvents");
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(m_events, true);
			this.serializedObject.ApplyModifiedProperties();
			GUILayout.Space(20);
			GUILayout.EndVertical();
			GUILayout.Space(30);
		}

		//"Add new wave" panel
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Add new wave", GUILayout.Width(150), GUILayout.Height(35)))
		{
			AddWave();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(30);
	}

	void AddWave()
	{
		WaveData newWave = new WaveData();
		AnimationCurve newCurve = new AnimationCurve();
		newCurve.AddKey(new Keyframe(0, 1));
		newCurve.AddKey(new Keyframe(1, 1));
		newWave.maxPowerLevel = newCurve;
		waveEditor.waveList.Add(newWave);
		enemyFoldoutIndex.Add(newWave, 0);
		enemyAvailable.Add(newWave, new EnemyData[0]);
		UpdateAvailableEnemies(newWave);
	}

	void UpdateAvailableEnemies(WaveData wave)
	{
		enemyAvailable[wave] = GetAvailableEnemies(wave);
	}

	void AddEnemy ( WaveData wave, int index)  
	{
		if (enemyAvailable[wave][index] == null) { return; }
		WaveEnemy newWEP = new WaveEnemy();
		newWEP.enemyType = enemyAvailable[wave][index];
		if (wave.currentEnemies.Count <= 0)
		{
			newWEP.probability = 1;
		} else
		{
			newWEP.probability = 0;
		}
		newWEP.spawnIndexes = new List<int>();
		wave.currentEnemies.Add(newWEP);
		UpdateAvailableEnemies(wave); 
	}

	void AddSpawn()
	{
		GameObject newSpawn = new GameObject();
		newSpawn.name = "Wave-Spawn [" + (waveEditor.spawnList.Count + 1) + "]";
		newSpawn.transform.SetParent(waveEditor.transform);
		newSpawn.transform.position = waveEditor.GetComponentInChildren<CameraZone>().GetCenterPosition();
		Texture2D tex = EditorGUIUtility.IconContent("sv_label_0").image as Texture2D; 
		Type editorGUIUtilityType = typeof(EditorGUIUtility);
		BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
		object[] args = new object[] { newSpawn, tex };
		editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
		SpawnInformation newSpawnInf = new SpawnInformation();
		newSpawnInf.transform = newSpawn.transform;
		newSpawnInf.customName = "SpawnName";
		newSpawnInf.color = Color.white;
		waveEditor.spawnList.Add(newSpawnInf);
		this.serializedObject.Update();
	}

	public string[] EnemyListToString(EnemyData[] enemyList)
	{
		string[] finalString = new string[enemyList.Length];
		for (int i = 0; i < enemyList.Length; i++)
		{
			if (enemyList[i] == null) { finalString[i] = ""; }
			else
			{
				finalString[i] = enemyList[i].name;
			}
		}
		return finalString;
	}

	public EnemyData[] GetAvailableEnemies(WaveData waveData)
	{
		EnemyDatas totalEnemies = enemyDatas;
		int enemyCount = totalEnemies.enemyDatas.Count - waveData.currentEnemies.Count;
		if (enemyCount <= 0) { return new EnemyData[0]; }
		EnemyData[] availableEnemiesList = new EnemyData[enemyCount]; 
		List<EnemyData> unavailableEnemiesList = new List<EnemyData>();
		foreach (WaveEnemy wep in waveData.currentEnemies)
		{
			unavailableEnemiesList.Add(wep.enemyType);
		}
		int index = 0;
		for (int i = 0; i < totalEnemies.enemyDatas.Count; i++)
		{
			if (IsEnemyAvailable(totalEnemies.enemyDatas[i], unavailableEnemiesList)) {
				availableEnemiesList[index] = totalEnemies.enemyDatas[i];
				index++;
			}
		}
		return availableEnemiesList;
	}

	public bool IsEnemyAvailable(EnemyData enemy, List<EnemyData> unavailableList)
	{
		foreach (EnemyData data in unavailableList)
		{
			if (data.name == enemy.name) { return false; }
		}
		return true;
	}
}
