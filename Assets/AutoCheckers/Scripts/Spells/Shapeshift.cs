using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shapeshift : MonoBehaviour, ISpell
{
    [SerializeField]
    private Material WolfIcon;
    [SerializeField]
    private Material HumanIcon;

    private readonly List<float> healthMultiplier = new List<float>() { 1.2f, 1.3f, 1.4f };
    private readonly List<int> damage = new List<int>() { 50, 100, 150 };

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

        hero.transform.Find("HeroPlane").GetComponent<Renderer>().material = WolfIcon;
        hero.GainMaxHealth(Mathf.FloorToInt(hero.MaxHealth * healthMultiplier[hero.Upgrades]));
        hero.GainDamage(damage[hero.Upgrades]);
    }

    public void ResetAbility()
    {
        isCooldown = false;
        hero.transform.Find("HeroPlane").GetComponent<Renderer>().material = HumanIcon;
    }

    public void PassiveSpell() { }
}
