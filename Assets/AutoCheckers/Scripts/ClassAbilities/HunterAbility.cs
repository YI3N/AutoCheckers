using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterAbility : MonoBehaviour, IAbility
{
    private readonly List<int> damage = new List<int>() { 0, 15, 30 };

    private Hero hero;

    public static readonly int lvlThreshold = 2;
    public static readonly int maxLvl = 4;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes <= lvlThreshold)
            return;

        int level = heroes / lvlThreshold;

        hero.GainDamage(damage[level]);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes <= lvlThreshold)
            return;

        int level = heroes / lvlThreshold;

        hero.GainDamage(damage[level]);
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
