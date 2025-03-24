using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WarriorAbility : MonoBehaviour, IAbility
{
    private const int lvlThreshold = 3;
    private readonly List<int> armor = new List<int>() { 0, 5, 12, 21 };

    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        int level = heroes / lvlThreshold;
        if (level <= 0)
            return;

        hero.GainArmor(armor[level]);
    }

    public void DeactivateAbility(int heroes)
    {
        int level = heroes / lvlThreshold;
        if (level <= 0)
            return;

        hero.GainArmor(-armor[level]);
    }
}
