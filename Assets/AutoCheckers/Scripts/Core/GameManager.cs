using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AutoCheckers;
using TMPro;
using System.ComponentModel;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField]
    private float attackTime = 1;
    [SerializeField]
    private float damageTime = 1;

    public GameObject testEnemy;

    private GameObject heroHit;
    private List<Hero> fightHeroes = new List<Hero>();

    public Analytics analytics;

    public Player Human { get; private set; } = new Player(GameTag.Human);
    public Player AI { get; private set; } = new Player(GameTag.AI);
    public int Round { get; private set; } = 1;
    public bool IsPlaying { get; private set; } = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance == this)
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] cells = GameObject.FindGameObjectsWithTag(nameof(GameTag.HumanBench));
        foreach (GameObject cell in cells)
        {
            Human.Bench[cell.GetComponent<BoardCell>().Row] = cell.GetComponent<BoardCell>();
        }

        cells = GameObject.FindGameObjectsWithTag(nameof(GameTag.AIBench));
        foreach (GameObject cell in cells)
        {
            AI.Bench[cell.GetComponent<BoardCell>().Row] = cell.GetComponent<BoardCell>();
        }

        analytics = new Analytics(Human);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsPlaying && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
                HandleRaycastHit(hit);
            else
                ClearSelection();
        }
    }

    private void HandleRaycastHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        switch (hitObject.tag)
        {
            case nameof(GameTag.Human):
                HandleHeroSelection(hitObject);
                break;
            case nameof(GameTag.Board):
                HandleBoardPlacement(hitObject);
                break;
            case nameof(GameTag.HumanBench):
                HandleBenchPlacement(hitObject);
                break;
            case nameof(GameTag.Sell):
                HandleHeroSell();
                break;
            default:
                ClearSelection();
                break;
        }
    }

    private void HandleHeroSelection(GameObject hitObject)
    {
        if (heroHit == null)
        {
            heroHit = hitObject;
            UIManager.instance.ShowSellPrice(heroHit.GetComponent<Hero>().Level);
        }
        else
        {
            SwapHeroes(hitObject);
        }
    }

    private void HandleBoardPlacement(GameObject hitObject)
    {
        if (heroHit != null && hitObject.GetComponent<BoardCell>().Row < 4)
        {
            heroHit.GetComponent<Hero>().SetStartCell(hitObject.GetComponent<BoardCell>());
            ClearSelection();
        }
    }

    private void HandleBenchPlacement(GameObject hitObject)
    {
        if (heroHit != null)
        {
            heroHit.GetComponent<Hero>().SetStartCell(hitObject.GetComponent<BoardCell>());
            ClearSelection();
        }
    }

    private void SwapHeroes(GameObject hitObject)
    {
        Hero switchHero = hitObject.GetComponent<Hero>();
        BoardCell switchCell = heroHit.GetComponent<Hero>().CurrentCell;

        heroHit.GetComponent<Hero>().SetStartCell(switchHero.CurrentCell);
        switchHero.SetStartCell(switchCell);

        ClearSelection();
    }

    private void HandleHeroSell()
    {
        if (heroHit != null)
        {
            heroHit.GetComponent<Hero>().Owner.HeroesOnBoard.Remove(heroHit);
            Shop.instance.SellHero(heroHit.GetComponent<Hero>(), Human);
            ClearSelection();
        }
    }

    private void ClearSelection()
    {
        heroHit = null;
        UIManager.instance.HideSellPrice();
    }

    // тут тест
    public void BeginGame()
    {
        GameObject dummy = Instantiate(testEnemy);
        Board.instance.SetTestHero(dummy, 4, 3, GameTag.AI);

        GameObject dummy2 = Instantiate(testEnemy);
        Board.instance.SetTestHero(dummy2, 4, 4, GameTag.AI);

        GameObject dummy3 = Instantiate(testEnemy);
        Board.instance.SetTestHero(dummy3, 4, 5, GameTag.AI);
    }

    private IEnumerator RoundTick()
    {
        yield return new WaitForSeconds(attackTime);

        IExtensions.Shuffle(fightHeroes);

        foreach (Hero hero in fightHeroes)
            hero.TakeAction();

        yield return new WaitForSeconds(damageTime);

        foreach (Hero hero in fightHeroes)
            hero.TakeDamage();

        if (Human.FightHeroes == 0 || AI.FightHeroes == 0)
            EndRound();
        else
            StartCoroutine(RoundTick());
    }

    private void PrepairBoard(Player player)
    {
        for (int i = 0; i < player.HeroesOnBoard.Count; i++)
        {
            Hero hero = player.HeroesOnBoard[i].GetComponent<Hero>();
            hero.ClearStatistic();

            if (player.HeroesOnBoard.Count <= player.Level)
            {
                fightHeroes.Add(hero);
                player.AddFightHero();
            }
            else if (Human.HeroesOnBench.Count < player.Bench.Length)
            {
                foreach (BoardCell cell in player.Bench)
                {
                    if (cell.IsOccupied == false)
                    {
                        hero.SetStartCell(cell);
                        i--;
                        break;
                    }
                }
            }
            else
            {
                player.HeroesOnBoard.Remove(hero.gameObject);
                Shop.instance.SellHero(hero, player);
                i--;
            }
        }
    }

    // тут тест
    public void StartRound()
    {
        IsPlaying = true;

        Human.RaceHeroes[Race.Human] = 3;

        PrepairBoard(Human);
        PrepairBoard(AI);

        Human.ActivateAbilities();
        AI.ActivateAbilities();

        StartCoroutine(RoundTick());
    }

    public void EndRound()
    {
        IsPlaying = false;
        fightHeroes.Clear();

        GameTag roundResult = DetermineRoundResult();

        DeactivatePlayers();

        ProcessMoney();

        switch (roundResult)
        {
            case GameTag.Human:
                HandleHumanVictory();
                break;
            case GameTag.AI:
                HandleAIVictory();
                break;
            default:
                HandleDraw();
                break;
        }

        UpdateWinStreak();
        GiveExperience();

        Round++;
        UIManager.instance.EndRound();

        analytics.SetDangerPlayers();
        analytics.SetBattlePriorities();
        analytics.SetBuyPriorities();
    }

    private GameTag DetermineRoundResult()
    {
        if (Human.FightHeroes == 0 && AI.FightHeroes == 0)
            return GameTag.Draw;
        else if (Human.FightHeroes == 0)
            return GameTag.AI;
        else
            return GameTag.Human;
    }

    private void DeactivatePlayers()
    {
        Human.DeactivateAbilities();
        AI.DeactivateAbilities();
        Human.ResetFightHeroes();
        AI.ResetFightHeroes();
    }

    private void ProcessMoney()
    {
        int humanMoneyBonus = Mathf.Clamp(Human.Money / 10, 0, 5);
        int aiMoneyBonus = Mathf.Clamp(AI.Money / 10, 0, 5);

        Human.GainMoney(humanMoneyBonus);
        AI.GainMoney(aiMoneyBonus);

        int roundMoneyBonus = Mathf.Clamp(Round, 1, 5);
        Human.GainMoney(roundMoneyBonus);
        AI.GainMoney(roundMoneyBonus);
    }

    private void HandleHumanVictory()
    {
        Human.CalculateStatistic(GameTag.Human);
        Human.GainMoney(1);

        AI.CalculateStatistic(GameTag.Human);
        AI.TakeDamage(Human.CalculateDamage());
    }

    private void HandleAIVictory()
    {
        AI.CalculateStatistic(GameTag.AI);
        AI.GainMoney(1);

        Human.CalculateStatistic(GameTag.AI);
        Human.TakeDamage(AI.CalculateDamage());
    }

    private void HandleDraw()
    {
        Human.CalculateStatistic(GameTag.Draw);
        AI.CalculateStatistic(GameTag.Draw);
    }

    private void UpdateWinStreak()
    {
        int humanWinStreakBonus = Mathf.Clamp(Human.WinStreak / 3, 0, 3);
        int aiWinStreakBonus = Mathf.Clamp(AI.WinStreak / 3, 0, 3);

        Human.GainMoney(humanWinStreakBonus);
        AI.GainMoney(aiWinStreakBonus);
    }

    private void GiveExperience()
    {
        Human.GainEXP(1);
        AI.GainEXP(1);
    }
}
