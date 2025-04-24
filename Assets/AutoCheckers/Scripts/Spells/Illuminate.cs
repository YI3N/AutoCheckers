using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Illuminate : MonoBehaviour, ISpell
{
    private readonly int castTime = 3;
    private readonly List<int> damage = new List<int>() { 300, 450, 600 };
    private readonly List<int> cooldown = new List<int>() { 10, 8, 6 };

    private bool isCooldown = false;
    private Hero hero;
    private BoardCell target;

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

        target = hero.TargetEnemy.CurrentCell;

        hero.GetStuned(castTime);
        StartCoroutine(StopCast());
        StartCoroutine(Cooldown());
    }

    private IEnumerator StopCast()
    {
        yield return new WaitForSeconds(castTime);
        if (gameObject.activeSelf)
        {
            List<Hero> enemiesLine = Board.instance.GetHeroesInLine(hero.CurrentCell, target, 3).Where(h => h.tag != hero.tag).OrderBy(h =>
                Mathf.FloorToInt(Vector2.Distance(hero.CurrentCell.GetBoardPosition(), h.CurrentCell.GetBoardPosition()))).ToList();

            foreach (Hero enemy in enemiesLine)
            {
                hero.MagicalAttack(enemy, damage[hero.Upgrades]);
            }
        }
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
