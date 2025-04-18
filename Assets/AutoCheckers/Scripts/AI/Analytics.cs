using AutoCheckers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor.Playables;
using UnityEngine;

public class Analytics
{
    private Player owner;
    private Player opponent;

    private readonly float[] duplicateFactors = new float[] { 0, 0.125f, 0.375f, 0.25f, 0.5f, 0.75f, 0.625f, 0.875f, 1f, 4f };
    private static readonly float[] ability3_6Factors = new float[] { 0, 0.17f, 0.5f, 0.33f, 0.67f, 1f, 0.83f };
    private static readonly float[] ability2_4Factors = new float[] { 0, 0.5f, 0.25f, 1f, 0.75f };
    private static readonly float[] ability3_3Factors = new float[] { 0, 0.33f, 1f, 0.67f };
    private static readonly float[] ability2_2Factors = new float[] { 0, 1f, 0.5f };

    private readonly Dictionary<(int, int), float[]> abilityFactors = new Dictionary<(int, int), float[]> {
        {(3, 6), ability3_6Factors},
        {(3, 3), ability3_3Factors},
        {(2, 4), ability2_4Factors},
        {(2, 2), ability2_2Factors}
    };

    public int FreeSpace { get; private set; }
    public int UniqueHeroes { get; private set; }
    public float WeakestWeight { get; private set; }
    public List<Hero> BoughtWeakHeroes { get; private set; } = new List<Hero>();
    public Dictionary<string, float> HeroBuyPriorities { get; private set; } = new Dictionary<string, float>();
    public Dictionary<string, float> HeroBattlePriorities { get; private set; } = new Dictionary<string, float>();
    public Dictionary<string, float> DangerPlayers { get; private set; } = new Dictionary<string, float>();

    public Analytics(Player player)
    {
        owner = player;
        opponent = owner.Tag == GameTag.Human ? GameManager.instance.AI : GameManager.instance.Human;
    }

    public void SetFreeSpace(int amount)
    {
        FreeSpace += amount;
        Debug.Log($"[Analytics:{owner.Tag}] FreeSpace changed by {amount}, now {FreeSpace}");
    }

    public void SetDangerPlayers()
    {
        DangerPlayers.Clear();

        // -1
        float ownerDanger = owner.Wins / (float)(GameManager.instance.Round);
        float opponentDanger = opponent.Wins / (float)(GameManager.instance.Round);

        Debug.Log($"[Analytics:{owner.Tag}] SetDangerPlayers - Owner: {ownerDanger:F2}, Opponent: {opponentDanger:F2}");

        DangerPlayers.Add(owner.Tag.ToString(), ownerDanger);
        DangerPlayers.Add(opponent.Tag.ToString(), opponentDanger);
    }

    public float CalculateMAD()
    {
        float[] values = DangerPlayers.Values.ToArray();
        float mean = values.Average();
        float mad = values.Select(x => Mathf.Abs(x - mean)).Average();

        Debug.Log($"[Analytics:{owner.Tag}] MAD calculated: {mad:F3}");

        return mad;
    }

