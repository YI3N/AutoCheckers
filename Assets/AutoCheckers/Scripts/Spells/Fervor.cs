using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fervor : MonoBehaviour, ISpell
{
    private readonly List<int> damage = new List<int>() { 15, 20, 25 };
    private readonly List<int> maxStacks = new List<int>() { 15, 20, 25 };

    private Hero hero;
    private Hero target = null;
    private int stacks = 1;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    void Start()
    {
        hero.GainOnAttackEvent(StackOnHit);
    }

    private void StackOnHit()
    {
        if (target != null)
        {
            if (target == hero.TargetEnemy)
            {
                hero.GainDamage(damage[hero.Upgrades] * stacks);
                stacks++;
                stacks = Mathf.Clamp(stacks, 0, maxStacks[hero.Upgrades]);
            }
            else
            {
                hero.GainDamage(-damage[hero.Upgrades] * stacks - 1);
                stacks = 1;
                target = hero.TargetEnemy;
            }
        }
        else
        {
            target = hero.TargetEnemy;
        }
    }

    public bool CanCast()
    {
        return false;
    }

    public void CastSpell() { }

    public void PassiveSpell() { }

    public void ResetAbility()
    {
        target = null;
        stacks = 1;
    }
}
