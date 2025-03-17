using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAbility : MonoBehaviour, IAbility
{
    [SerializeField]
    private int lvlThreshold = 3;
    [SerializeField]
    private int silenceTime = 4;
    [SerializeField]
    private List<int> silenceChance = new List<int>() { 20, 40, 60 };

    private Hero hero;
    private int currentLvL;
    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        currentLvL = heroes / lvlThreshold;
        hero.GainOnAttackEvent(TryToSilence);
    }

    public void DeactivateAbility(int heroes)
    {
        hero.RemoveOnAttackEvent(TryToSilence);
    }

    private void TryToSilence()
    {
        if (silenceChance[currentLvL] <= Random.Range(0, 100))
        {
            hero.TargetEnemy.GetSilenced(silenceTime);
        }
    }
}
