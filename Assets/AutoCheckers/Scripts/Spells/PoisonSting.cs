using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSting: MonoBehaviour, ISpell
{
    private readonly List<int> damage = new List<int>() { 10, 20, 30 };

    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public bool CanCast()
    {
        return false;
    }

    public void CastSpell() { }

    public void PassiveSpell()
    {
        if (hero.TargetEnemy != null)
            hero.MagicalAttack(hero.TargetEnemy, damage[hero.Upgrades]);
    }

    public void ResetAbility() { }
}
