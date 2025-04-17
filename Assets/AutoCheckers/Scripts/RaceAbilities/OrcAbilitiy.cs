using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcAbilitiy : MonoBehaviour, IAbility
{
    private static bool abilityActive = false;
    private readonly List<int> hpBoost = new List<int>() {0, 150};

    private Hero hero;

    public static readonly int lvlThreshold = 2;
    public static readonly int maxLvl = 2;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold || abilityActive)
            return;

        abilityActive = true;
        int level = heroes / lvlThreshold;
        ApplyHpBoost(level, false);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes < lvlThreshold || !abilityActive)
            return;

        abilityActive = false;
        int level = heroes / lvlThreshold;
        ApplyHpBoost(level, true);
    }

    private void ApplyHpBoost(int level, bool isNegative)
    {
        int boostValue = isNegative ? -hpBoost[level] : hpBoost[level];

        foreach (GameObject piece in hero.Owner.HeroesOnBoard)
        {
            Hero ally = piece.GetComponent<Hero>();
            ally.GainMaxHealth(boostValue);
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
