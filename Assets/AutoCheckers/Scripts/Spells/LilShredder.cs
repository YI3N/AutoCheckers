using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilShredder : MonoBehaviour, ISpell
{
    private readonly List<int> damage = new List<int>() { 600, 900, 1200 };
    private readonly int cooldown = 10;

    private bool isCooldown = false;
    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public bool CanCast()
    {
        return !isCooldown;
    }

    public void CastSpell()
    {
        isCooldown = true;

        hero.PhysicalAttack(hero.TargetEnemy, hero.Damage * 6 + damage[hero.Upgrades]);

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }

    public void ResetAbility()
    {
        isCooldown = false;
    }

    public void PassiveSpell() { }
}
