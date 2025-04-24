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

        Debug.Log($"[Management: {owner.Tag}] Danger: {selfDanger} | Opponent: {opponentDanger} | MAD: {mad}");

        if (opponentDanger >= selfDanger + mad || selfDanger <= opponentDanger)
        {
            Debug.Log($"[Management: {owner.Tag}] Предсказание: проигрыш");
            ActOnLoosePrediction();
        }
        else
        {
            Debug.Log($"[Management: {owner.Tag}] Предсказание: победа");
            ActOnWinPrediction();
        }
    }

    private void ActOnWinPrediction()
    {
        shouldTryToWin = false;
        FreeMoney = owner.Money >= 50 ? owner.Money - 50 : owner.Money % 11;

        Debug.Log($"[Management: {owner.Tag}] Стратегия на победу. FreeMoney: {FreeMoney}");

        ExecuteRoundStrategy();
    }

    private void ActOnLoosePrediction()
    {
        float possibleDamage = opponent.CalculateDamage() / 2f;
        float damageRation = possibleDamage / owner.CurrentHealth;

        Debug.Log($"[Management: {owner.Tag}] Потенциальный урон: {possibleDamage}, доля урона: {damageRation}");

        shouldTryToWin = false;

        if (owner.CurrentHealth - possibleDamage <= 0)
        {
            Debug.Log($"[Management: {owner.Tag}] Срочная защита. Все деньги на игру.");
            FreeMoney = owner.Money;
            shouldTryToWin = true;
        }
        else if (damageRation >= 0.20)
        {
            FreeMoney = owner.Money > 50 ? owner.Money - 50 : owner.Money % 21;
            shouldTryToWin = true;
            Debug.Log($"[Management: {owner.Tag}] Высокий урон. Агрессивная защита. FreeMoney: {FreeMoney}");
        }
        else
        {
            int possibleWinBonus = Mathf.Clamp((owner.WinStreak + 1) / 3, 0, 3) + 1;

            FreeMoney = owner.Money > 50 ? owner.Money - 50 : owner.Money % 11;
            shouldTryToWin = FreeMoney >= possibleWinBonus;
            Debug.Log($"[Management: {owner.Tag}] Умеренный риск. FreeMoney: {FreeMoney}, TryToWin: {shouldTryToWin}");
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

        analytics.SetBattlePriorities();
        tactics.CreateTactic(analytics.HeroBattlePriorities);
    }

    private void TryBuyHeroes()
    {
        analytics.SetBuyPriorities();

        List<GameObject> shop = Shop.instance.GetCurrentCardShop(owner.Tag);
        Debug.Log($"[Management: {owner.Tag}] Анализ магазина. Герои: {string.Join(", ", shop.Select(i => i.GetComponent<HeroCard>().HeroPrefab.name))}");

        shop = shop.OrderByDescending(item =>
        {
            HeroCard heroCard = item.GetComponent<HeroCard>();
            float heroWeight = 0;

            if (analytics.HeroBuyPriorities.ContainsKey(heroCard.HeroPrefab.name))
            {
                heroWeight = analytics.HeroBuyPriorities[heroCard.HeroPrefab.name];
            }

            return heroWeight;
        }).ToList();

        foreach (GameObject item in shop)
        {
            HeroCard heroCard = item.GetComponent<HeroCard>();
            Hero hero = heroCard.HeroPrefab.GetComponent<Hero>();

            float heroWeight = 0;

            if (analytics.HeroBuyPriorities.ContainsKey(heroCard.HeroPrefab.name))
            {
                heroWeight = analytics.HeroBuyPriorities[heroCard.HeroPrefab.name];
            }

            if (heroWeight > 4)
                continue;

            Debug.Log($"[Management: {owner.Tag}] Анализ героя: {hero.name} | Weight: {heroWeight} | Cost: {hero.Cost} | FreeMoney: {FreeMoney}");

            if (heroWeight > analytics.WeakestWeight)
            {
                Debug.Log($"[Management: {owner.Tag}] {hero.name} выше самого слабого героя ({analytics.WeakestWeight})");

                if (analytics.FreeSpace > 0 && FreeMoney >= hero.Cost)
                {
                    Debug.Log($"[Management: {owner.Tag}] Есть свободное место и достаточно золота. Покупаем.");
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

                    if (FreeMoney < hero.Cost)
                    {
                        List<Hero> benchHeroes = owner.HeroesOnBench
                            .Select(h => h.GetComponent<Hero>())
                            .OrderBy(h => h.Level)
                            .ToList();

                        int totalCost = benchHeroes.Sum(h => h.Level);

                        if (totalCost >= hero.Cost)
                        {
                            Debug.Log($"[Management: {owner.Tag}] Недостаточно золота. Пробуем продать героев с лавки.");
                            foreach (Hero sellHero in benchHeroes)
                            {
                                if (analytics.HeroBuyPriorities[sellHero.name] > heroWeight)
                                    continue;

                                float sellHeroDifference = analytics.CountDuplicateFactorLoss(sellHero);
                                float heroDifference = analytics.CountDuplicateFactorGain(hero);

                                Debug.Log($"[Management: {owner.Tag}] Кандидат на продажу: {sellHero.name} | Разница: {sellHeroDifference:F2} vs {heroDifference:F2}");

                                if (sellHeroDifference <= heroDifference)
                                {
                                    Debug.Log($"[Management: {owner.Tag}] Продаем {sellHero.name} ради {hero.name}.");
                                    SellHero(sellHero);
                                }

                                if (FreeMoney >= hero.Cost)
                                {
                                    BuyHero(heroCard, -1);
                                    break;
                                }
                            }
                        }
                    }
                    else if (count >= hero.CombineThreshold)
                    {
                        Debug.Log($"[Management: {owner.Tag}] Достигнут порог комбинации ({count} / {hero.CombineThreshold}). Покупаем.");
                        BuyHero(heroCard, 2);
                    }
                    else if (analytics.FreeSpace <= 0)
                    {
                        Debug.Log($"[Management: {owner.Tag}] Нет свободного места. Пробуем заменить слабого героя.");

                        List<Hero> weakHeroes = analytics.BoughtWeakHeroes.OrderBy(h => h.Level).ToList();
                        Hero weakHero = weakHeroes[0];

                        float weakHeroDifference = analytics.CountDuplicateFactorLoss(weakHero);
                        float heroDifference = analytics.CountDuplicateFactorGain(hero);

                        if (weakHeroDifference <= heroDifference)
                        {
                            Debug.Log($"[Management: {owner.Tag}] Продаем {weakHero.name} ради {hero.name}.");
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
                Debug.Log($"[Management: {owner.Tag}] {hero.name} равен по весу слабейшему ({analytics.WeakestWeight}), но есть место и золото. Покупаем.");
                BuyHero(heroCard, -1);
            }
            else if (analytics.UniqueHeroes <= owner.Level && FreeMoney >= hero.Cost)
            {
                Debug.Log($"[Management: {owner.Tag}] Число уникальных героев не больше уровня ({analytics.UniqueHeroes} <= {owner.Level}). Берем {hero.name} ради разнообразия.");
                BuyHero(heroCard, -1);
            }
            else
            {
                Debug.Log($"[Management: {owner.Tag}] Пропущен {hero.name}. Вес: {heroWeight}, Свободное место: {analytics.FreeSpace}, Золото: {FreeMoney}");
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
                    Debug.Log($"[Management: {owner.Tag}] Покупка EXP напрямую. Остаток: {FreeMoney}");

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
                    Debug.Log($"[Management: {owner.Tag}] Покупка EXP через продажу слабых героев. Остаток: {FreeMoney}");
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

            Debug.Log($"[Management: {owner.Tag}] Реролл магазина. Остаток: {FreeMoney}");

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
                Debug.Log($"[Management: {owner.Tag}] Магазин зафиксирован из-за дубликата: {hero.name}");
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

        Debug.Log($"[Management: {owner.Tag}] Куплен герой: {hero.name} за {hero.Cost} золота. Остаток: {FreeMoney}");

        heroCard.PurchaceHero(owner.Tag);
        analytics.SetBuyPriorities();
    }

    private void SellHero(Hero hero)
    {
        analytics.SetFreeSpace(1);
        FreeMoney += hero.Level;

        Debug.Log($"[Management: {owner.Tag}] Продан герой: {hero.name} за {hero.Level} золота. Итого: {FreeMoney}");

        Shop.instance.SellHero(hero, owner);
        analytics.SetBuyPriorities();
    }
}
