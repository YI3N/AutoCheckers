using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 3;
    public static readonly int lvlThreshold = 3;
    private readonly int silenceTime = 4;
    private readonly List<int> silenceChance = new List<int>() {0, 10};

    private Hero hero;
    private int currentLevel;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold)
            return;

        currentLevel = heroes / lvlThreshold;
        hero.GainOnAttackEvent(TryToSilence);
    }

    public void DeactivateAbility()
    {
        return;
    }

    private void TryToSilence()
    {
        if (silenceChance[currentLevel] >= Random.Range(0, 100))
            hero.TargetEnemy.GetSilenced(silenceTime);
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
