using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAbility : MonoBehaviour, IAbility
{
    private readonly int silenceTime = 4;
    private readonly List<int> silenceChance = new List<int>() {0, 10, 20, 30};

    private Hero hero;
    private int currentLvL;

    public static readonly int lvlThreshold = 3;
    public static readonly int maxLvl = 9;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold)
            return;

        currentLvL = heroes / lvlThreshold;
        hero.GainOnAttackEvent(TryToSilence);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes < lvlThreshold)
            return;

        hero.RemoveOnAttackEvent(TryToSilence);
    }

    private void TryToSilence()
    {
        if (silenceChance[currentLvL] >= Random.Range(0, 100))
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
