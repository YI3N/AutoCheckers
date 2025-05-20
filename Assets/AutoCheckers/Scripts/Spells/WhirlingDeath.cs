using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhirlingDeath : MonoBehaviour, ISpell
{
    private readonly int range = 2;
    private readonly List<int> damage = new List<int>() { 150, 200, 250 };
    private readonly List<int> cooldown = new List<int>() { 6, 5, 4 };

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
            hero.PureAttack(enemy, damage[hero.Upgrades]);

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
