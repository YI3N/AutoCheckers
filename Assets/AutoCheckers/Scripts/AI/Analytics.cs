using AutoCheckers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Analytics;

public class Analytics
{
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

    public PlayerState State { get; private set; }
    public Dictionary<string, float> HeroBuyPriorities { get; private set; } = new Dictionary<string, float>();
    public Dictionary<string, float> HeroBattlePriorities { get; private set; } = new Dictionary<string, float>();
    public Dictionary<string, float> DangerPlayers { get; private set; } = new Dictionary<string, float>();

    public Queue<Action> ManagementActions { get; private set; } = new Queue<Action>();
    public Queue<Action> TacticsActions { get; private set; } = new Queue<Action>();

    public Tactics Tactics { get; private set; }
    private Management management;

    private Player owner;
    private Player opponent;

    //settings
    private readonly float managementDelay = 1f;
    private readonly float tacticDelay = .75f;
    private readonly float reactionDelay = 3f;

    private float actionTimer = 2f;
    private float reactionTimer = 3f;

    public Analytics(Player player)
    {
        State = new PlayerState(player);
        management = new Management(this);
        Tactics = new Tactics(this);

        owner = State.Owner;
        opponent = State.Opponent;
    }

    public void HandleActions()
    {
        actionTimer -= Time.deltaTime;

        if (ManagementActions.Count > 0 && actionTimer <= 0f)
        {
            var action = ManagementActions.Dequeue();
            action.Invoke();
            actionTimer = managementDelay;

            if (ManagementActions.Count <= 0)
            {
                CreateTactics();
            }
        }

        if (ManagementActions.Count <= 0)
        {
            Tactics.SetDangerBoard();
        }

        if (Tactics.OpponentChanged)
        {
            reactionTimer = reactionDelay;
            Tactics.ReactToChanges();
        }

        if (reactionTimer > 0f)
        {
            reactionTimer -= Time.deltaTime;
            if (reactionTimer <= 0f)
            {
                CreateTactics();
            }
            return;
        }

        if (TacticsActions.Count > 0 && actionTimer <= 0f)
        {
            var action = TacticsActions.Dequeue();
            action.Invoke();
            actionTimer = tacticDelay;
        }
    }

    public void StartThinking()
    {
        ManagementActions.Clear();
        State.ResetState();
        management.PredictBattleOutcome();
    }

    public void CreateTactics()
    {
        TacticsActions.Clear();
        SetBattlePriorities();
        Tactics.CreateTactic(HeroBattlePriorities);
    }

    public void SetDangerPlayers()
    {
        DangerPlayers.Clear();

        float ownerDanger = owner.Wins / (float)(GameManager.instance.Round); // -1?
        float opponentDanger = opponent.Wins / (float)(GameManager.instance.Round); // -1?

        Debug.Log($"[Analytics:{owner.Tag}] Опасность игроков - Свой: {ownerDanger:F2}, Оппонент: {opponentDanger:F2}");

        DangerPlayers.Add(owner.Tag.ToString(), ownerDanger);
        DangerPlayers.Add(opponent.Tag.ToString(), opponentDanger);
    }

    public float CalculateMAD()
    {
        float[] values = DangerPlayers.Values.ToArray();
        float mean = values.Average();
        float mad = values.Select(x => Mathf.Abs(x - mean)).Average();

        Debug.Log($"[Analytics:{owner.Tag}] MAD рассчитано: {mad:F3}");

        return mad;
    }

