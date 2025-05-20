using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WarlockAbility : MonoBehaviour, IAbility
{
    public static readonly int maxLvl = 2;
    public static readonly int lvlThreshold = 2;
    private readonly List<int> lifesteal = new List<int>() { 0, 10 };

    private Hero hero;
    private int currentLvL;

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

    public void DeactivateAbility()
    {
        return;
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
