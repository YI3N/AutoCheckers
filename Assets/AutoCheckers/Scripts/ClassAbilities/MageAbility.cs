using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MageAbility : MonoBehaviour, IAbility
{
    private static bool abilityActive = false;
    private readonly List<int> magicalResistanceDebuff = new List<int>() {0, -33, -66, -99 };

    private Hero hero;

    public static readonly int lvlThreshold = 3;
    public static readonly int maxLvl = 9;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold || abilityActive)
            return;

        abilityActive = true;
        ApplyMagicalResistanceDebuff(heroes);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes < lvlThreshold || !abilityActive)
            return;

        abilityActive = false;
        ApplyMagicalResistanceDebuff(-heroes);
    }

    private void ApplyMagicalResistanceDebuff(int heroes)
    {
        int debuffValue = magicalResistanceDebuff[Mathf.Min(heroes / lvlThreshold, magicalResistanceDebuff.Count - 1)];

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
