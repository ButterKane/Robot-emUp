using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.Analytics;

public class AnalyticsManager
{
	static AnalyticsDatas datas;

	public static AnalyticsDatas GetDatas()
	{
		return Resources.Load<AnalyticsDatas>("AnalyticsDatas");
	}
	public static void IncrementData ( string _dataName, PlayerIndex _playerIndex, int _amount = 1 )
	{
		if (!datas) { datas = GetDatas(); }
		foreach (AnalyticsData data in datas.analyticsDatas)
		{
			data.totalValue += _amount;
			switch (_playerIndex)
			{
				case PlayerIndex.One:
					data.playerOneValue += _amount;
					break;
				case PlayerIndex.Two:
					data.playerTwoValue += _amount;
					break;
				default:
					break;
			}
		}
	}

	public static void IncrementData ( string _dataName, int _amount = 1 )
	{
		if (!datas) { datas = GetDatas(); }
		foreach (AnalyticsData data in datas.analyticsDatas)
		{
			if (data.dataName == _dataName)
			{
				data.totalValue += _amount;
			}
		}
	}

	public static void LoadDatas ()
	{
		foreach (AnalyticsData data in GetDatas().analyticsDatas)
		{
			if (PlayerPrefs.HasKey(data.dataName))
			{
				data.totalValue = PlayerPrefs.GetInt(data.dataName);
				data.playerOneValue = PlayerPrefs.GetInt(data.dataName + "[1]");
				data.playerTwoValue = PlayerPrefs.GetInt(data.dataName + "[2]");
			} else
			{
				data.totalValue = 0;
				data.playerOneValue = 0;
				data.playerTwoValue = 0;
			}
		}
	}

	public static void SaveDatas ()
	{
		foreach (AnalyticsData data in GetDatas().analyticsDatas)
		{
			PlayerPrefs.SetInt(data.dataName, data.totalValue);
			PlayerPrefs.SetInt(data.dataName + "[1]", data.playerOneValue);
			PlayerPrefs.SetInt(data.dataName + "[2]", data.playerTwoValue);
			PlayerPrefs.Save();
		}
	}

	public static void CleanData(AnalyticsData _data)
	{
		PlayerPrefs.DeleteKey(_data.dataName);
		PlayerPrefs.Save();
		LoadDatas();
	}
	
	public static void CleanDatas()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		LoadDatas();
	}

	public static void CleanZoneDatas (string _zoneName)
	{/*
		foreach (AnalyticsData data in GetDatas())
		{
			if (data.sortPerZone)
			{

			}
		}
        */
	}

	public static void CleanArenaDatas (string _arenaName)
	{

	}

	public static void SendData(AnalyticsData _data, string _additionalInformation)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (_data.perPlayer)
		{
			dictionary.Add("value", _data.playerOneValue);
			if (_data.sortPerArena)
			{
				dictionary.Add("arena", _additionalInformation);
			}
			if (_data.sortPerZone)
			{
				dictionary.Add("zone", _additionalInformation);
			}
			Analytics.CustomEvent(_data.dataName, dictionary);
			dictionary["value"] = _data.playerTwoValue;
			Analytics.CustomEvent(_data.dataName, dictionary);
		}
		else
		{
			dictionary.Add("value", _data.totalValue);
			if (_data.sortPerArena)
			{
				dictionary.Add("arena", _additionalInformation);
			}
			if (_data.sortPerZone)
			{
				dictionary.Add("zone", _additionalInformation);
			}
			Analytics.CustomEvent(_data.dataName, dictionary);
		}
	}
	public static void SendDatas ()
	{

	}
}
