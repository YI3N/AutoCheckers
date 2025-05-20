using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WarriorAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 6;
    public static readonly int lvlThreshold = 3;
    private readonly List<int> armor = new List<int>() { 0, 6, 14};

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

        hero.GainArmor(armor[level]);
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
