using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorAbility : MonoBehaviour, IAbility
{
    [SerializeField]
    private int lvlThreshold = 3;
    [SerializeField]
    private List<int> armor = new List<int>() { 5, 12, 21 };

    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        hero.GainArmor(armor[heroes/lvlThreshold]);
    }

    public void DeactivateAbility(int heroes)
    {
        hero.GainArmor(-armor[heroes / lvlThreshold]);
    }
}
