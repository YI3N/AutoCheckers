using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CallDown : MonoBehaviour, ISpell
{
    private readonly int range = 4;
    private readonly List<int> damage = new List<int>() { 200, 300, 400 };
    private readonly List<int> cooldown = new List<int>() { 10, 8, 6 };

    private bool isCooldown = false;
    private BoardCell target;
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

        if (hero.TargetEnemy != null)
            target = hero.TargetEnemy.CurrentCell;

        StartCoroutine(DropTheBombs());
        StartCoroutine(Cooldown());
    }

    private IEnumerator DropTheBombs()
    {
        List<Hero> enemies = Board.instance.GetHeroesInRange(target, range).Where(h => h.tag != hero.tag).ToList();

        foreach (Hero enemy in enemies)
            hero.MagicalAttack(enemy, damage[hero.Upgrades]);

        yield return new WaitForSeconds(GameManager.instance.attackTime);

        enemies = Board.instance.GetHeroesInRange(target, range).Where(h => h.tag != hero.tag).ToList();

        foreach (Hero enemy in enemies)
            hero.MagicalAttack(enemy, damage[hero.Upgrades]);
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown[hero.Upgrades]);
        isCooldown = false;
    }

    public void ResetAbility()
    {
        isCooldown = false;
        target = null;
    }

    public void PassiveSpell() { }
}
