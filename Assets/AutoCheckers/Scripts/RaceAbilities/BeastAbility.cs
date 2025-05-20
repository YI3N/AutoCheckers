using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BeastAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 2;
    public static readonly int lvlThreshold = 2;
    private readonly List<float> damage = new List<float>() { 0, 0.15f };

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
        ApplyDamageBonus();
    }

    public void DeactivateAbility()
    {
        if (abilityActive)
            abilityActive = false;
    }

    private void ApplyDamageBonus()
    {
        float damageBuff = damage[currentLevel];

        foreach (GameObject ally in hero.Owner.HeroesOnBoard)
        {
            ally.GetComponent<Hero>().GainDamage(damageBuff);
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
