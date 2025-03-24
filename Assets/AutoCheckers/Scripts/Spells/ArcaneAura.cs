using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcaneAura : MonoBehaviour, ISpell
{
    private int range = 3;
    private List<int> manaRegeneration = new List<int>() { 8, 14, 20 };
    private int cooldown = 2;

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

            List<Hero> heroes = Board.instance.GetHeroesInRange(hero.CurrentCell, range);
            foreach (Hero ally in heroes)
            {
                if (ally.tag == tag)
                    ally.GainMana(manaRegeneration[hero.Upgrades]);
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
