using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MechAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 4;
    public static readonly int lvlThreshold = 2;
    private readonly List<int> healthRegeneration = new List<int>() { 0, 25 };

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

        hero.GainHealthRegeneration(healthRegeneration[level]);
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
