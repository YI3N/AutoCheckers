using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AutoCheckers;
using System;
using System.Diagnostics;

public class AbilityIcon : MonoBehaviour
{
    [Header("Раса")]
    [SerializeField]
    private Texture2D orcIcon;
    [SerializeField]
    private Texture2D humanIcon;

    [Space(10)]
    [Header("Класс")]
    [SerializeField]
    private Texture2D warriorIcon;
    [SerializeField]
    private Texture2D mageIcon;

    [Space(10)]
    [Header("UI")]
    [SerializeField]
    private RawImage abilityIcon;
    [SerializeField]
    private TMP_Text abilityText;

    public void SetAbility(Enum abilityType, string text)
    {
        abilityText.text = text;
        switch (abilityType)
        {
            case Race race:
                abilityIcon.texture = race switch
                {
                    Race.Human => humanIcon,
                    Race.Orc => orcIcon,
                    Race.Beast => null,
                    Race.Demon => null,
                    Race.Dragon => null,
                    Race.Dwarf => null,
                    Race.Elemental => null,
                    Race.Elf => null,
                    Race.Goblin => null,
                    Race.Satyr => null,
                    Race.Ogre => null,
                    Race.Naga => null,
                    Race.Troll => null,
                    Race.Undead => null,
                    Race.God => null,
                    _ => null
                };
                break;
            case HeroClass heroClass:
                abilityIcon.texture = heroClass switch
                {
                    HeroClass.Warrior => warriorIcon,
                    HeroClass.Mage => mageIcon,
                    HeroClass.Assassin => null,
                    HeroClass.DemonHunter => null,
                    HeroClass.Druid => null,
                    HeroClass.Knight => null,
                    HeroClass.Hunter => null,
                    HeroClass.Priest => null,
                    HeroClass.Mech => null,
                    HeroClass.Shaman => null,
                    HeroClass.Warlock => null,
                    _ => null
                };
                break;
        }
    }
}
