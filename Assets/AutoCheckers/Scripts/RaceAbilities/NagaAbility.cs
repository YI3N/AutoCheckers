using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NagaAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 2;
    public static readonly int lvlThreshold = 2;
    private readonly List<int> magicalResistance = new List<int>() { 0, 35 };

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
        ApplyMagicalResistance();
    }

    public void DeactivateAbility()
    {
        if (abilityActive)
            abilityActive = false;
    }

    private void ApplyMagicalResistance()
    {
        int buffValue = magicalResistance[currentLevel];

        foreach (GameObject ally in hero.Owner.HeroesOnBoard)
        {
            ally.GetComponent<Hero>().GainMagicalResistance(buffValue);
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
