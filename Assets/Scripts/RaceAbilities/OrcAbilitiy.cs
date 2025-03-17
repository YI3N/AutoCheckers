using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcAbilitiy : MonoBehaviour, IAbility
{
    [SerializeField]
    private int lvlThreshold = 2;
    [SerializeField]
    private List<int> hpBoost = new List<int>() { 100, 350, 350 };

    private Hero hero;
    private static bool DoneOnce = false;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (!DoneOnce)
        {
            DoneOnce = true;
            foreach (GameObject allyObj in hero.Owner.HeroesOnBoard)
            {
                Hero ally = allyObj.GetComponent<Hero>();
                if (heroes / lvlThreshold >= 3)
                    ally.GainMaxHealth(hpBoost[heroes / lvlThreshold] + (ally.Owner.MaxHealth - ally.Owner.CurrentHealth) * 8);
                else
                    ally.GainMaxHealth(hpBoost[heroes / lvlThreshold]);
            }
        }
    }

    public void DeactivateAbility(int heroes)
    {
        if (DoneOnce)
        {
            DoneOnce = false;
            foreach (GameObject allyObj in hero.Owner.HeroesOnBoard)
            {
                Hero ally = allyObj.GetComponent<Hero>();
                if (heroes / lvlThreshold >= 3)
                    ally.GainMaxHealth(-hpBoost[heroes / lvlThreshold] - (ally.Owner.MaxHealth - ally.Owner.CurrentHealth) * 8);
                else
                    ally.GainMaxHealth(-hpBoost[heroes / lvlThreshold]);
            }
        }
    }
}
