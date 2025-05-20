using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrollAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 2;
    public static readonly int lvlThreshold = 2;
    private readonly List<float> damage = new List<float>() { 0, 0.35f };

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
