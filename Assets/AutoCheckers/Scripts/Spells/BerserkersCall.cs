using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkersCall : MonoBehaviour, ISpell
{
    private readonly int range = 1;
    private readonly List<int> bonusArmor = new List<int>() { 5, 10, 15 };
    private readonly List<int> duration = new List<int>() { 2, 3, 4 };
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

        hero.GainArmor(bonusArmor[hero.Upgrades]);
        List<Hero> heroes = Board.instance.GetHeroesInRange(hero.CurrentCell, range);
        foreach (Hero enemy in heroes)
        {
            if (enemy.tag != tag)
                enemy.SetTargetEnemy(hero);
        }

        StartCoroutine(Deactivate());
        StartCoroutine(Cooldown());
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(duration[hero.Upgrades]);
        if (isCooldown)
            hero.GainArmor(-bonusArmor[hero.Upgrades]);
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

    public void PassiveSpell() {}
}
