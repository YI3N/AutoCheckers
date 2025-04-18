using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marksmanship : MonoBehaviour, ISpell
{
    private List<float> damage = new List<float>() { 0.4f, 0.6f, 0.8f };

    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    void Start()
    {
        hero.GainDamage(damage[hero.Upgrades]);
    }

    public bool CanCast()
    {
        return false;
    }

    public void CastSpell() { }

    public void PassiveSpell() { }

    public void ResetAbility() { }
}
