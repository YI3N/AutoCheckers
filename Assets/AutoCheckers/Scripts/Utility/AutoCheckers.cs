using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoCheckers
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Mythic,
        Legendary
    };
    public enum HeroClass
    {
        Assassin,
        DemonHunter,
        Druid,
        Knight,
        Hunter,
        Priest,
        Mage,
        Mech,
        Shaman,
        Warlock,
        Warrior
    };
    public enum Race
    {
        Beast,
        Demon,
        Dragon,
        Dwarf,
        Elemental,
        Elf,
        Goblin,
        Satyr,
        Human,
        Ogre,
        Orc,
        Naga,
        Troll,
        Undead,
        God
    };

    public enum GameTag
    {
        Human,
        AI,
        Board,
        HumanBench,
        AIBench,
        Draw,
        Sell,
        Physical,
        Magical,
        Pure
    }

    public static class IExtensions
    {
        private static readonly Dictionary<Enum, (int, int)> abilityParameters = new Dictionary<Enum, (int, int)> {
            { Race.Beast, (BeastAbility.lvlThreshold, BeastAbility.maxLvl) },
            { Race.Dwarf, (DwarfAbility.lvlThreshold, DwarfAbility.maxLvl) },
            { Race.Goblin, (GoblinAbility.lvlThreshold, GoblinAbility.maxLvl) },
            { Race.Human, (HumanAbility.lvlThreshold, HumanAbility.maxLvl) },
            { Race.Naga, (NagaAbility.lvlThreshold, NagaAbility.maxLvl) },
            { Race.Orc, (OrcAbilitiy.lvlThreshold, OrcAbilitiy.maxLvl) },
            { Race.Troll, (TrollAbility.lvlThreshold, TrollAbility.maxLvl) },
            { Race.Undead, (UndeadAbility.lvlThreshold, UndeadAbility.maxLvl) },
            { HeroClass.Hunter, (HunterAbility.lvlThreshold, HunterAbility.maxLvl) },
            { HeroClass.Knight, (KnightAbility.lvlThreshold, KnightAbility.maxLvl) },
            { HeroClass.Mage, (MageAbility.lvlThreshold, MageAbility.maxLvl) },
            { HeroClass.Mech, (MechAbility.lvlThreshold, MechAbility.maxLvl) },
            { HeroClass.Warlock, (WarlockAbility.lvlThreshold, WarlockAbility.maxLvl) },
            { HeroClass.Warrior, (WarriorAbility.lvlThreshold, WarriorAbility.maxLvl) },  
        };

        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }

        public static void AddValue<TKey>(this Dictionary<TKey, int> dict, TKey key, int value) where TKey : System.Enum
        {
            if (dict.ContainsKey(key))
                dict[key] += value;
            else
                dict.Add(key, value);
        }

        public static (int, int) GetAbilityParameters(Enum ability)
        {
            if (abilityParameters.TryGetValue(ability, out var parameters))
                return parameters;
            else
                throw new ArgumentException("Unsupported ability type: " + ability);
        }

        public static bool AreEqual2DArrays<T>(T[,] a, T[,] b)
        {
            if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
                return false;

            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (!EqualityComparer<T>.Default.Equals(a[i, j], b[i, j]))
                        return false;
                }
            }

            return true;
        }
    }
}