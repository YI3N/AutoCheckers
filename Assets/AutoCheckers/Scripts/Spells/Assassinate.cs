using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Assassinate : MonoBehaviour, ISpell
{
    private readonly List<int> range = new List<int>() { 5, 6, 7 };
    private readonly List<int> damage = new List<int>() { 400, 600, 800 };
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

        Hero targetHero = null;
        List<Hero> enemies = Board.instance.GetHeroesInRange(hero.CurrentCell, range[hero.Upgrades]).Where(h => h.tag != hero.tag).ToList();

        if (hero.IsTargetLowest)
        {
            float lowestHP = Mathf.Infinity;
            
            foreach (Hero enemy in enemies)
            {
                if (enemy.CurrentHealth < lowestHP)
                {
                    lowestHP = enemy.CurrentHealth;
                    targetHero = enemy;
                }
            }
        }
        else
        {
            float nearestDistance = Mathf.Infinity;
            foreach (Hero enemy in enemies)
            {
                float distance = Mathf.FloorToInt(Vector2.Distance(hero.CurrentCell.GetBoardPosition(), enemy.CurrentCell.GetBoardPosition()));
                if (distance < nearestDistance)
                {
                    targetHero = enemy;
                    nearestDistance = distance;
                }
            }
        }

        hero.MagicalAttack(targetHero, damage[hero.Upgrades]);

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
