using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArcaneAura : MonoBehaviour, ISpell
{
    private readonly int range = 3;
    private readonly List<int> manaRegeneration = new List<int>() { 8, 14, 20 };
    private readonly int cooldown = 2;

    private bool isCooldown = false;
    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public bool CanCast()
    {
        return false;
    }

    public void PassiveSpell()
    {
        if (!isCooldown)
        {
            isCooldown = true;

            List<Hero> heroes = Board.instance.GetHeroesInRange(hero.CurrentCell, range).Where(h => h.tag == hero.tag).ToList();
            foreach (Hero ally in heroes)
            {
                ally.GainMana(manaRegeneration[hero.Upgrades] * 5);
            }

            StartCoroutine(Cooldown());
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
    }

    public void CastSpell() {}
}
