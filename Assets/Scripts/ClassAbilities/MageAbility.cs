using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MageAbility : MonoBehaviour, IAbility
{
    public static bool DoneOnce = false;

    [SerializeField]
    private int lvlThreshold = 3;
    [SerializeField]
    private List<int> magicalResistanceDebuff = new List<int>() { -35, -90, -150 };

    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (!DoneOnce)
        {
            DoneOnce = true;
            foreach (GameObject enemy in hero.Opponent.HeroesOnBoard)
            {
                enemy.GetComponent<Hero>().GainMagicalResistance(magicalResistanceDebuff[heroes/lvlThreshold]);
            }
        }
    }

    public void DeactivateAbility(int heroes)
    {
        if (DoneOnce)
        {
            DoneOnce = false;
            foreach (GameObject enemy in hero.Opponent.HeroesOnBoard)
            {
                enemy.GetComponent<Hero>().GainMagicalResistance(-magicalResistanceDebuff[heroes / lvlThreshold]);
            }
        }
    }
}