    public void SetBattlePriorities()
    {
        HeroBattlePriorities.Clear();

        foreach (GameObject piece in owner.HeroesOnBoard)
        {
            Hero hero = piece.GetComponent<Hero>();
            if (!HeroBattlePriorities.ContainsKey(hero.name))
            {
                float utility = (owner.AttackStatistics == 0 || owner.DefenceStatistics == 0) ? 0 : hero.AttackStatistics / (float)owner.AttackStatistics + hero.DefenceStatistics / (float)owner.DefenceStatistics;
                float value = hero.Level * utility;
                HeroBattlePriorities.Add(hero.name, value);

                Debug.Log($"[Analytics:{owner.Tag}] Battle priority for {hero.name}: {value:F2}");
            }
        }

        HeroBattlePriorities = HeroBattlePriorities.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public void SetBuyPriorities()
    {
        HeroBuyPriorities.Clear();

        UniqueHeroes = 0;
        int freeBoardSpace = owner.Level - owner.HeroesOnBoard.Count;
        int freeBenchSpace = 8 - owner.HeroesOnBench.Count;
        FreeSpace = freeBoardSpace + freeBenchSpace;

        Debug.Log($"[Analytics:{owner.Tag}] FreeSpace: {FreeSpace}, Board: {freeBoardSpace}, Bench: {freeBenchSpace}");

        float minWeight = Mathf.Infinity;

        List<GameObject> boughtHeroes = owner.HeroesOnBoard.Concat(owner.HeroesOnBench).ToList();
        foreach (GameObject piece in boughtHeroes)
        {
            Hero hero = piece.GetComponent<Hero>();
            if (!HeroBuyPriorities.ContainsKey(hero.name))
            {
                float heroWeight = CountHeroWeight(hero);
                HeroBuyPriorities.Add(hero.name, heroWeight);

                Debug.Log($"[Analytics:{owner.Tag}] Hero {hero.name} weight: {heroWeight:F2}");

                if (heroWeight < minWeight)
                {
                    BoughtWeakHeroes.Clear();
                    BoughtWeakHeroes.Add(hero);
                    minWeight = heroWeight;
                }
                else if (heroWeight == minWeight)
                {
                    BoughtWeakHeroes.Add(hero);
                }

                UniqueHeroes++;
            }
        }

        WeakestWeight = minWeight == Mathf.Infinity ? 0 : minWeight;
        Debug.Log($"[Analytics:{owner.Tag}] Weakest weight: {WeakestWeight:F2}, UniqueHeroes: {UniqueHeroes}");

        int amount = UniqueHeroes + freeBoardSpace + freeBenchSpace + 1;
        int freeSpace = amount - HeroBuyPriorities.Count();

        Dictionary<string, float> shopPriorities = new Dictionary<string, float>();
        foreach (GameObject piece in Shop.instance.GetCurrentHeroShop(owner.Tag))
        {
            Hero hero = piece.GetComponent<Hero>();
            if (!shopPriorities.ContainsKey(hero.name) && !HeroBuyPriorities.ContainsKey(hero.name))
                shopPriorities.Add(hero.name, CountHeroWeight(hero));
        }

        shopPriorities = shopPriorities.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (KeyValuePair<string, float> hero in shopPriorities.Take(freeSpace))
        {
            HeroBuyPriorities.Add(hero.Key, hero.Value);
            Debug.Log($"[Analytics:{owner.Tag}] Added shop hero {hero.Key} with weight {hero.Value:F2}");
        }

        HeroBuyPriorities = HeroBuyPriorities.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private float CountHeroWeight(Hero hero)
    {
        float spawnChance = CountHeroSpawnChance(hero);
        float duplicateFactor = CountHeroDuplicateFactor(hero);
        float raceFactor = CountHeroRaceFactor(hero);
        float classFactor = CountHeroClassFactor(hero);

        float result = spawnChance + duplicateFactor + raceFactor + classFactor;

        Debug.Log($"[Analytics:{owner.Tag}] Weight breakdown for {hero.name} → Spawn: {spawnChance:F2}, Dup: {duplicateFactor:F2}, Race: {raceFactor:F2}, Class: {classFactor:F2} = {result:F2}");

        return result;
    }

    //Внедрить упрощенный подсчет спавна
    private float CountHeroSpawnChance(Hero hero)
    {
        float rarityChance = Shop.instance.GetCurrentRaritySpawnChance(owner.Level, hero.Rarity);

        List<GameObject> heroes = Shop.instance.GetHeroRarityList(hero.Rarity);
        int heroDuplicates = 0;

        foreach (GameObject card in heroes)
        {
            Hero cardHero = card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>();
            if (cardHero.ID == hero.ID)
                heroDuplicates += 1;
        }

        float chance = heroes.Count() == 0 ? 0 : rarityChance * heroDuplicates / heroes.Count();

        return chance;
    }

    public int CountHeroDuplicates(Hero hero)
    {
        int heroDuplicates = 0;
        List<GameObject> heroes = owner.HeroesOnBoard.Concat(owner.HeroesOnBench).ToList();
        foreach (GameObject piece in heroes)
        {
            Hero pieceHero = piece.GetComponent<Hero>();
            if (pieceHero.ID == hero.ID)
                heroDuplicates += (int)Mathf.Pow(pieceHero.CombineThreshold, pieceHero.Upgrades);
        };

        return heroDuplicates;
    }

    private float CountHeroDuplicateFactor(Hero hero)
    {
        int heroDuplicates = CountHeroDuplicates(hero);
        float factor = duplicateFactors[heroDuplicates];

        return factor;
    }

    private float CountHeroRaceFactor(Hero hero)
    {
        int heroes = 0;
        if (owner.RaceHeroes.TryGetValue(hero.Race, out int amount))
            heroes = amount;

        if (owner.BenchRaces.TryGetValue(hero.Race, out amount))
            heroes += amount;

        var key = IExtensions.GetAbilityParameters(hero.Race);

        if (abilityFactors.TryGetValue(key, out var factors))
            return factors[heroes];
        else
            throw new ArgumentException("Unsupported ability");
    }

    private float CountHeroClassFactor(Hero hero)
    {
        int heroes = 0;
        if (owner.ClassHeroes.TryGetValue(hero.HeroClass, out int amount))
            heroes = amount;

        if (owner.BenchClasses.TryGetValue(hero.HeroClass, out amount))
            heroes += amount;

        var key = IExtensions.GetAbilityParameters(hero.HeroClass);

        if (abilityFactors.TryGetValue(key, out var factors))
            return factors[heroes];
        else
            throw new ArgumentException("Unsupported ability");
    }
    public float CountDuplicateFactorLoss(Hero hero)
    {
        int heroDuplicates = CountHeroDuplicates(hero);

        float currentFactor = duplicateFactors[heroDuplicates];
        float previousFactor = heroDuplicates == hero.CombineThreshold ? duplicateFactors[heroDuplicates - 3] : duplicateFactors[heroDuplicates - 1];

        float result = currentFactor - previousFactor;

        Debug.Log($"[Analytics:{owner.Tag}] {hero.name} DupFactorGainPrev = {result:F2} (Current: {currentFactor:F2}, Prev: {previousFactor:F2})");

        return currentFactor - previousFactor;
    }

    public float CountDuplicateFactorGain(Hero hero)
    {
        int heroDuplicates = CountHeroDuplicates(hero);

        float currentFactor = duplicateFactors[heroDuplicates];
        float futureFactor = duplicateFactors[heroDuplicates + 1];

        float result = futureFactor - currentFactor;

        Debug.Log($"[Analytics:{owner.Tag}] {hero.name} DupFactorGainNext = {result:F2} (Next: {futureFactor:F2}, Current: {currentFactor:F2})");


        return result;
    }
}
