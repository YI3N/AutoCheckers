using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BladeFury : MonoBehaviour, ISpell
{
    private readonly int range = 2;
    private readonly List<int> damage = new List<int>() { 50, 100, 150 };
    private readonly int bonusMagicResistance = 100;
    private readonly int duration = 5;
    private readonly int cooldown = 12;

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

        hero.GainMagicalResistance(bonusMagicResistance);
        StartCoroutine(StartAttack());
        StartCoroutine(Deactivate());
        StartCoroutine(Cooldown());
    }

    public IEnumerator StartAttack()
    {
        List<Hero> enemies = Board.instance.GetHeroesInRange(hero.CurrentCell, range).Where(h => h.tag != hero.tag).ToList();

        foreach (Hero enemy in enemies)
            hero.MagicalAttack(enemy, damage[hero.Upgrades]);

        yield return new WaitForSeconds(GameManager.instance.attackTime);

        if (isAttack && gameObject.activeSelf)
            StartCoroutine(StartAttack());
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(duration);
        isAttack = false;
        if (isCooldown)
            hero.GainMagicalResistance(-bonusMagicResistance);
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }

    public void ResetAbility()
    {
        isAttack = false;
        isCooldown = false;
    }

    public void PassiveSpell() { }
}
