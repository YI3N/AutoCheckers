using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAbility : MonoBehaviour, IAbility
{
    private const int lvlThreshold = 3;
    private const int silenceTime = 4;
    private readonly List<int> silenceChance = new List<int>() {0, 10, 20, 30};

    private Hero hero;
    private int currentLvL;

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
}
