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
    private static readonly float[] ability3_9Factors = new float[] { 0, 0.11f, 0.33f, 0.22f, 0.44f, 0.67f, 0.56f, 0.78f, 1f, 0.89f };
    private static readonly float[] ability2_4Factors = new float[] { 0, 0.5f, 0.25f, 1f, 0.75f };
    private static readonly float[] ability2_6Factors = new float[] { 0, 0.33f, 0.17f, 0.67f, 0.5f, 1f, 0.83f };

    private readonly Dictionary<(int, int), float[]> abilityFactors = new Dictionary<(int, int), float[]> {
        {(3, 6), ability3_6Factors},
        {(3, 9), ability3_9Factors},
        {(2, 4), ability2_4Factors},
        {(2, 6), ability2_6Factors}
    };

    public Dictionary<string, float> HeroBuyPriorities { get; private set; } = new Dictionary<string, float>();
    public Dictionary<string, float> HeroBattlePriorities { get; private set; } = new Dictionary<string, float>();
    public Dictionary<string, float> DangerPlayers { get; private set; } = new Dictionary<string, float>();

    public Analytics(Player player)
    {
        owner = player;
        opponent = owner.Tag == GameTag.Human ? GameManager.instance.AI : GameManager.instance.Human;
    }

    public void SetDangerPlayers()
    {
        DangerPlayers.Clear();

        float ownerDanger = owner.Wins / (GameManager.instance.Round - 1);
        float opponentDanger = opponent.Wins / (GameManager.instance.Round - 1);

        if (opponentDanger > ownerDanger)
        {
            DangerPlayers.Add(GameTag.AI.ToString(), opponentDanger);
            DangerPlayers.Add(GameTag.Human.ToString(), ownerDanger);
        }
        else
        {
            DangerPlayers.Add(GameTag.Human.ToString(), ownerDanger);
            DangerPlayers.Add(GameTag.AI.ToString(), opponentDanger);
        }
    }

    public void SetBattlePriorities()
    {
        HeroBattlePriorities.Clear();

        foreach (GameObject piece in owner.HeroesOnBoard)
        {
            Hero hero = piece.GetComponent<Hero>();
            if (!HeroBattlePriorities.ContainsKey(hero.name))
            {
                float utility = hero.AttackStatistics / (float)owner.AttackStatistics + hero.DefenceStatistics / (float)owner.DefenceStatistics;
                float value = hero.Level * utility;
                HeroBattlePriorities.Add(hero.name, value);
            }
        }

        HeroBattlePriorities = HeroBattlePriorities.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public void SetBuyPriorities()
    {
        HeroBuyPriorities.Clear();

        List<GameObject> heroSources = owner.HeroesOnBoard.Concat(owner.HeroesOnBench.Concat(Shop.instance.GetCurrentHeroShop(owner.Tag))).ToList();
        foreach (GameObject piece in heroSources)
        {
            Hero hero = piece.GetComponent<Hero>();
            if (!HeroBuyPriorities.ContainsKey(hero.name))
                HeroBuyPriorities.Add(hero.name, CountHeroWeight(hero));
        }

        HeroBuyPriorities = HeroBuyPriorities.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private float CountHeroWeight(Hero hero)
    {
        float spawnChance = CountHeroSpawnChance(hero);
        float duplicateFactor = CountHeroDuplicateFactor(hero);
        float raceFactor = CountHeroRaceFactor(hero);
        float classFactor = CountHeroClassFactor(hero);

        return spawnChance + duplicateFactor + raceFactor + classFactor;
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

    private float CountHeroDuplicateFactor(Hero hero)
    {
        int heroDuplicates = 0;
        List<GameObject> heroes = owner.HeroesOnBoard.Concat(owner.HeroesOnBench).ToList();
        foreach (GameObject piece in heroes)
        {
            Hero pieceHero = piece.GetComponent<Hero>();
            if (pieceHero.ID == hero.ID)
                heroDuplicates += (int)Mathf.Pow(hero.CombineThreshold, hero.Upgrades);
        }

        float factor = duplicateFactors[heroDuplicates];

        return factor;
    }

    private float CountHeroRaceFactor(Hero hero)
    {
        int heroes = 0;
        if (owner.RaceHeroes.TryGetValue(hero.Race, out var amount))
            heroes = amount;

            var key = IExtensions.GetAbilityParameters(hero.Race);

        if (abilityFactors.TryGetValue(key, out var factors))
            return factors[heroes];
        else
            throw new ArgumentException("Unsupported ability");
    }

    private float CountHeroClassFactor(Hero hero)
    {
        int heroes = 0;
        if (owner.ClassHeroes.TryGetValue(hero.HeroClass, out var amount))
            heroes = amount;

        var key = IExtensions.GetAbilityParameters(hero.HeroClass);

        if (abilityFactors.TryGetValue(key, out var factors))
            return factors[heroes];
        else
            throw new ArgumentException("Unsupported ability");
    }
}
