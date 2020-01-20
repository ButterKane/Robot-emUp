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
		this.serializedObject.Update();
		if (waveEditor.waveList.Count > 0)
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUIStyle i_headerStyle = new GUIStyle(EditorStyles.helpBox);
			i_headerStyle.alignment = TextAnchor.MiddleCenter;
			i_headerStyle.fontSize = 20;
			i_headerStyle.fontStyle = FontStyle.Bold;
			GUILayout.Box("Global settings", i_headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			SerializedProperty m_startOnTriggerEnter = serializedObject.FindProperty("startOnTriggerEnter");
			EditorGUILayout.PropertyField(m_startOnTriggerEnter, new GUIContent("Start automatically when players enters zone ?"));
			this.serializedObject.ApplyModifiedProperties();

			EditorGUI.BeginChangeCheck();
			SerializedProperty m_exitDoor = serializedObject.FindProperty("exitDoor");
			EditorGUILayout.PropertyField(m_exitDoor, true);
			this.serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				UpdateDoorCounter();
			}
			GUILayout.Space(10);
			GUILayout.EndVertical();
			GUILayout.Space(20);


			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUILayout.Box("Spawns", i_headerStyle);
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
			EditorGUILayout.PropertyField(m_events, true);
		}
		for (int i = 0; i < waveEditor.waveList.Count; i++)
		{
			WaveData i_waveData = waveEditor.waveList[i];
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.color = Color.gray;
			GUILayout.BeginHorizontal();
			GUIStyle i_headerStyle = new GUIStyle(EditorStyles.helpBox);
			i_headerStyle.alignment = TextAnchor.MiddleCenter;
			i_headerStyle.fontSize = 20;
			i_headerStyle.fontStyle = FontStyle.Bold;
			GUILayout.Box("Wave " + (i + 1).ToString(), i_headerStyle);
			GUI.color = Color.white;
			if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(30), GUILayout.Height(30)))
			{
				RemoveWave(i_waveData);
			}
			GUILayout.EndHorizontal();

			//"Wave global settings" panel
			GUILayout.Label("Global settings", EditorStyles.centeredGreyMiniLabel);
			i_waveData.maxPowerLevel = EditorGUILayout.CurveField("Max power level", i_waveData.maxPowerLevel);
			i_waveData.duration = EditorGUILayout.FloatField("Duration", i_waveData.duration) ;
			i_waveData.pauseBeforeNextWave = EditorGUILayout.FloatField("Pause before next wave", i_waveData.pauseBeforeNextWave);

			//"Wave enemy ratio visualizers" panel
			GUILayout.Space(10);
			GUILayout.Label("Enemy spawn chances", EditorStyles.centeredGreyMiniLabel);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			for (int y = 0; y < i_waveData.currentEnemies.Count; y++)
			{
				GUIStyle visualizerStyle = new GUIStyle(GUI.skin.textField);
				GUI.color = i_waveData.currentEnemies[y].enemyType.color;
				visualizerStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.TextField(i_waveData.currentEnemies[y].enemyType.name, visualizerStyle, GUILayout.Width(300 * i_waveData.currentEnemies[y].probability), GUILayout.Height(30));
				GUI.color = i_waveData.currentEnemies[y].enemyType.color;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			//"Current enemies" panel
			GUI.color = Color.white;
			GUILayout.BeginVertical();
			for (int y = 0; y < i_waveData.currentEnemies.Count; y++)
			{
				GUILayout.BeginHorizontal();
				GUI.color = i_waveData.currentEnemies[y].enemyType.color;
				GUILayout.Label(EditorGUIUtility.IconContent("Canvas Icon"), GUILayout.Width(30), GUILayout.Height(30));
				GUI.color = Color.white;
				GUILayout.BeginVertical();
				GUILayout.Box(waveEditor.waveList[i].currentEnemies[y].enemyType.name, EditorStyles.miniButton);
				GUILayout.Box("Power level: "+waveEditor.waveList[i].currentEnemies[y].enemyType.prefab.GetComponent<EnemyBehaviour>().powerLevel, EditorStyles.centeredGreyMiniLabel);
				GUILayout.EndVertical();
				EditorGUI.BeginChangeCheck();
				float probaSliderValue = EditorGUILayout.Slider(Mathf.Round(i_waveData.currentEnemies[y].probability * 100f) / 100f, 0f, 1f);
				if (EditorGUI.EndChangeCheck())
				{
					i_waveData.SetEnemySpawnProbability(i_waveData.currentEnemies[y].enemyType, probaSliderValue);
				}
				if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
				{
					i_waveData.RemoveEnemySpawnProbability(i_waveData.currentEnemies[y]);
					UpdateAvailableEnemies(i_waveData);
					return;
				}
				for (int z = 0; z < waveEditor.spawnList.Count; z++) {
					GUI.color = waveEditor.spawnList[z].color;
					GUIStyle buttonStyle = new GUIStyle(EditorStyles.boldLabel);
					if (GUILayout.Button(Resources.Load<Texture2D>("WaveToolSprites/WhiteSquareTransparentBorder"), buttonStyle, GUILayout.Width(30), GUILayout.Height(30)))
					{
						if (i_waveData.currentEnemies[y].spawnIndexes.Contains(z))
						{
							i_waveData.currentEnemies[y].spawnIndexes.Remove(z);
						} else
						{
							i_waveData.currentEnemies[y].spawnIndexes.Add(z);
						}
					}
					GUI.color = Color.white;
					if (i_waveData.currentEnemies[y].spawnIndexes.Contains(z))
					{
						GUI.Label(GUILayoutUtility.GetLastRect(), Resources.Load<Texture2D>("WaveToolSprites/WhiteOutline"));
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
			}
			GUILayout.EndVertical();

			//"Add enemy" panel
			if (enemyAvailable[i_waveData] != null)
			{
				if (enemyAvailable[i_waveData].Length > 0)
				{
					GUILayout.Space(10);
					GUILayout.Label("Add enemy type", EditorStyles.centeredGreyMiniLabel );
					GUILayout.BeginHorizontal();
					enemyFoldoutIndex[i_waveData] = EditorGUILayout.Popup(enemyFoldoutIndex[i_waveData], EnemyListToString(enemyAvailable[i_waveData]));
					if (GUILayout.Button("Add"))
					{
						AddEnemy(i_waveData, enemyFoldoutIndex[i_waveData]);
					}
					GUILayout.EndHorizontal();
				}
			}

			//"onEndActions" panel
			GUILayout.Space(10);
			GUILayout.Label("Actions on wave end", EditorStyles.centeredGreyMiniLabel);
			SerializedProperty m_events = serializedObject.FindProperty("waveList.Array.data[" + i + "].onEndEvents");
			EditorGUILayout.PropertyField(m_events, true);
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
		EditorUtility.SetDirty(target);
		this.serializedObject.ApplyModifiedProperties();

	}

	void AddWave()
	{
		WaveData i_newWave = new WaveData();
		AnimationCurve i_newCurve = new AnimationCurve();
		i_newCurve.AddKey(new Keyframe(0, 1));
		i_newCurve.AddKey(new Keyframe(1, 1));
		i_newWave.maxPowerLevel = i_newCurve;
		waveEditor.waveList.Add(i_newWave);
		enemyFoldoutIndex.Add(i_newWave, 0);
		enemyAvailable.Add(i_newWave, new EnemyData[0]);
		UpdateAvailableEnemies(i_newWave);
		UpdateDoorCounter();
	}

	void RemoveWave(WaveData _waveData)
	{
		waveEditor.waveList.Remove(_waveData);
		UpdateDoorCounter();
	}

	void UpdateAvailableEnemies(WaveData _wave)
	{
		enemyAvailable[_wave] = GetAvailableEnemies(_wave);
	}

	void AddEnemy ( WaveData _wave, int _index)  
	{
		if (enemyAvailable[_wave][_index] == null) { return; }
		WaveEnemy i_newWEP = new WaveEnemy();
		i_newWEP.enemyType = enemyAvailable[_wave][_index];
		if (_wave.currentEnemies.Count <= 0)
		{
			i_newWEP.probability = 1;
		} else
		{
			i_newWEP.probability = 0;
		}
		i_newWEP.spawnIndexes = new List<int>();
		_wave.currentEnemies.Add(i_newWEP);
		UpdateAvailableEnemies(_wave); 
	}

	void AddSpawn()
	{
		GameObject i_newSpawn = new GameObject();
		i_newSpawn.name = "Wave-Spawn [" + (waveEditor.spawnList.Count + 1) + "]";
		i_newSpawn.transform.SetParent(waveEditor.transform);
		i_newSpawn.transform.position = waveEditor.GetComponentInChildren<CameraZone>().GetCenterPosition();
		Texture2D tex = EditorGUIUtility.IconContent("sv_label_0").image as Texture2D; 
		Type editorGUIUtilityType = typeof(EditorGUIUtility);
		BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
		object[] args = new object[] { i_newSpawn, tex };
		editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
		SpawnInformation i_newSpawnInf = new SpawnInformation();
		i_newSpawnInf.transform = i_newSpawn.transform;
		i_newSpawnInf.customName = "SpawnName";
		i_newSpawnInf.color = Color.white;
		waveEditor.spawnList.Add(i_newSpawnInf);
		this.serializedObject.Update();
	}

	public string[] EnemyListToString(EnemyData[] _enemyList)
	{
		string[] finalString = new string[_enemyList.Length];
		for (int i = 0; i < _enemyList.Length; i++)
		{
			if (_enemyList[i] == null) { finalString[i] = ""; }
			else
			{
				finalString[i] = _enemyList[i].name;
			}
		}
		return finalString;
	}

	void UpdateDoorCounter()
	{
		if (waveEditor.exitDoor != null) { if (waveEditor.exitDoor.currentCounter != null) { waveEditor.exitDoor.currentCounter.isWaveCounter = true; waveEditor.exitDoor.currentCounter.Generate(waveEditor.waveList.Count); } }
	}

	public EnemyData[] GetAvailableEnemies(WaveData _waveData)
	{
		EnemyDatas i_totalEnemies = enemyDatas;
		int enemyCount = i_totalEnemies.enemyDatas.Count - _waveData.currentEnemies.Count;
		if (enemyCount <= 0) { return new EnemyData[0]; }
		EnemyData[] i_availableEnemiesList = new EnemyData[enemyCount]; 
		List<EnemyData> i_unavailableEnemiesList = new List<EnemyData>();
		foreach (WaveEnemy wep in _waveData.currentEnemies)
		{
			i_unavailableEnemiesList.Add(wep.enemyType);
		}
		int index = 0;
		for (int i = 0; i < i_totalEnemies.enemyDatas.Count; i++)
		{
			if (IsEnemyAvailable(i_totalEnemies.enemyDatas[i], i_unavailableEnemiesList)) {
				i_availableEnemiesList[index] = i_totalEnemies.enemyDatas[i];
				index++;
			}
		}
		return i_availableEnemiesList;
	}

	public bool IsEnemyAvailable(EnemyData _enemy, List<EnemyData> _unavailableList)
	{
		foreach (EnemyData data in _unavailableList)
		{
			if (data.name == _enemy.name) { return false; }
		}
		return true;
	}
}
