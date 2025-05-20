using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AphoticShield : MonoBehaviour, ISpell
{
    private readonly int range = 3;
    private readonly List<int> absorb = new List<int>() { 100, 150, 200 };
    private readonly int duration = 10;
    private readonly int cooldown = 12;

    private bool isCooldown = false;
    private Hero hero;

    private bool isBlowUp = false;
    private int absorbLeft = 0;
    private Hero targetHero;

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
        isBlowUp = false;

        List<GameObject> heroes = hero.Owner.HeroesOnBoard.Where(p => p.activeSelf).ToList();

        float lowestHP = Mathf.Infinity;
        foreach (GameObject hero in heroes)
        {
            Hero ally = hero.GetComponent<Hero>();
            if (ally.CurrentHealth < lowestHP)
            {
                lowestHP = ally.CurrentHealth;
                targetHero = ally;
            }
        }

        absorbLeft = absorb[hero.Upgrades];
        targetHero.GainOnHitEvent(ShieldDamage);
        StartCoroutine(Deactivate());
        StartCoroutine(Cooldown());
    }

    private void ShieldDamage()
    {
        if (!isBlowUp)
        {
            absorbLeft -= targetHero.DamageTook;
            if (absorbLeft <= 0)
            {
                targetHero.RemoveOnHitEvent(ShieldDamage);
                targetHero.AbsorbDamage(targetHero.DamageTook - absorbLeft);
                absorbLeft = 0;
                BlowUp();
            }
            else
                targetHero.AbsorbDamage(targetHero.DamageTook);
        }
    }

    private void BlowUp()
    {
        if (!isBlowUp)
        {
            isBlowUp = true;
            List<Hero> enemies = Board.instance.GetHeroesInRange(targetHero.CurrentCell, range).Where(h => h.tag != hero.tag).ToList();
            foreach (Hero enemy in enemies)
            {
                hero.MagicalAttack(enemy, absorb[hero.Upgrades] - absorbLeft);
            }
        }
    }
    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(duration);
        if (!isBlowUp)
        {
            targetHero.RemoveOnHitEvent(ShieldDamage);
            BlowUp();
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
        isBlowUp = false;
    }

    public void PassiveSpell() { }
}
