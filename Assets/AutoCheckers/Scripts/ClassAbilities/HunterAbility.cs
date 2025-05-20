using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 4;
    public static readonly int lvlThreshold = 2;

    private readonly List<int> damage = new List<int>() { 0, 15, 30 };

    private Hero hero;
    

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
    public void DeactivateAbility()
    {
        return;
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
