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
    private Texture2D beastIcon;
    [SerializeField]
    private Texture2D dwarfIcon;
    [SerializeField]
    private Texture2D goblinIcon;
    [SerializeField]
    private Texture2D humanIcon;
    [SerializeField]
    private Texture2D nagaIcon;
    [SerializeField]
    private Texture2D orcIcon;
    [SerializeField]
    private Texture2D trollIcon;
    [SerializeField]
    private Texture2D undeadIcon;

    [Space(10)]
    [Header("Класс")]
    [SerializeField]
    private Texture2D hunterIcon;
    [SerializeField]
    private Texture2D knightIcon;
    [SerializeField]
    private Texture2D mageIcon;
    [SerializeField]
    private Texture2D mechIcon;
    [SerializeField]
    private Texture2D warlockIcon;
    [SerializeField]
    private Texture2D warriorIcon;


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
                    Race.Beast => beastIcon,
                    Race.Demon => null,
                    Race.Dragon => null,
                    Race.Dwarf => dwarfIcon,
                    Race.Elemental => null,
                    Race.Elf => null,
                    Race.Goblin => goblinIcon,
                    Race.Satyr => null,
                    Race.Ogre => null,
                    Race.Naga => nagaIcon,
                    Race.Troll => trollIcon,
                    Race.Undead => undeadIcon,
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
                    HeroClass.Knight => knightIcon,
                    HeroClass.Hunter => hunterIcon,
                    HeroClass.Priest => null,
                    HeroClass.Mech => mechIcon,
                    HeroClass.Shaman => null,
                    HeroClass.Warlock => warlockIcon,
                    _ => null
                };
                break;
        }
    }
}
