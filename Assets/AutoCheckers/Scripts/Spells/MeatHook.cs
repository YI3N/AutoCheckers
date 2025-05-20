using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeatHook : MonoBehaviour, ISpell
{
    private readonly List<int> damage = new List<int>() { 100, 200, 300 };
    private readonly List<int> cooldown = new List<int>() { 10, 8, 6 };

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
        List<GameObject> enemies = hero.Opponent.HeroesOnBoard.Where(h => h.activeSelf).ToList();


        float farestDistance = 0;
        foreach (GameObject piece in enemies)
        {
            Hero enemy = piece.GetComponent<Hero>();

            float distance = Mathf.FloorToInt(Vector2.Distance(hero.CurrentCell.GetBoardPosition(), enemy.CurrentCell.GetBoardPosition()));
            if (distance > farestDistance)
            {
                targetHero = enemy;
                farestDistance = distance;
            }
        }

        List<Hero> enemiesLine = Board.instance.GetHeroesInLine(hero.CurrentCell, targetHero.CurrentCell, 1).Where(h => h.tag != hero.tag).OrderBy(h => 
            Mathf.FloorToInt(Vector2.Distance(hero.CurrentCell.GetBoardPosition(), h.CurrentCell.GetBoardPosition()))).ToList();

        if (enemiesLine.Count != 0)
        {
            targetHero = enemiesLine[0];
            hero.MagicalAttack(targetHero, damage[hero.Upgrades]);

            BoardCell moveCell = Board.instance.GetFirstAvailableCell(hero.CurrentCell);
            if (moveCell != null)
                targetHero.Move(moveCell);
        }
        

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
