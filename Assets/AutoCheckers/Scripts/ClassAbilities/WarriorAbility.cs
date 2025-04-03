using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WarriorAbility : MonoBehaviour, IAbility
{
    private static bool abilityActive = false;
    private readonly List<int> armor = new List<int>() { 0, 6, 14, 14 };

    private Hero hero;

    public static readonly int lvlThreshold = 3;
    public static readonly int maxLvl = 9;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes <= lvlThreshold)
            return;

        int level = heroes / lvlThreshold;

        hero.GainArmor(armor[level]);

        if (!abilityActive && heroes >= maxLvl)
        {
            abilityActive = true;
            ApplyArmorBoost(false);
        }
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes <= lvlThreshold)
            return;

        int level = heroes / lvlThreshold;

        hero.GainArmor(-armor[level]);

        if (abilityActive && heroes >= maxLvl)
        {
            abilityActive = false;
            ApplyArmorBoost(true);
        }
    }

    private void ApplyArmorBoost(bool isNegative)
    {
        int boostValue = isNegative ? -armor[1] : armor[1];
        foreach (GameObject piece in hero.Owner.HeroesOnBoard)
        {
            Hero ally = piece.GetComponent<Hero>();
            ally.GainArmor(boostValue);
        }
    }
    public int GetLvlThreshold()
    {
        return lvlThreshold;
    }
    public int GetMaxLvl()
    {
        return maxLvl;
    }
}
