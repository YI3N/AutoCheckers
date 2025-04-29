using AutoCheckers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Management
{
    private Analytics analytics;
    private Tactics tactics;

    private Player owner;
    private Player opponent;

    private PlayerState state;
    private float damageThreshold = 0.2f;

    public Management(Analytics analytics)
    {
        this.analytics = analytics;
        tactics = analytics.Tactics;
        state = analytics.State;

        owner = analytics.State.Owner;
        opponent = analytics.State.Opponent;
    }

    public void PredictBattleOutcome()
    {
        analytics.SetDangerPlayers();

        float selfDanger = analytics.DangerPlayers[owner.Tag.ToString()];
        float opponentDanger = analytics.DangerPlayers[opponent.Tag.ToString()];
        float mad = analytics.CalculateMAD();

        Debug.Log($"[Analytics:{owner.Tag}] Danger: {selfDanger} | Opponent: {opponentDanger} | MAD: {mad}");

        if (opponentDanger >= selfDanger + mad || selfDanger <= opponentDanger)
        {
            Debug.Log($"[Analytics:{owner.Tag}] ������������: ��������");
            ActOnLoosePrediction();
        }
        else
        {
            Debug.Log($"[Analytics:{owner.Tag}] ������������: ������");
            ActOnWinPrediction();
        }
    }

    private void ActOnWinPrediction()
    {
        state.SetFreeMoney(owner.Money >= 50 ? owner.Money - 50 : owner.Money > 10 ? owner.Money % 11 : 0);
        Debug.Log($"[Management: {owner.Tag}] ��������� �� ������. FreeMoney: {state.FreeMoney}");

        ExecuteRoundStrategy();
    }

    private void ActOnLoosePrediction()
    {
        float possibleDamage = opponent.CalculateDamage() / 2f;
        float damageRation = possibleDamage / owner.CurrentHealth;

        Debug.Log($"[Management: {owner.Tag}] ������������� ����: {possibleDamage}, ���� �����: {damageRation}");

        if (owner.CurrentHealth - possibleDamage <= 0)
        {
            Debug.Log($"[Management: {owner.Tag}] ������� ������. ��� ������ �� ����.");
            state.SetFreeMoney(owner.Money);
            state.SetShouldWin(true);
        }
        else if (damageRation >= damageThreshold)
        {
            state.SetFreeMoney(owner.Money > 50 ? owner.Money - 50 : owner.Money % 21);
            state.SetShouldWin(true);
            Debug.Log($"[Management: {owner.Tag}] ������� ����. ����������� ������. FreeMoney: {state.FreeMoney}");
        }
        else
        {
            int possibleWinBonus = Mathf.Clamp((owner.WinStreak + 1) / 3, 0, 3) + 1;

            state.SetFreeMoney(owner.Money > 50 ? owner.Money - 50 : owner.Money % 11);
            state.SetShouldWin(state.FreeMoney >= possibleWinBonus);
            Debug.Log($"[Management: {owner.Tag}] ��������� ����. FreeMoney: {state.FreeMoney}, ShouldWin: {state.ShouldWin}");
        }

        ExecuteRoundStrategy();
    }

    private void ExecuteRoundStrategy()
    {
        TryBuyHeroes();

        TryBuyLevel();

        if (!TryReroll())
            TryLockShop();
    }

    private void TryBuyHeroes()
    {
        analytics.SetBuyPriorities();

        List<GameObject> shop = Shop.instance.GetCurrentCardShop(owner.Tag);
        Debug.Log($"[Management: {owner.Tag}] ������ ��������. �����: {string.Join(", ", shop.Select(i => i.GetComponent<HeroCard>().HeroPrefab.name))}");

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
            HeroCard shopCard = item.GetComponent<HeroCard>();
            Hero shopHero = shopCard.HeroPrefab.GetComponent<Hero>();

            float heroWeight = 0;

            if (analytics.HeroBuyPriorities.ContainsKey(shopCard.HeroPrefab.name))
            {
                heroWeight = analytics.HeroBuyPriorities[shopCard.HeroPrefab.name];
            }

            if (heroWeight > 4)
                continue;

            Debug.Log($"[Management: {owner.Tag}] ������ �����: {shopHero.name} | Weight: {heroWeight} | Cost: {shopHero.Cost} | FreeMoney: {state.FreeMoney}");

            if (heroWeight > state.WeakestWeight)
            {
                Debug.Log($"[Management: {owner.Tag}] {shopHero.name} ���� ������ ������� ����� ({state.WeakestWeight})");

                if (state.FreeSpace > 0 && state.FreeMoney >= shopHero.Cost)
                {
                    Debug.Log($"[Management: {owner.Tag}] ���� ��������� ����� � ���������� ������. ��������.");
                    BuyHero(shopCard, -1);
                }
                else
                {
                    int count = 1;
                    foreach (Hero hero in state.AllHeroes)
                    {
                        if (shopHero.ID == hero.ID && shopHero.Upgrades == hero.Upgrades)
                        {
                            count++;
                            if (count >= shopHero.CombineThreshold)
                                break;
                        }
                    }

                    if (state.FreeMoney < shopHero.Cost)
                    {
                        List<Hero> benchHeroes = state.HeroesOnBench.OrderBy(h => h.Level).ToList();
                        int totalCost = benchHeroes.Sum(h => h.Level);

                        if (totalCost >= shopHero.Cost)
                        {
                            Debug.Log($"[Management: {owner.Tag}] ������������ ������. ������� ������� ������ � �����.");
                            foreach (Hero sellHero in benchHeroes)
                            {
                                if (analytics.HeroBuyPriorities[sellHero.name] > heroWeight)
                                    continue;

                                float sellHeroDifference = analytics.CountDuplicateFactorLoss(sellHero);
                                float heroDifference = analytics.CountDuplicateFactorGain(shopHero);

                                Debug.Log($"[Management: {owner.Tag}] �������� �� �������: {sellHero.name} | �������: {sellHeroDifference:F2} vs {heroDifference:F2}");

                                if (sellHeroDifference <= heroDifference)
                                {
                                    Debug.Log($"[Management: {owner.Tag}] ������� {sellHero.name} ���� {shopHero.name}.");
                                    SellHero(sellHero);
                                }

                                if (state.FreeMoney >= shopHero.Cost)
                                {
                                    BuyHero(shopCard, -1);
                                    break;
                                }
                            }
                        }
                    }
                    else if (count >= shopHero.CombineThreshold)
                    {
                        Debug.Log($"[Management: {owner.Tag}] ��������� ����� ���������� ({count} / {shopHero.CombineThreshold}). ��������.");
                        BuyHero(shopCard, 2);
                    }
                    else if (state.FreeSpace <= 0)
                    {
                        Debug.Log($"[Management: {owner.Tag}] ��� ���������� �����. ������� �������� ������� �����.");

                        Hero weakHero = state.WeakHeroes[0];

                        float weakHeroDifference = analytics.CountDuplicateFactorLoss(weakHero);
                        float heroDifference = analytics.CountDuplicateFactorGain(shopHero);

                        if (weakHeroDifference <= heroDifference)
                        {
                            Debug.Log($"[Management: {owner.Tag}] ������� {weakHero.name} ���� {shopHero.name}.");
                            SellHero(weakHero);
                        }

                        if (state.FreeMoney >= shopHero.Cost)
                        {
                            BuyHero(shopCard, -1);
                            break;
                        }
                    }
                }
            }
            else if (heroWeight == state.WeakestWeight && state.FreeSpace > 0 && state.FreeMoney >= shopHero.Cost)
            {
                Debug.Log($"[Management: {owner.Tag}] {shopHero.name} ����� �� ���� ���������� ({state.WeakestWeight}), �� ���� ����� � ������. ��������.");
                BuyHero(shopCard, -1);
            }
            else if (state.UniqueHeroes <= owner.Level && state.FreeMoney >= shopHero.Cost)
            {
                Debug.Log($"[Management: {owner.Tag}] ����� ���������� ������ �� ������ ������ ({state.UniqueHeroes} <= {owner.Level}). ����� {shopHero.name} ���� ������������.");
                BuyHero(shopCard, -1);
            }
            else
            {
                Debug.Log($"[Management: {owner.Tag}] �������� {shopHero.name}. ���: {heroWeight}, ��������� �����: {state.FreeSpace}, ������: {state.FreeMoney}");
            }
        }
    }

    private void TryBuyLevel()
    {
        if (state.UniqueHeroes > owner.Level || owner.Level < opponent.Level)
        {
            if (state.FreeMoney >= Shop.instance.EXPCost)
            {
                while (state.FreeMoney >= Shop.instance.EXPCost)
                {
                    state.ChangeFreeMoney(-Shop.instance.EXPCost);
                    Debug.Log($"[Management: {owner.Tag}] ������� EXP ��������. �������: {state.FreeMoney}");
                    analytics.AddManagementAction(() =>
                    {
                        Shop.instance.BuyEXP(owner.Tag);
                    });
                    if (state.UniqueHeroes <= owner.Level || owner.Level >= opponent.Level)
                        break;
                }
            }
            else
            {
                int neededGold = Shop.instance.EXPCost - state.FreeMoney;

                List<Hero> potentialSellers = state.WeakHeroes.Where(h =>
                    state.HeroesOnBench.Contains(h) && state.IsHeroOnBoard(h))
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

                    state.ChangeFreeMoney(-Shop.instance.EXPCost);
                    Debug.Log($"[Management: {owner.Tag}] ������� EXP ����� ������� ������ ������. �������: {state.FreeMoney}");
                    analytics.AddManagementAction(() =>
                    {
                        Shop.instance.BuyEXP(owner.Tag);
                    });
                }
            }
        }
    }

    private bool TryReroll()
    {
        if (state.FreeMoney > Shop.instance.RerollCost)
        {
            state.ChangeFreeMoney(-Shop.instance.RerollCost);

            Debug.Log($"[Management: {owner.Tag}] ������ ��������. �������: {state.FreeMoney}");

            analytics.AddManagementAction(() =>
            {
                Shop.instance.RerollShop(owner.Tag, true);
                ExecuteRoundStrategy();
            });
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
                Debug.Log($"[Management: {owner.Tag}] ������� ������������ ��-�� ���������: {hero.name}");
                Shop.instance.LockShop(owner.Tag);
                break;
            }
        }
    }

    private void BuyHero(HeroCard heroCard, int spaceDifference)
    {
        Hero targetHero = heroCard.HeroPrefab.GetComponent<Hero>();

        state.ChangeFreeSpace(spaceDifference);
        state.ChangeFreeMoney(-targetHero.Cost);
        state.AddBuyHero(heroCard);

        Debug.Log($"[Management: {owner.Tag}] ������ �����: {targetHero.name} �� {targetHero.Cost} ������. �������: {state.FreeMoney}");

        analytics.SetBuyPriorities();
        analytics.AddManagementAction(() =>
        {
            heroCard.PurchaceHero(owner.Tag);
        });
    }

    private void SellHero(Hero hero)
    {
        state.ChangeFreeSpace(1);
        state.ChangeFreeMoney(hero.Level);
        state.AddSellHero(hero);

        Debug.Log($"[Management: {owner.Tag}] ������ �����: {hero.name} �� {hero.Level} ������. �����: {state.FreeMoney}");

        analytics.SetBuyPriorities();
        analytics.AddManagementAction(() =>
        {
            Shop.instance.SellHero(hero, owner);
        });
    }
}
