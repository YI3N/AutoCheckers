using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AutoCheckers;
using TMPro;
using System.ComponentModel;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsPlaying && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (heroHit == null && hit.collider.tag == nameof(GameTag.Human))
                {
                    heroHit = hit.collider.gameObject;
                    UIManager.instance.ShowSellPrice(heroHit.GetComponent<Hero>().Level);
                }
                else if (heroHit != null && hit.collider.tag == nameof(GameTag.Board) && hit.collider.gameObject.GetComponent<BoardCell>().Row < 4)
                {
                    heroHit.GetComponent<Hero>().SetStartCell(hit.collider.gameObject.GetComponent<BoardCell>());
                    heroHit = null;
                    UIManager.instance.HideSellPrice();
                }
                else if (heroHit != null && hit.collider.tag == nameof(GameTag.HumanBench))
                {
                    heroHit.GetComponent<Hero>().SetStartCell(hit.collider.gameObject.GetComponent<BoardCell>());
                    heroHit = null;
                    UIManager.instance.HideSellPrice();
                }
                else if (heroHit != null && hit.collider.tag == nameof(GameTag.Human))
                {
                    Hero switchHero = hit.collider.gameObject.GetComponent<Hero>();
                    BoardCell switcCell = heroHit.GetComponent<Hero>().CurrentCell;

                    heroHit.GetComponent<Hero>().SetStartCell(switchHero.CurrentCell);
                    switchHero.SetStartCell(switcCell);
                    heroHit = null;
                    UIManager.instance.HideSellPrice();
                }
                else if (heroHit != null && hit.collider.tag == nameof(GameTag.Sell))
                {
                    heroHit.GetComponent<Hero>().Owner.HeroesOnBoard.Remove(heroHit);
                    Shop.instance.SellHero(heroHit.GetComponent<Hero>(), Human);
                    heroHit = null;
                    UIManager.instance.HideSellPrice();
                }
                else
                {
                    heroHit = null;
                    UIManager.instance.HideSellPrice();
                }
            }
            else
            {
                heroHit = null;
                UIManager.instance.HideSellPrice();
            }
        }
    }

    public void BeginGame()
    {
        GameObject dummy = Instantiate(testEnemy);
        Board.instance.SetAIHero(dummy, 4, 3);

        GameObject dummy2 = Instantiate(testEnemy);
        Board.instance.SetAIHero(dummy2, 4, 4);

        GameObject dummy3 = Instantiate(testEnemy);
        Board.instance.SetAIHero(dummy3, 4, 5);
    }

    private IEnumerator RoundTick()
    {
        yield return new WaitForSeconds(attackTime);

        IListExtensions.Shuffle(fightHeroes);

        foreach (Hero hero in fightHeroes)
        {
            hero.Attack();
        }

        yield return new WaitForSeconds(damageTime);

        foreach (Hero hero in fightHeroes)
        {
            hero.TakeDamage();
        }

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

    public void StartRound()
    {
        IsPlaying = true;

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

        GameTag roundResult;
        if (Human.FightHeroes == 0 && AI.FightHeroes == 0)
            roundResult = GameTag.Draw;
        else if (Human.FightHeroes == 0)
            roundResult = GameTag.AI;
        else
            roundResult = GameTag.Human;

        Human.DeactivateAbilities();
        AI.DeactivateAbilities();

        Human.ResetFightHeroes();
        AI.ResetFightHeroes();

        Human.GainMoney(Mathf.Clamp(Human.Money / 10, 0, 5));
        AI.GainMoney(Mathf.Clamp(Human.Money / 10, 0, 5));

        Human.GainMoney(Mathf.Clamp(Round, 1, 5));
        AI.GainMoney(Mathf.Clamp(Round, 1, 5));

        switch (roundResult)
        {
            case GameTag.Human:
                Human.CalculateStatistic(GameTag.Human);
                Human.GainMoney(1);

                AI.CalculateStatistic(GameTag.Human);
                AI.TakeDamage(Human.CalculateDamage());
                break;
            case GameTag.AI:
                AI.CalculateStatistic(GameTag.AI);
                AI.GainMoney(1);

                Human.CalculateStatistic(GameTag.AI);
                Human.TakeDamage(AI.CalculateDamage());
                break;
            default:
                Human.CalculateStatistic(GameTag.Draw);
                AI.CalculateStatistic(GameTag.Draw);
                break;
        }

        Human.GainMoney(Mathf.Clamp(Human.WinStreak / 3, 0, 3));
        AI.GainMoney(Mathf.Clamp(Human.WinStreak / 3, 0, 3));

        Human.GainEXP(1);
        AI.GainEXP(1);

        Round++;
        UIManager.instance.EndRound();
    }
}
