using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndeadAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 4;
    public static readonly int lvlThreshold = 2;
    private readonly List<int> armorDebuff = new List<int>() { 0, -4, -8};

    private static bool abilityActive = false;

    private Hero hero;
    private int currentLevel;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold || abilityActive)
            return;

        abilityActive = true;
        currentLevel = heroes / lvlThreshold;
        ApplyArmorDebuff();
    }

    public void DeactivateAbility()
    {
        if (abilityActive)
            abilityActive = false;
    }

    private void ApplyArmorDebuff()
    {
        int debuffValue = armorDebuff[currentLevel];

        foreach (GameObject enemy in hero.Opponent.HeroesOnBoard)
        {
            enemy.GetComponent<Hero>().GainArmor(debuffValue);
        }
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
