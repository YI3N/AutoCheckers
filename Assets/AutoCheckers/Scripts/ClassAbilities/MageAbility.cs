using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MageAbility : MonoBehaviour, IAbility
{
    public static bool abilityActive = false;

    private const int lvlThreshold = 3;
    private readonly List<int> magicalResistanceDebuff = new List<int>() {0, -35, -90, -150 };

    private Hero hero;

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
}
