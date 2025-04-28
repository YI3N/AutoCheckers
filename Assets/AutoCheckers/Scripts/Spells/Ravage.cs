using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ravage : MonoBehaviour, ISpell
{
    private readonly int range = 6;
    private readonly List<int> damage = new List<int>() { 150, 250, 350 };
    private readonly List<int> stunTime = new List<int>() { 2, 3, 3 };
    private readonly int cooldown = 30;

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

        List<Hero> enemies = Board.instance.GetHeroesInRange(hero.CurrentCell, range).Where(h => h.tag != hero.tag).ToList();

        foreach (Hero enemy in enemies)
        {
            hero.MagicalAttack(enemy, damage[hero.Upgrades]);
            enemy.GetStuned(stunTime[hero.Upgrades]);
        }

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
