using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AutoCheckers;
using System.Linq;
using System;

public class Player
{
    private int[] exps = { 0, 1, 1, 2, 4, 8, 16, 24, 32, 40 };
    public List<GameObject> HeroesOnBoard { get; private set; } = new List<GameObject>();
    public List<GameObject> HeroesOnBench { get; private set; } = new List<GameObject>();
    public BoardCell[] Bench { get; private set; } = new BoardCell[8];
    public Dictionary<HeroClass, int> ClassHeroes { get;  set; } = new Dictionary<HeroClass, int>();
    public Dictionary<Race, int> RaceHeroes { get;  set; } = new Dictionary<Race, int>();

    public GameTag Tag { get; private set; }
    public int FightHeroes { get; private set; } = 0;

    public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; } = 100;
    public int Level { get; private set; } = 1;
    public int Exp { get; private set; } = 0;
    public int MaxExp
    {
        get {  return exps[Level]; }
    }
    public int Money { get; private set; } = 100;
    public int WinStreak { get; private set; } = 0;
    public int Wins { get; private set; } = 0;
    public int Losses { get; private set; } = 0;
    public int AttackStatistics { get; private set; } = 0;
    public int DefenceStatistics { get; private set; } = 0;

    public Player(GameTag tag)
    {
        this.Tag = tag;
    }

    public void ActivateAbilities()
    {
        foreach (GameObject piece in HeroesOnBoard)
        {
            Hero hero = piece.GetComponent<Hero>();
            hero.ClassAbility.ActivateAbility(ClassHeroes[hero.HeroClass]);
            hero.RaceAbility.ActivateAbility(RaceHeroes[hero.Race]);
        }
    }

    public void DeactivateAbilities()
    {
        foreach (GameObject piece in HeroesOnBoard)
        {
            Hero hero = piece.GetComponent<Hero>();
            hero.ClassAbility.DeactivateAbility(ClassHeroes[hero.HeroClass]);
            hero.RaceAbility.DeactivateAbility(RaceHeroes[hero.Race]);
        }
    }

    public bool IsHeroOnBoard(GameObject hero)
    {
        return HeroesOnBoard.Any(piece => hero.GetComponent<Hero>().ID == piece.GetComponent<Hero>().ID);
    }

    public void SetHeroToBoard(GameObject hero)
    {
        bool isDuplicate = IsHeroOnBoard(hero);

        HeroesOnBoard.Add(hero);

        if (!isDuplicate)
        {
            ClassHeroes.AddValue(hero.GetComponent<Hero>().HeroClass, 1);
            RaceHeroes.AddValue(hero.GetComponent<Hero>().Race, 1);
            UIManager.instance.UpdatePlayerUI(this);
        }
    }

    public void MoveHeroToBoard(GameObject hero)
    {
        bool isDuplicate = IsHeroOnBoard(hero);

        HeroesOnBoard.Add(hero);
        HeroesOnBench.Remove(hero);

        if (!isDuplicate)
        {
            ClassHeroes[hero.GetComponent<Hero>().HeroClass] += 1;
            RaceHeroes[hero.GetComponent<Hero>().Race] += 1;
            UIManager.instance.UpdatePlayerUI(this);
        }
    }

    public void MoveHeroToBench(GameObject hero)
    {
        HeroesOnBench.Add(hero);
        HeroesOnBoard.Remove(hero);

        bool isDuplicate = IsHeroOnBoard(hero);

        if (!isDuplicate)
        {
            ClassHeroes[hero.GetComponent<Hero>().HeroClass] -= 1;
            RaceHeroes[hero.GetComponent<Hero>().Race] -= 1;
            UIManager.instance.UpdatePlayerUI(this);
        }
    }

    public void AddFightHero()
    {
        FightHeroes++;
    }

    public void ResetFightHeroes()
    {
        foreach (GameObject hero in HeroesOnBoard)
        {
            hero.SetActive(true);
            hero.GetComponent<Hero>().ResetHero();
        }

        FightHeroes = 0;
    }

    public void HeroDied()
    {
        FightHeroes--;
    }

    public void Purchase(int money)
    {
        Money -= money;
        UIManager.instance.UpdatePlayerUI(this);
    }

    public void GainMoney(int money)
    {
        Money += money;
        Money = Mathf.Clamp(Money, 0, 100);
        UIManager.instance.UpdatePlayerUI(this);
    }

    public void GainEXP(int exp)
    {
        Exp += exp;
        while (true)
        {
            if (Exp >= MaxExp)
            {
                Exp -= MaxExp;
                Level += 1;
                Shop.instance.UpdateChances(this);
            }
            else
            {
                break;
            }
        }
        UIManager.instance.UpdatePlayerUI(this);
    }

    public void CalculateStatistic(GameTag roundResult)
    {
        if (roundResult == GameTag.Draw)
        {
            WinStreak = 0;
        }
        else if (roundResult == Tag)
        {
            Wins++;
            WinStreak++;
        }
        else
        {
            Losses++;
            WinStreak = 0;
        }

        foreach (GameObject hero in HeroesOnBoard)
        {
            AttackStatistics += hero.GetComponent<Hero>().AttackStatistics;
            DefenceStatistics += hero.GetComponent<Hero>().DefenceStatistics;
        }

        UIManager.instance.SetStatistics(this);
        UIManager.instance.UpdatePlayerUI(this);
    }

    public int CalculateDamage()
    {
        int damage = 0;
        foreach (GameObject hero in HeroesOnBoard)
        {
            if (hero.activeSelf == false)
                continue;

            damage += 1 + hero.GetComponent<Hero>().Level / 3;
        }
        return damage;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            //Show Results
        }
        UIManager.instance.UpdatePlayerUI(this);
    }
}
