using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LagunaBlade : MonoBehaviour, ISpell
{
    private readonly List<int> damage = new List<int>() { 500, 750, 1000 };
    private readonly List<int> cooldown = new List<int>() { 10, 7, 5 };

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

        hero.MagicalAttack(hero.TargetEnemy, damage[hero.Upgrades]);

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown[hero.Upgrades]);
        isCooldown = false;
    }

    public void ResetAbility()
    {
        isCooldown = false;
    }

    public void PassiveSpell() { }
}
