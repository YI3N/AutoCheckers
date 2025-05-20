using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 3;
    public static readonly int lvlThreshold = 3;
    private readonly List<int> armor = new List<int>() { 0, 15 };
    private readonly List<int> healthRegeneration = new List<int>() { 0, 20 };

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
        ApplyBuffToRandomHero();
    }

    public void DeactivateAbility()
    {
        if (abilityActive)
            abilityActive = false;
    }

    private void ApplyBuffToRandomHero()
    {
        GameObject piece = hero.Owner.HeroesOnBoard[Random.Range(0, hero.Owner.HeroesOnBoard.Count - 1)];
        Hero ally = piece.GetComponent<Hero>();

        ally.GainArmor(armor[currentLevel]);
        ally.GainHealthRegeneration(healthRegeneration[currentLevel]);
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
