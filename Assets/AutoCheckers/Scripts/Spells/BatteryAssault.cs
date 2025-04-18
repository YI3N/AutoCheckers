using AutoCheckers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BatteryAssault : MonoBehaviour, ISpell
{
    private readonly int range = 1;
    private readonly List<int> damage = new List<int>() { 50, 75, 100 };
    private readonly List<int> cooldown = new List<int>() { 12, 10, 8 };
    private readonly int duration = 5;
    private readonly int interaval = 1;

    private bool isCooldown = false;
    private bool isAttack = false;
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
        isAttack = true;


        StartCoroutine(StartAssault());
        StartCoroutine(Deactivate());
        StartCoroutine(Cooldown());
    }

    public IEnumerator StartAssault()
    {
        List<Hero> enemies = Board.instance.GetHeroesInRange(hero.CurrentCell, range).Where(h => h.tag != hero.tag).ToList();

        if (enemies.Count != 0)
        {
            Hero enemy = enemies[Random.Range(0, enemies.Count - 1)];
            hero.MagicalAttack(enemy, damage[hero.Upgrades]);
        }

        yield return new WaitForSeconds(interaval);

        if (isAttack)
            StartCoroutine(StartAssault());
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(duration);
        isAttack = false;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown[hero.Upgrades]);
        isCooldown = false;
    }

    public void ResetAbility()
    {
        isAttack = false;
        isCooldown = false;
    }

    public void PassiveSpell() { }
}
