using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarlockAbility : MonoBehaviour, IAbility
{
    private readonly List<int> lifesteal = new List<int>() { 0, 10 };

    private Hero hero;
    private int currentLvL;

    public static readonly int lvlThreshold = 2;
    public static readonly int maxLvl = 2;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes < lvlThreshold)
            return;

        currentLvL = heroes / lvlThreshold;
        hero.GainOnAttackEvent(RestoreHealth);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes < lvlThreshold)
            return;

        currentLvL = heroes / lvlThreshold;
        hero.RemoveOnAttackEvent(RestoreHealth);
    }

    private void RestoreHealth()
    {
        int restore = Mathf.FloorToInt(hero.DamageDealt * (lifesteal[currentLvL] / 100f));
        hero.GainHealth(restore);
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