    public void SetBattlePriorities()
    {
        HeroBattlePriorities.Clear();
        List<Hero> heroes = owner.HeroesOnBoard.Concat(owner.HeroesOnBench).Select(piece => piece.GetComponent<Hero>()).ToList();

        foreach (Hero hero in heroes)
            if (!HeroBattlePriorities.ContainsKey(hero.name))
            {
                float utility = (owner.AttackStatistics == 0 || owner.DefenceStatistics == 0 || hero.AttackStatistics + hero.DefenceStatistics == 0) ? (2f / (float)owner.Level) : hero.AttackStatistics / (float)owner.AttackStatistics + hero.DefenceStatistics / (float)owner.DefenceStatistics;
                float value = hero.Level * utility;
                HeroBattlePriorities.Add(hero.name, value);

                Debug.Log($"[Analytics:{owner.Tag}] Приоритет боя для {hero.name}: {value:F2}");
            }

        HeroBattlePriorities = HeroBattlePriorities.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public void SetBuyPriorities()
    {
        HeroBuyPriorities.Clear();

        State.SetUniqueHeroes(0);
        int freeBoardSpace = owner.Level - State.HeroesOnBoard.Count;
        int freeBenchSpace = 8 - State.HeroesOnBench.Count;
        State.SetFreeSpace(freeBoardSpace + freeBenchSpace);

        Debug.Log($"[Analytics:{owner.Tag}] Свободное место: {State.FreeSpace}, На поле: {freeBoardSpace}, На скамейке: {freeBenchSpace}");

        float minWeight = Mathf.Infinity;
        foreach (Hero hero in State.AllHeroes)
            if (!HeroBuyPriorities.ContainsKey(hero.name))
            {
                float heroWeight = CountHeroWeight(hero);
                HeroBuyPriorities.Add(hero.name, heroWeight);

                Debug.Log($"[Analytics:{owner.Tag}] Вес героя {hero.name}: {heroWeight:F2}");

                if (heroWeight < minWeight)
                {
                    State.ClearWeakHeroes();
                    State.AddWeakHero(hero);
                    minWeight = heroWeight;
                }
                else if (heroWeight == minWeight)
                {
                    State.AddWeakHero(hero);
                }

                State.ChangeUniqueHeroes(1);
            }

        State.SetWeakestWeight(minWeight == Mathf.Infinity ? 0 : minWeight);
        Debug.Log($"[Analytics:{owner.Tag}] Наименьший вес: {State.WeakestWeight:F2}, Уникальные герои: {State.UniqueHeroes}");

        int amount = State.UniqueHeroes + freeBoardSpace + freeBenchSpace + 1;
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
            Debug.Log($"[Analytics:{owner.Tag}] Добавлен герой из магазина {hero.Key} с весом {hero.Value:F2}");
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

        Debug.Log($"[Analytics:{owner.Tag}] Разбор веса для {hero.name} → Шанс: {spawnChance:F3}, Дубликаты: {duplicateFactor:F3}, Раса: {raceFactor:F3}, Класс: {classFactor:F2} = {result:F2}");

        return result;
    }

    private float CountHeroSpawnChance(Hero hero)
    {
        float rarityChance = Shop.instance.GetCurrentRaritySpawnChance(owner.Level, hero.Rarity);

        List<GameObject> heroes = Shop.instance.GetHeroRarityList(hero.Rarity).Concat(Shop.instance.GetCurrentCardShop(opponent.Tag)).ToList();
        int heroDuplicates = 0;

        foreach (GameObject card in heroes)
        {
            Hero cardHero = card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>();
            if (cardHero.ID == hero.ID)
                heroDuplicates += 1;
        }

        List<GameObject> currentShop = Shop.instance.GetCurrentHeroShop(owner.Tag);

        int heroDupInShop = 0;
        foreach (GameObject piece in currentShop)
        {
            Hero shopHero = piece.GetComponent<Hero>();
            if (shopHero.ID == hero.ID)
                heroDupInShop += 1;
        }

        int heroCount = heroes.Count();

        foreach (GameObject piece in State.HeroesToBuy)
        {
            Hero buyHero = piece.GetComponent<Hero>();
            if (buyHero.ID == hero.ID)
            {
                heroDupInShop -= 1;
                heroDuplicates -= 1;
                heroCount -= 1;
            }
        }

        foreach (GameObject piece in State.HeroesToSell)
        {
            Hero sellHero = piece.GetComponent<Hero>();
            if (sellHero.ID == hero.ID)
            {
                heroDuplicates += 1;
                heroCount += 1;
            }
        }

        float chance = heroCount == 0 ? 0 : rarityChance * heroDupInShop * heroDuplicates / heroCount;

        return chance;
    }

    public int CountHeroDuplicates(Hero targetHero)
    {
        int heroDuplicates = 0;
        foreach (Hero hero in State.AllHeroes)
        {
            if (hero.ID == targetHero.ID)
                heroDuplicates += (int)Mathf.Pow(hero.CombineThreshold, hero.Upgrades);
        };

        return heroDuplicates;
    }

    private float CountHeroDuplicateFactor(Hero hero)
    {
        int heroDuplicates = CountHeroDuplicates(hero);
        if (heroDuplicates > 9)
            heroDuplicates = 9;
        float factor = duplicateFactors[heroDuplicates];

        return factor;
    }

    private float CountHeroRaceFactor(Hero hero)
    {
        int heroes = 0;
        if (State.RaceHeroes.TryGetValue(hero.Race, out int amount))
            heroes = amount;

        if (State.BenchRaces.TryGetValue(hero.Race, out amount))
            heroes += amount;

        var key = IExtensions.GetAbilityParameters(hero.Race);

        if (abilityFactors.TryGetValue(key, out var factors))
        {
            if (heroes > factors.Length - 1)
                heroes = factors.Length - 1;
            return factors[heroes];
        }   
        else
            throw new ArgumentException("Unsupported ability");
    }

    private float CountHeroClassFactor(Hero hero)
    {
        int heroes = 0;
        if (State.ClassHeroes.TryGetValue(hero.HeroClass, out int amount))
            heroes = amount;

        if (State.BenchClasses.TryGetValue(hero.HeroClass, out amount))
            heroes += amount;

        var key = IExtensions.GetAbilityParameters(hero.HeroClass);

        if (abilityFactors.TryGetValue(key, out var factors))
        {
            if (heroes > factors.Length - 1)
                heroes = factors.Length - 1;
            return factors[heroes];
        }
        else
            throw new ArgumentException("Unsupported ability");
    }
    public float CountDuplicateFactorLoss(Hero hero)
    {
        int heroDuplicates = CountHeroDuplicates(hero);

        float currentFactor = duplicateFactors[heroDuplicates];
        float previousFactor = heroDuplicates == hero.CombineThreshold ? duplicateFactors[heroDuplicates - 3] : duplicateFactors[heroDuplicates - 1];

        float result = currentFactor - previousFactor;

        Debug.Log($"[Analytics:{owner.Tag}] {hero.name} Потеря фактора дубликатов: {result:F2} (Текущий: {currentFactor:F2}, Предыдущий: {previousFactor:F2})");

        return currentFactor - previousFactor;
    }

    public float CountDuplicateFactorGain(Hero hero)
    {
        int heroDuplicates = CountHeroDuplicates(hero);

        float currentFactor = duplicateFactors[heroDuplicates];
        float futureFactor = duplicateFactors[heroDuplicates + 1];

        float result = futureFactor - currentFactor;

        Debug.Log($"[Analytics:{owner.Tag}] {hero.name} Прирост фактора дубликатов: {result:F2} (Следующий: {futureFactor:F2}, Текущий: {currentFactor:F2})");

        return result;
    }

    public void AddManagementAction(Action action)
    {
        ManagementActions.Enqueue(action);
    }

    public void AddTacticsAction(Action action)
    {
        TacticsActions.Enqueue(action);
    }
}
