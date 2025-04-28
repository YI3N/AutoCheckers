using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningSpear : MonoBehaviour, ISpell
{
    private readonly List<int> damage = new List<int>() { 15, 20, 35 };
    private readonly int time = 5;

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
            StartCoroutine(FireDamage(hero.TargetEnemy));
    }

    private IEnumerator FireDamage(Hero enemy)
    {
        for (int i = 0; i < time; i++)
        {
            hero.MagicalAttack(enemy, damage[hero.Upgrades]);
            yield return new WaitForSeconds(GameManager.instance.attackTime);
        }
    }

    public void ResetAbility() { }
}
