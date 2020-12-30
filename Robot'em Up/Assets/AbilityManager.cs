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

	private void Update ()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			Debug.Log("Current upgrades: " + SaveUpgrades());
		}
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
		//Get current ability level
		int currentAbilityLevel = (int)GetAbilityLevel(_ability);

		//Check that the upgrade is better than the current
		int upgradeValue = (int)_upgrade;
		if (upgradeValue > currentAbilityLevel)
		{
			//Unlocking upgrade
			//unlockedAbilities[_ability] = unlockedAbilities[_ability] + 1;
			unlockedAbilities[_ability] = _upgrade;
			SaveUpgrades();
			UpdateUpgrades();
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
			unlockedAbilities.Add(ability, Upgrade.Upgrade2);
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
	private static string SaveUpgrades()
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
		return str.ToString();
	}
	private static void GenerateNewDictionnary ()
	{
		unlockedAbilities = new Dictionary<ConcernedAbility, Upgrade>();
		foreach (ConcernedAbility ability in System.Enum.GetValues(typeof(ConcernedAbility)))
		{
			if (ability == ConcernedAbility.PerfectReception)
			{
				unlockedAbilities.Add(ability, Upgrade.Locked);
			}
			else
			{
				unlockedAbilities.Add(ability, Upgrade.Base);
			}
		}
	}
}
