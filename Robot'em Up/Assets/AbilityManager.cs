using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
	public static Dictionary<ConcernedAbility, Upgrade> unlockedAbilities = new Dictionary<ConcernedAbility, Upgrade>();
	public static AbilityManager instance;
	public float level1PassDamageMultiplier = 1.25f;
	public float level1PassDamageTreshold = 0.2f;

	private void Awake ()
	{
		instance = this;
	}
	private void Start ()
	{
		//Generates ability dictionnary with base values
		GenerateNewDictionnary();

		//Fill the ability dictionnary with saved upgrades
		LoadUpgrades();

		//Saves the new dictionnary to player prefs
		SaveUpgrades();
	}

	public static Upgrade GetAbilityLevel (ConcernedAbility _ability)
	{
		if (unlockedAbilities.ContainsKey(_ability)) {
			return unlockedAbilities[_ability];
		} else
		{
			return Upgrade.Base;
		}
	}

    public static bool IsAbilityUnlocked(ConcernedAbility _ability)
    {
        if (GetAbilityLevel(_ability) >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
	public static void UpgradeAbility(ConcernedAbility _ability, Upgrade _upgrade)
	{
		//Check that the upgrade is better than the current
		int upgradeValue = (int)_upgrade;
		if (upgradeValue > 0)
		{
			//Unlocking upgrade
			unlockedAbilities[_ability] = unlockedAbilities[_ability] + 1;
			SaveUpgrades();
			UpdateUpgrades();
			Debug.Log("Upgraded " + _ability + " with " + _upgrade);
		}
	}
	public static void ResetUpgrades()
	{
		GenerateNewDictionnary();
		SaveUpgrades();
		UpdateUpgrades();
	}
	public static void UnlockAllAbilities()
	{
		unlockedAbilities = new Dictionary<ConcernedAbility, Upgrade>();
		foreach (ConcernedAbility ability in System.Enum.GetValues(typeof(ConcernedAbility)))
		{
			unlockedAbilities.Add(ability, Upgrade.Upgrade3);
		}
		SaveUpgrades();
		UpdateUpgrades();
	}


	private static void UpdateUpgrades() //Recalculate UIs and upgrade-specific elements
	{
		if (GameManager.playerOne != null && GameManager.playerTwo != null)
		{
			GameManager.playerOne.dashController.CheckForUpgrades();
			GameManager.playerTwo.dashController.CheckForUpgrades();
		}
	}
	private static void LoadUpgrades()
	{
		string savedUpgrades = PlayerPrefs.GetString("savedUpgrades");
		if (savedUpgrades != default && savedUpgrades != "") //If saved datas are found
		{
			//Example of string retrieved from playerPrefs: Dash=Base,Pass=Upgrade1,PerfectReception=Upgrade2
			string[] upgradesValues = savedUpgrades.Split(char.Parse(","));
			foreach (string s in upgradesValues)
			{
				if (s != "")
				{
					string[] splitValue = s.Split(char.Parse("="));
					string abilityName = splitValue[0];
					string upgradeName = splitValue[1];
					ConcernedAbility foundAbility = default;
					Upgrade foundUpgrade = default;
					if (System.Enum.TryParse<ConcernedAbility>(abilityName, out foundAbility) && System.Enum.TryParse<Upgrade>(upgradeName, out foundUpgrade))
					{
						unlockedAbilities[foundAbility] = foundUpgrade;
					}
				}
			}
		}
		UpdateUpgrades();
	}
	private static void SaveUpgrades()
	{
		StringBuilder str = new StringBuilder();
		foreach (KeyValuePair<ConcernedAbility, Upgrade> pair in unlockedAbilities)
		{
			str.Append(pair.Key);
			str.Append("=");
			str.Append(pair.Value);
			str.Append(",");
		}
		PlayerPrefs.SetString("savedUpgrades", str.ToString());
	}
	private static void GenerateNewDictionnary ()
	{
		unlockedAbilities = new Dictionary<ConcernedAbility, Upgrade>();
		foreach (ConcernedAbility ability in System.Enum.GetValues(typeof(ConcernedAbility)))
		{
			unlockedAbilities.Add(ability, Upgrade.Base);
		}
	}
}
