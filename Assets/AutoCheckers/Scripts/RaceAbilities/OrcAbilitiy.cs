using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcAbilitiy : MonoBehaviour, IAbility
{
    private static bool abilityActive = false;

    private const int lvlThreshold = 2;
    private readonly List<int> hpBoost = new List<int>() {0, 100, 350, 350 };

    private Hero hero;

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
        ApplyHpBoost(level);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes < lvlThreshold || !abilityActive)
            return;


        abilityActive = false;
        int level = heroes / lvlThreshold;
        ApplyHpBoost(-level);
    }

    private void ApplyHpBoost(int level)
    {
        int boostValue = hpBoost[level];
        if (level >= 3)
        {
            foreach (GameObject allyObj in hero.Owner.HeroesOnBoard)
            {
                Hero ally = allyObj.GetComponent<Hero>();
                ally.GainMaxHealth(boostValue + (ally.Owner.MaxHealth - ally.Owner.CurrentHealth) * 8);
            }
        }
        else
        {
            foreach (GameObject allyObj in hero.Owner.HeroesOnBoard)
            {
                Hero ally = allyObj.GetComponent<Hero>();
                ally.GainMaxHealth(boostValue);
            }
        }
    }
}
