using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoneGaze : MonoBehaviour, ISpell
{
    private readonly float damageDebuff = .3f;
    private readonly int armorDebuff = 4;
    private readonly List<int> range = new List<int>() { 5, 6, 7 };
    private readonly List<int> gazeDuration = new List<int>() { 3, 4, 5 };
    private readonly List<int> stunDuration = new List<int>() { 2, 3, 4 };
    private readonly float stunAngle = 45f;
    private readonly int cooldown = 20;

    private bool isCooldown = false;
    private bool isAttack = false;
    private Hero hero;
    private Dictionary<Hero, int> gazedHeroes = new Dictionary<Hero, int>();

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

        StartCoroutine(ActivateGaze());
        StartCoroutine(Deactivate());
        StartCoroutine(Cooldown());
    }

    private IEnumerator ActivateGaze()
    {
        Vector2 lookDirection;
        if (hero.TargetEnemy != null)
            lookDirection = (hero.TargetEnemy.CurrentCell.GetBoardPosition() - hero.CurrentCell.GetBoardPosition()).normalized;
        else
        {
            Vector2 target = hero.Owner.Tag == AutoCheckers.GameTag.Human ? Vector2.up : Vector2.down;
            lookDirection = (target - hero.CurrentCell.GetBoardPosition()).normalized;
        }

        List<Hero> enemies = Board.instance.GetHeroesInRange(hero.CurrentCell, range[hero.Upgrades]).Where(h => h.tag != hero.tag).ToList();
        foreach (Hero enemy in enemies)
        {
            Vector2 toEnemy = (enemy.CurrentCell.GetBoardPosition() - hero.CurrentCell.GetBoardPosition()).normalized;
            float angle = Vector2.Angle(lookDirection, toEnemy);

            if (angle <= stunAngle)
            {
                if (gazedHeroes.ContainsKey(enemy) && gazedHeroes[enemy] <= 2)
                    gazedHeroes[enemy]++;
                else if (!gazedHeroes.ContainsKey(enemy))
                    gazedHeroes.Add(enemy, 1);
            }
            else if (gazedHeroes.ContainsKey(enemy))
            {
                gazedHeroes.Remove(enemy);
                enemy.GainDamage(damageDebuff);
            }
        }

        foreach (KeyValuePair<Hero, int> kvp in gazedHeroes)
        {
            Hero enemy = kvp.Key;
            
            if (kvp.Value == 1)
            {
                enemy.GainDamage(-damageDebuff);
            }
            else if (kvp.Value == 2)
            {
                enemy.GetStuned(stunDuration[hero.Upgrades]);
                enemy.GainArmor(-armorDebuff);
                StartCoroutine(RemoveArmorDebuff(enemy));
            }
        }

        yield return new WaitForSeconds(GameManager.instance.attackTime);

        if (isAttack && gameObject.activeSelf)
            StartCoroutine(ActivateGaze());
        else
            StartCoroutine(RemoveDamageDebuff());
    }

    private IEnumerator RemoveArmorDebuff(Hero enemy)
    {
        yield return new WaitForSeconds(stunDuration[hero.Upgrades]);
        enemy.GainArmor(armorDebuff);
    }

    private IEnumerator RemoveDamageDebuff()
    {
        yield return new WaitForSeconds(stunDuration[hero.Upgrades]);
        foreach (KeyValuePair<Hero, int> kvp in gazedHeroes)
        {
            Hero enemy = kvp.Key;
            enemy.GainDamage(damageDebuff);
        }
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(gazeDuration[hero.Upgrades]);
        if (isCooldown)
        {
            isAttack = false;
            foreach (KeyValuePair<Hero, int> kvp in gazedHeroes)
            {
                Hero enemy = kvp.Key;
                enemy.GainDamage(damageDebuff);
            }
        }
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }

    public void ResetAbility()
    {
        isCooldown = false;
        isAttack = false;
        gazedHeroes.Clear();
    }

    public void PassiveSpell() { }
}
