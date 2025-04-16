using AutoCheckers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Management
{
    private Player owner;
    private Player opponent;

    private Analytics analytics;
    private Tactics tactics;

    private bool shouldTryToWin;
    public int FreeMoney { get; private set; }

    public Management(Player player)
    {
        owner = player;
        opponent = owner.Tag == GameTag.Human ? GameManager.instance.AI : GameManager.instance.Human;

        analytics = new Analytics(owner);
        tactics = new Tactics(owner);
    }

    public void PredictIncome()
    {
        float guarantyIncome = Mathf.Clamp(owner.Money / 10, 0, 5);
    }

    public void PredictBattleOutcome()
    {
        analytics.SetDangerPlayers();

        float selfDanger = analytics.DangerPlayers[owner.Tag.ToString()];
        float opponentDanger = analytics.DangerPlayers[opponent.Tag.ToString()];
        float mad = analytics.CalculateMAD();

        Debug.Log($"[{owner.Tag}] Danger: {selfDanger} | Opponent: {opponentDanger} | MAD: {mad}");

        if (opponentDanger > selfDanger + mad || selfDanger < opponentDanger)
        {
            Debug.Log($"[{owner.Tag}] Предсказание: проигрыш");
            ActOnLoosePrediction();
        }
        else
        {
            Debug.Log($"[{owner.Tag}] Предсказание: победа");
            ActOnWinPrediction();
        }
    }

    private void ActOnWinPrediction()
    {
        shouldTryToWin = false;
        FreeMoney = owner.Money >= 50 ? owner.Money - 50 : owner.Money > 10 ? owner.Money % 10 + 1 : owner.Money % 10;

        Debug.Log($"[{owner.Tag}] Стратегия на победу. FreeMoney: {FreeMoney}");

        ExecuteRoundStrategy();
    }

    private void ActOnLoosePrediction()
    {
        float possibleDamage = opponent.CalculateDamage() / 2f;
        float damageRation = possibleDamage / owner.CurrentHealth;

        Debug.Log($"[{owner.Tag}] Потенциальный урон: {possibleDamage}, доля урона: {damageRation}");

        shouldTryToWin = false;

        if (owner.CurrentHealth - possibleDamage <= 0)
        {
            Debug.Log($"[{owner.Tag}] Срочная защита. Все деньги на игру.");
            FreeMoney = owner.Money;
            shouldTryToWin = true;
        }
        else if (damageRation >= 0.20)
        {
            FreeMoney = owner.Money >= 50 ? owner.Money - 50 : owner.Money % 20;
            shouldTryToWin = true;
            Debug.Log($"[{owner.Tag}] Высокий урон. Агрессивная защита. FreeMoney: {FreeMoney}");
        }
        else
        {
            int possibleWinBonus = Mathf.Clamp((owner.WinStreak + 1) / 3, 0, 3) + 1;

            FreeMoney = owner.Money >= 50 ? owner.Money - 50 : owner.Money % 10;
            shouldTryToWin = FreeMoney >= possibleWinBonus;
            Debug.Log($"[{owner.Tag}] Умеренный риск. FreeMoney: {FreeMoney}, TryToWin: {shouldTryToWin}");
        }

        ExecuteRoundStrategy();
    }

    private void ExecuteRoundStrategy()
    {
        TryBuyHeroes();

        TryBuyLevel();

        if (TryReroll())
            ExecuteRoundStrategy();
        else
            TryLockShop();

        tactics.SetHeroes(analytics.HeroBuyPriorities);
        analytics.SetBattlePriorities();
        tactics.CreateTactic(analytics.HeroBattlePriorities);
    }

    private void TryBuyHeroes()
    {
        analytics.SetBuyPriorities();

        List<GameObject> shop = Shop.instance.GetCurrentCardShop(owner.Tag);
        shop = shop.OrderByDescending(item =>
        {
            HeroCard heroCard = item.GetComponent<HeroCard>();
            float heroWeight = analytics.HeroBuyPriorities[heroCard.HeroPrefab.name];
            return heroWeight;
        }).ToList();

        foreach (GameObject item in shop)
        {
            HeroCard heroCard = item.GetComponent<HeroCard>();
            Hero hero = heroCard.HeroPrefab.GetComponent<Hero>();
            float heroWeight = analytics.HeroBuyPriorities[heroCard.HeroPrefab.name];

            if (heroWeight > analytics.WeakestWeight)
            {
                if (analytics.FreeSpace > 0 && FreeMoney >= hero.Cost)
                {
                    BuyHero(heroCard, -1);
                }
                else
                {
                    List<GameObject> heroes = owner.HeroesOnBoard.Concat(owner.HeroesOnBench).ToList();
                    int count = 1;

                    foreach (GameObject piece in heroes)
                    {
                        if (hero.ID == piece.GetComponent<Hero>().ID && hero.Upgrades == piece.GetComponent<Hero>().Upgrades)
                        {
                            count++;
                            if (count >= hero.CombineThreshold)
                                break;
                        }
                    }

                    if (count >= hero.CombineThreshold)
                    {
                        BuyHero(heroCard, 2);
                    }
                    else if (FreeMoney < hero.Cost)
                    {
                        List<Hero> benchHeroes = owner.HeroesOnBench
                            .Select(h => h.GetComponent<Hero>())
                            .OrderBy(h => h.Level)
                            .ToList();

                        int totalCost = benchHeroes.Sum(h => h.Level);

                        if (totalCost >= hero.Cost)
                            foreach (Hero sellHero in benchHeroes)
                            {
                                if (analytics.HeroBuyPriorities[sellHero.name] > heroWeight)
                                    continue;

                                float sellHeroDifference = analytics.CountDuplicateFactorLoss(sellHero);
                                float heroDifference = analytics.CountDuplicateFactorGain(hero);

                                if (sellHeroDifference <= heroDifference)
                                {
                                    SellHero(sellHero);
                                }

                                if (FreeMoney >= hero.Cost)
                                {
                                    BuyHero(heroCard, -1);
                                    break;
                                }
                            }
                    }
                    else if (analytics.FreeSpace <= 0)
                    {
                        List<Hero> weakHeroes = analytics.BoughtWeakHeroes.OrderBy(h => h.Level).ToList();
                        Hero weakHero = weakHeroes[0];

                        float weakHeroDifference = analytics.CountDuplicateFactorLoss(weakHero);
                        float heroDifference = analytics.CountDuplicateFactorGain(hero);

                        if (weakHeroDifference <= heroDifference)
                        {
                            SellHero(weakHero);
                        }

                        if (FreeMoney >= hero.Cost)
                        {
                            BuyHero(heroCard, -1);
                            break;
                        }
                    }
                }
            }
            else if (heroWeight == analytics.WeakestWeight && analytics.FreeSpace > 0 && FreeMoney >= hero.Cost)
            {
                BuyHero(heroCard, -1);
            }
            else if (analytics.UniqueHeroes <= owner.Level && FreeMoney >= hero.Cost)
            {
                BuyHero(heroCard, -1);
            }
        }
    }

    private void TryBuyLevel()
    {
        if (analytics.UniqueHeroes > owner.Level || owner.Level < opponent.Level)
        {
            if (FreeMoney >= Shop.instance.EXPCost)
            {
                while (FreeMoney >= Shop.instance.EXPCost)
                {
                    FreeMoney -= Shop.instance.EXPCost;
                    Shop.instance.BuyEXP(owner.Tag);
                    Debug.Log($"[{owner.Tag}] Покупка EXP напрямую. Остаток: {FreeMoney}");

                    if (analytics.UniqueHeroes <= owner.Level || owner.Level >= opponent.Level)
                        break;
                }
            }
            else
            {
                int neededGold = Shop.instance.EXPCost - FreeMoney;

                List<Hero> potentialSellers = analytics.BoughtWeakHeroes.Where(h =>
                    h.CurrentCell.tag != nameof(GameTag.Board) && owner.HeroesOnBoard.Contains(h.gameObject))
                    .OrderBy(h => h.Level).ToList();

                int accumulatedGold = 0;
                List<Hero> heroesToSell = new();

                foreach (Hero sellHero in potentialSellers)
                {
                    accumulatedGold += sellHero.Level;
                    heroesToSell.Add(sellHero);

                    if (accumulatedGold >= neededGold)
                        break;
                }

                if (accumulatedGold >= neededGold)
                {
                    foreach (Hero heroToSell in heroesToSell)
                    {
                        SellHero(heroToSell);
                    }

                    FreeMoney -= Shop.instance.EXPCost;
                    Shop.instance.BuyEXP(owner.Tag);
                    Debug.Log($"[{owner.Tag}] Покупка EXP через продажу слабых героев. Остаток: {FreeMoney}");
                }
            }
        }
    }

    private bool TryReroll()
    {
        if (FreeMoney > Shop.instance.RerollCost)
        {
            FreeMoney -= Shop.instance.RerollCost;
            Shop.instance.RerollShop(owner.Tag, true);

            Debug.Log($"[{owner.Tag}] Реролл магазина. Остаток: {FreeMoney}");

            return true;
        }

        return false;
    }

    private void TryLockShop()
    {
        foreach (GameObject item in Shop.instance.GetCurrentHeroShop(owner.Tag))
        {
            Hero hero = item.GetComponent<Hero>();
            if (analytics.CountHeroDuplicates(hero) % hero.CombineThreshold == hero.CombineThreshold - 1)
            {
                Debug.Log($"[{owner.Tag}] Магазин зафиксирован из-за дубликата: {hero.name}");
                Shop.instance.LockShop(owner.Tag);
                break;
            }
        }
    }

    private void BuyHero(HeroCard heroCard, int spaceDifference)
    {
        Hero hero = heroCard.HeroPrefab.GetComponent<Hero>();

        analytics.SetFreeSpace(spaceDifference);
        FreeMoney -= hero.Cost;

        Debug.Log($"[{owner.Tag}] Куплен герой: {hero.name} за {hero.Cost} золота. Остаток: {FreeMoney}");

        heroCard.PurchaceHero(owner.Tag);
        analytics.SetBuyPriorities();
    }

    private void SellHero(Hero hero)
    {
        analytics.SetFreeSpace(1);
        FreeMoney += hero.Level;

        Debug.Log($"[{owner.Tag}] Продан герой: {hero.name} за {hero.Level} золота. Итого: {FreeMoney}");

        Shop.instance.SellHero(hero, owner);
        analytics.SetBuyPriorities();
    }
}
