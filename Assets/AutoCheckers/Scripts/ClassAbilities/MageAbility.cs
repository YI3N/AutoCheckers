using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MageAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 3;
    public static readonly int lvlThreshold = 3;
    private readonly List<int> magicalResistanceDebuff = new List<int>() {0, -33};

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
        ApplyMagicalResistanceDebuff();
    }

    public void DeactivateAbility()
    {
        if (abilityActive)
            abilityActive = false;
    }

    private void ApplyMagicalResistanceDebuff()
    {
        int debuffValue = magicalResistanceDebuff[currentLevel];

        foreach (GameObject enemy in hero.Opponent.HeroesOnBoard)
        {
            enemy.GetComponent<Hero>().GainMagicalResistance(debuffValue);
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
