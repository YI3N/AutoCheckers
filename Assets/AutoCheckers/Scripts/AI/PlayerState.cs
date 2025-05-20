using AutoCheckers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class PlayerState
{
    public Player Owner { get; private set; }
    public Player Opponent { get; private set; }

    public List<Hero> AllHeroes
    { 
        get { return PiecesOnBoard.Concat(PiecesOnBench).Where(piece => piece != null).Select(piece => piece.GetComponent<Hero>()).ToList(); } 
    }
    public List<Hero> HeroesOnBoard
    {
        get { return PiecesOnBoard.Where(piece => piece != null).Select(piece => piece.GetComponent<Hero>()).ToList(); }
    }
    public List<Hero> HeroesOnBench
    {
        get { return PiecesOnBench.Where(piece => piece != null).Select(piece => piece.GetComponent<Hero>()).ToList(); }
    }

    public List<GameObject> PiecesOnBoard { get; private set; }
    public List<GameObject> PiecesOnBench { get; private set; }
    public Dictionary<HeroClass, int> ClassHeroes { get; private set; }
    public Dictionary<Race, int> RaceHeroes { get; private set; }
    public Dictionary<HeroClass, int> BenchClasses { get; private set; }
    public Dictionary<Race, int> BenchRaces { get; private set; }

    public List<GameObject> HeroesToBuy { get; private set; }
    public List<GameObject> HeroesToSell { get; private set; }

    public float WeakestWeight { get; private set; }
    public List<Hero> WeakHeroes
    {
        get { return weakHeroes.OrderBy(h => h.Level).ToList(); }
    }

    public int UniqueHeroes { get; private set; }
    public int FreeSpace { get; private set; }
    public int FreeMoney { get; private set; }
    public bool ShouldWin { get; private set; }

    private List<Hero> weakHeroes;

    public PlayerState(Player player)
    {
        Owner = player;
        Opponent = Owner.Tag == GameTag.Human ? GameManager.instance.AI : GameManager.instance.Human;

        ResetState();
    }

    public void ResetState()
    {
        PiecesOnBoard = new List<GameObject>(Owner.HeroesOnBoard);
        PiecesOnBench = new List<GameObject>(Owner.HeroesOnBench);

        ClassHeroes = new Dictionary<HeroClass, int>(Owner.ClassHeroes);
        RaceHeroes = new Dictionary<Race, int>(Owner.RaceHeroes);

        BenchClasses = new Dictionary<HeroClass, int>(Owner.BenchClasses);
        BenchRaces = new Dictionary<Race, int>(Owner.BenchRaces);

        HeroesToBuy = new List<GameObject>();
        HeroesToSell = new List<GameObject>();

        WeakestWeight = 0;
        weakHeroes = new List<Hero>();

        UniqueHeroes = 0;
        FreeSpace = 0;
        FreeMoney = 0;
        ShouldWin = false;
    }

    public void AddBuyHero(HeroCard card)
    {
        Hero hero = card.HeroPrefab.GetComponent<Hero>();
        HeroesToBuy.Add(card.HeroPrefab);

        bool isDuplicate = IsHeroOnBoard(hero);
        PiecesOnBoard.Add(card.HeroPrefab);
        if (!isDuplicate)
        {
            ClassHeroes.AddValue(hero.HeroClass, 1);
            RaceHeroes.AddValue(hero.Race, 1);
        }
    }

    public void AddSellHero(Hero hero)
    {
        if (hero.CurrentCell.IsBench)
        {
            bool isDuplicate = IsHeroOnBench(hero);
            PiecesOnBench.Remove(hero.gameObject);
            if (!isDuplicate)
            {
                BenchClasses.AddValue(hero.HeroClass, 1);
                BenchRaces.AddValue(hero.Race, 1);
            }
        }
        else
        {
            bool isDuplicate = IsHeroOnBoard(hero);
            PiecesOnBoard.Remove(hero.gameObject);
            if (!isDuplicate)
            {   
                ClassHeroes.AddValue(hero.HeroClass, 1);
                RaceHeroes.AddValue(hero.Race, 1);
            }
        }
    }

    public void SetWeakestWeight(float weight)
    {
        WeakestWeight = weight;
    }

    public void ClearWeakHeroes()
    {
        weakHeroes.Clear();
    }

    public void AddWeakHero(Hero hero)
    {
        weakHeroes.Add(hero);
    }

    public void SetUniqueHeroes(int amount)
    {
        UniqueHeroes = amount;
    }

    public void ChangeUniqueHeroes(int amount)
    {
        UniqueHeroes += amount;
    }

    public void SetFreeSpace(int amount)
    {
        FreeSpace = amount;
        Debug.Log($"[Analytics:{Owner.Tag}] Свободное место изменено на {amount}, теперь {FreeSpace}");
    }

    public void ChangeFreeSpace(int amount)
    {
        FreeSpace += amount;
    }

    public void SetFreeMoney(int amount)
    {
        FreeMoney = amount;
    }

    public void ChangeFreeMoney(int amount)
    {
        FreeMoney += amount;
    }

    public void SetShouldWin(bool result)
    {
        ShouldWin = result;
    }

    public bool IsHeroOnBoard(Hero targetHero)
    {
        return HeroesOnBoard.Any(hero => targetHero.ID == hero.ID);
    }

    public bool IsHeroOnBench(Hero targetHero)
    {
        return HeroesOnBench.Any(hero => targetHero.ID == hero.ID);
    }
}
