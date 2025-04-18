using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcAbilitiy : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 2;
    public static readonly int lvlThreshold = 2;
    private readonly List<int> hpBoost = new List<int>() {0, 150};
    
    private static bool abilityActive = false;

    private Hero hero;
    private int currentLevel;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold || abilityActive)
            return;

        abilityActive = true;
        currentLevel = heroes / lvlThreshold;
        ApplyHpBoost();
    }

    public void DeactivateAbility()
    {
        if (abilityActive)
            abilityActive = false;
    }

    private void ApplyHpBoost()
    {
        int boostValue = hpBoost[currentLevel];

        foreach (GameObject ally in hero.Owner.HeroesOnBoard)
        {
            ally.GetComponent<Hero>().GainMaxHealth(boostValue);
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
