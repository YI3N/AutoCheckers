using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechAbility : MonoBehaviour, IAbility
{
    private readonly List<int> healthRegeneration = new List<int>() { 0, 25 };

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

        hero.GainHealthRegeneration(healthRegeneration[level]);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes <= lvlThreshold)
            return;

        int level = heroes / lvlThreshold;

        hero.GainHealthRegeneration(-healthRegeneration[level]);
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
