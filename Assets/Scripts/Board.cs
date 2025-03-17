using AutoCheckers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board instance = null;

    private BoardCell[,] boardCells = new BoardCell[8, 8];

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance == this)
            Destroy(this);
    }

    void Start()
    {
        GameObject[] cells = GameObject.FindGameObjectsWithTag(nameof(GameTag.Board));
        foreach (GameObject cell in cells)
        {
            boardCells[cell.GetComponent<BoardCell>().Row, cell.GetComponent<BoardCell>().Col] = cell.GetComponent<BoardCell>();
        }
    }

    void Update()
    {
        
    }

    public void SetHumanHero(GameObject hero)
    {
        GameManager.instance.Human.SetHeroToBoard(hero);

        foreach (BoardCell cell in boardCells)
        {
            if (cell.Row > 3)
                break;

            if (cell.IsOccupied == false)
            {
                hero.GetComponent<Hero>().SetTags(true);
                hero.GetComponent<Hero>().SetStartCell(cell);
                break;
            }
        }

        hero.GetComponent<Hero>().CheckHeroUpgrade();
    }

    public void SetAIHero(GameObject hero, int row, int col)
    {
        GameManager.instance.AI.SetHeroToBoard(hero);

        hero.GetComponent<Hero>().SetTags(false);
        hero.GetComponent<Hero>().SetStartCell(boardCells[row, col]);
        hero.GetComponent<Hero>().CheckHeroUpgrade();
    }

    public List<BoardCell> GetAvailableCells(BoardCell targetCell, BoardCell ownersCell, int moveDistance, int attackRange)
    {
        List<BoardCell> availableCells = new List<BoardCell>();

        for (int i = -attackRange; i <= attackRange; i++)
        {
            if (targetCell.Row + i < 0 || targetCell.Row + i > 7)
                continue;

            for (int j = -attackRange; j <= attackRange; j++)
            {
                if (targetCell.Col + j < 0 || targetCell.Col + j > 7)
                    continue;

                BoardCell possibleCell = boardCells[targetCell.Row + i, targetCell.Col + j];

                if (Mathf.FloorToInt(Vector2.Distance(ownersCell.GetBoardPosition(), possibleCell.GetBoardPosition())) > moveDistance)
                    continue;
                if (Mathf.FloorToInt(Vector2.Distance(possibleCell.GetBoardPosition(), targetCell.GetBoardPosition())) > attackRange)
                    continue;

                if (possibleCell.IsOccupied == false)
                    availableCells.Add(possibleCell);
            }
        }

        for (int i = -moveDistance; i <= moveDistance; i++)
        {
            if (ownersCell.Row + i < 0 || ownersCell.Row + i > 7)
                continue;

            for (int j = -moveDistance; j <= moveDistance; j++)
            {
                if (ownersCell.Col + j < 0 || ownersCell.Col + j > 7)
                    continue;

                BoardCell possibleCell = boardCells[ownersCell.Row + i, ownersCell.Col + j];

                if (Mathf.FloorToInt(Vector2.Distance(ownersCell.GetBoardPosition(), possibleCell.GetBoardPosition())) > moveDistance)
                    continue;

                if (possibleCell.IsOccupied == false)
                    availableCells.Add(possibleCell);
                else if (possibleCell.Row == ownersCell.Row && possibleCell.Col == ownersCell.Col)
                    availableCells.Add(possibleCell);
            }
        }

        return availableCells;
    }

    public List<Hero> GetHeroesInRange(BoardCell targetCell, int range)
    {
        List<Hero> enemies = new List<Hero>();

        for (int i = -range; i <= range; i++)
        {
            if (targetCell.Row + i < 0 || targetCell.Row + i > 7)
                continue;

            for (int j = -range; j <= range; j++)
            {
                if (targetCell.Col + j < 0 || targetCell.Col + j > 7)
                    continue;

                BoardCell cell = boardCells[targetCell.Row + i, targetCell.Col + j];

                if (cell.OccupiedHero != null)
                    enemies.Add(cell.OccupiedHero);
            }
        }

        return enemies;
    }
}
