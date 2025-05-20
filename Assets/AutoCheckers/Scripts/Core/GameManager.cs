using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AutoCheckers;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public readonly float attackTime = .5f;
    public readonly float damageTime = .5f;

    public readonly float pauseDelay = 30f;

    public Player Human { get; private set; } = new Player(GameTag.Human);
    public Player AI { get; private set; } = new Player(GameTag.AI);
    public int Round { get; private set; } = 1;
    public bool IsPlaying { get; private set; } = false;

    private GameObject heroHit;
    private List<Hero> fightHeroes = new List<Hero>();

    private Analytics AIAnalytics;
    private Analytics HumanAnalytics;

    private bool player1AI = false;
    private float pauseTimer = Mathf.Infinity;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance == this)
            Destroy(this);
    }

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
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();

        if (!IsPlaying)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                    HandleRaycastHit(hit);
                else
                    ClearSelection();
            }

            if (pauseTimer > 0)
            {
                pauseTimer -= Time.deltaTime;
                UIManager.instance.UpdateTimer(pauseTimer);
            }
            else
            {
                UIManager.instance.StartRound();
            }

            if (AIAnalytics != null)
                AIAnalytics.HandleActions();
            if (player1AI)
                HumanAnalytics.HandleActions();
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
            if (!hitObject.GetComponent<BoardCell>().IsOccupied)
            {
                heroHit.GetComponent<Hero>().SetStartCell(hitObject.GetComponent<BoardCell>());
            }
            else
                SwapHeroes(hitObject.GetComponent<BoardCell>().OccupiedHero.gameObject);
            ClearSelection();
        }
    }

    private void HandleBenchPlacement(GameObject hitObject)
    {
        if (heroHit != null)
        {
            if (!hitObject.GetComponent<BoardCell>().IsOccupied)
            {
                heroHit.GetComponent<Hero>().SetStartCell(hitObject.GetComponent<BoardCell>());
            }
            else
                SwapHeroes(hitObject.GetComponent<BoardCell>().OccupiedHero.gameObject);
            ClearSelection();
        }
    }

    private void SwapHeroes(GameObject hitObject)
    {
        if (hitObject != heroHit)
        {
            Hero switchHero = hitObject.GetComponent<Hero>();
            BoardCell switchCell = heroHit.GetComponent<Hero>().CurrentCell;

            heroHit.GetComponent<Hero>().SetStartCell(switchHero.CurrentCell);
            switchHero.SetStartCell(switchCell);
        }

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

    public void BeginGame(string player1, string player2)
    {
        if (player1 == "Человек")
        {
            player1AI = false;
        }
        else if (player1 == "ИИ")
        {
            Time.timeScale = 4f;
            HumanAnalytics = new Analytics(Human);
            player1AI = true;
        }

        if (player2 == "ИИ")
            AIAnalytics = new Analytics(AI);

        AIAnalytics.StartThinking();
        if (player1AI)
            HumanAnalytics.StartThinking();
        pauseTimer = pauseDelay;
    }

    public void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    // Жесткий костыль
    private void RemoveBugs(Player player)
    {
        List<GameObject> toMove = new List<GameObject>();

        foreach (GameObject piece in player.HeroesOnBoard)
        {
            Hero hero = piece.GetComponent<Hero>();
            if (hero.CurrentCell.tag != nameof(GameTag.Board))
                toMove.Add(piece);
        }

        foreach (GameObject piece in toMove)
        {
            player.HeroesOnBoard.Remove(piece);
            player.HeroesOnBench.Add(piece);
        }

        player.ClassHeroes.Clear();
        player.RaceHeroes.Clear();
        player.BenchClasses.Clear();
        player.BenchRaces.Clear();

        foreach (GameObject piece in player.HeroesOnBoard.GroupBy(go => go.GetComponent<Hero>().ID).Select(g => g.First()).ToList())
        {
            Hero hero = piece.GetComponent<Hero>();
            player.ClassHeroes.AddValue(hero.HeroClass, 1);
            player.RaceHeroes.AddValue(hero.Race, 1);
        }

        foreach (GameObject piece in player.HeroesOnBench.GroupBy(go => go.GetComponent<Hero>().ID).Select(g => g.First()).ToList())
        {
            Hero hero = piece.GetComponent<Hero>();
            if (!player.IsHeroOnBoard(piece))
            {
                player.BenchClasses.AddValue(hero.HeroClass, 1);
                player.BenchRaces.AddValue(hero.Race, 1);
            }
        }
    }

    private void PrepairBoard(Player player)
    {
        RemoveBugs(player);

        for (int i = 0; i < player.HeroesOnBoard.Count; i++)
        {
            Hero hero = player.HeroesOnBoard[i].GetComponent<Hero>();
            hero.ClearStatistic();

            if (player.HeroesOnBoard.Count <= player.Level)
            {
                fightHeroes.Add(hero);
                player.AddFightHero();
            }
            else if (player.HeroesOnBench.Count < player.Bench.Length)
            {
                foreach (BoardCell cell in player.Bench)
                {
                    if (!cell.IsOccupied)
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

    public void StartRound()
    {
        IsPlaying = true;

        Human.ResetStatistics();
        AI.ResetStatistics();

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

        GiveStreaksBonus();
        GiveExperience();

        Round++;
        UIManager.instance.EndRound();

        UpdateShops();

        pauseTimer = pauseDelay;
        AIAnalytics.StartThinking();
        if (player1AI)
            HumanAnalytics.StartThinking();
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

    private void GiveStreaksBonus()
    {
        int humanWinStreakBonus = Mathf.Clamp(Human.WinStreak / 3, 0, 3);
        int aiWinStreakBonus = Mathf.Clamp(AI.WinStreak / 3, 0, 3);

        int humanLooseStreakBonus = Mathf.Clamp(Human.LooseStreak / 3, 0, 3);
        int aiLooseStreakBonus = Mathf.Clamp(AI.LooseStreak / 3, 0, 3);

        Human.GainMoney(humanWinStreakBonus);
        AI.GainMoney(aiWinStreakBonus);

        Human.GainMoney(humanLooseStreakBonus);
        AI.GainMoney(aiLooseStreakBonus);
    }

    private void GiveExperience()
    {
        Human.GainEXP(1);
        AI.GainEXP(1);
    }
    
    private void UpdateShops()
    {
        Shop.instance.RerollShop(GameTag.Human, false);
        Shop.instance.RerollShop(GameTag.AI, false);
    }
}
