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
        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = Random.Range(i, count);
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
    }
}