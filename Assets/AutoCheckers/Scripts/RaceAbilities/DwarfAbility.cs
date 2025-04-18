using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwarfAbility : MonoBehaviour
{
    public static readonly int maxLvl = 2;
    public static readonly int lvlThreshold = 2;
    private readonly List<int> range = new List<int>() { 0, 1 };

    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold)
            return;

        int level = heroes / lvlThreshold;
        hero.GainAttackRange(range[level]);
        hero.GainTargetLowest();
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
