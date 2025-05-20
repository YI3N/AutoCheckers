using AutoCheckers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject abilityPrefab;

    [SerializeField]
    private TMP_Text playerName;
    [SerializeField]
    private TMP_Text exp;
    [SerializeField]
    private TMP_Text lvl;
    [SerializeField]
    private TMP_Text money;
    [SerializeField]
    private TMP_Text streak;
    [SerializeField]
    private TMP_Text win;
    [SerializeField]
    private TMP_Text health;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private GameObject abilitiesClass;
    [SerializeField]
    private GameObject abilitiesRace;

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    public void UpdatePlayerPanel(Player player)
    {
        exp.text = player.Exp.ToString() + "/" + player.MaxExp.ToString();
        lvl.text = "Óð. " + player.Level.ToString();
        money.text = player.Money.ToString();
        streak.text = player.WinStreak.ToString();
        win.text = player.Wins.ToString();
        health.text = player.CurrentHealth.ToString();
        healthBar.value = player.CurrentHealth / (float)player.MaxHealth;

        foreach (Transform child in abilitiesClass.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in abilitiesRace.transform)
        {
            Destroy(child.gameObject);
        }

        SetAbilities(player.ClassHeroes);
        SetAbilities(player.RaceHeroes);
    }

    private void SetAbilities<TKey>(Dictionary<TKey, int> abilities) where TKey : Enum
    {
        foreach (KeyValuePair<TKey, int> ability in abilities)
        {
            if (ability.Value <= 0)
                continue;

            var (threshold, maxLvl) = IExtensions.GetAbilityParameters(ability.Key);

            string amount;
            if (ability.Value < maxLvl)
            {
                int progress = 1 + ability.Value / threshold;
                amount = $"{ability.Value}/{threshold * progress}";
            }
            else
            {
                amount = $"{ability.Value}/{maxLvl}";
            }

            Enum abilityType = null;
            GameObject abilitiesSection = null;
            if (ability.Key is HeroClass heroClass)
            {
                abilityType = heroClass;
                abilitiesSection = abilitiesClass;

            }
            else if (ability.Key is Race race)
            {
                abilityType = race;
                abilitiesSection = abilitiesRace;
            }

            GameObject abilityIcon = Instantiate(abilityPrefab, abilitiesSection.transform);
            abilityIcon.GetComponent<AbilityIcon>().SetAbility(abilityType, amount);

        }
    }

    public string GetPlayerName()
    {
        return playerName.text;
    }
}
