using AutoCheckers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tactics
{
    private Player owner;
    private Player opponent;

    private int[,] dangerBoard = new int[8, 8];

    private List<Hero> battleHeroes = new List<Hero>();
    private List<Hero> benchHeroes = new List<Hero>();

    public Tactics(Player player)
    {
        owner = player;
        opponent = owner.Tag == GameTag.Human ? GameManager.instance.AI : GameManager.instance.Human;
    }

    public void CreateTactic(Dictionary<string, float> battlePriority)
    {
        SetDangerBoard();
        SetBattleOrder(battlePriority);
    }

    private void SetDangerBoard()
    {
        Array.Clear(dangerBoard, 0, dangerBoard.Length);

        foreach (GameObject piece in opponent.HeroesOnBoard)
        {
            Hero enemy = piece.GetComponent<Hero>();
            dangerBoard[enemy.StartCell.Row, enemy.StartCell.Col] = 1;
        }

        if (IsDangerBoardEmpty())
            dangerBoard[3, owner.Tag == GameTag.Human ? 3 : 4] = 1;
    }

    private bool IsDangerBoardEmpty()
    {
        for (int row = 0; row < dangerBoard.GetLength(0); row++)
        {
            for (int col = 0; col < dangerBoard.GetLength(1); col++)
            {
                if (dangerBoard[row, col] != 0)
                    return false;
            }
        }

        return true;
    }

    public void SetHeroes(Dictionary<string, float> heroWeights)
    {
        battleHeroes.Clear();
        benchHeroes.Clear();

        List<GameObject> heroes = owner.HeroesOnBoard.Concat(owner.HeroesOnBench).ToList();

        Dictionary<string, Hero> battleList = new Dictionary<string, Hero>();

        foreach (GameObject piece in heroes)
        {
            Hero hero = piece.GetComponent<Hero>();

            if (!battleList.ContainsKey(hero.name))
            {
                battleList[hero.name] = hero;
            }
            else if (hero.Level > battleList[hero.name].Level)
            {
                battleList[hero.name] = hero;
            }

        }

        battleHeroes = battleList.Values
            .OrderByDescending(h => heroWeights[h.name])
            .Take(owner.Level)
            .ToList();

        foreach (GameObject piece in heroes)
        {
            Hero hero = piece.GetComponent<Hero>();
            if (!battleHeroes.Contains(hero))
                benchHeroes.Add(hero);
        }

        MoveHeroesToBoard(battleHeroes);
        MoveHeroesToBench(benchHeroes);
    }

    private void MoveHeroesToBoard(List<Hero> heroes)
    {
        int row = owner.Tag == GameTag.Human ? 0 : 4;
        int col = 0;

        foreach (Hero hero in heroes)
        {
            BoardCell cell = Board.instance.GetCell(row, col);
            hero.SetStartCell(cell);

            if (col > 7)
            {
                row++;
                col = 0;
            }
            else
                col++;
        }
    }

    private void MoveHeroesToBench(List<Hero> heroes)
    {
        int i = 0;

        foreach (Hero hero in heroes)
        {
            BoardCell cell = owner.Bench[i];
            hero.SetStartCell(cell);
            i++;
        }
    }

    private void SetBattleOrder(Dictionary<string, float> battlePriority)
    {
        Vector2 target = GetMainDangerPoint();
        Vector2 neighbor = GetMostDangerousNeighbor(target);

        foreach (KeyValuePair<string, float> kvp in battlePriority)
        {
            GameObject piece = owner.HeroesOnBoard.FirstOrDefault(piece => piece.GetComponent<Hero>().name == kvp.Key);
            Hero hero = piece.GetComponent<Hero>();

            if (hero.AttackRange == 1)
                SetMeleePosition(hero, target, neighbor);
            else
                SetRangedPosition(hero, target, neighbor);
        }
    }

    private void SetMeleePosition(Hero hero, Vector2 target, Vector2 neighbor)
    {
        List<Vector2> freeCells = GetFreeCellsForMelee();

        Vector2 bestCell = freeCells[0];

        int bestDist = Mathf.FloorToInt(Vector2.Distance(bestCell, target));

        foreach (Vector2 cell in freeCells)
        {
            int dist = Mathf.FloorToInt(Vector2.Distance(cell, target));
            if (dist < bestDist)
            {
                bestCell = cell;
                bestDist = dist;
            }
            else if (dist == bestDist)
            {
                int currDist = Mathf.FloorToInt(Vector2.Distance(cell, neighbor));
                int bestDistToNeighbor = Mathf.FloorToInt(Vector2.Distance(bestCell, neighbor));

                if (currDist < bestDistToNeighbor)
                    bestCell = cell;
            }
        }

        hero.SetStartCell(Board.instance.GetCell((int)bestCell.x, (int)bestCell.y));
    }

    private void SetRangedPosition(Hero hero, Vector2 target, Vector2 neighbor)
    {
        List<Vector2> freeCells = GetFreeCellsForRanged()
            .Where(cell => Mathf.FloorToInt(Vector2.Distance(cell, target)) <= hero.AttackRange)
            .ToList();

        if (freeCells.Count == 0)
        {
            target = new Vector2(owner.Tag == GameTag.Human ? 4 : 3, target.y);
            GetFreeCellsForRanged()
            .Where(cell => Mathf.FloorToInt(Vector2.Distance(cell, target)) <= hero.AttackRange)
            .ToList();
        }

        Vector2 bestCell = freeCells[0];
        float bestDist = Mathf.FloorToInt(Vector2.Distance(bestCell, target));

        foreach (Vector2 cell in freeCells)
        {
            float dist = Mathf.FloorToInt(Vector2.Distance(cell, target));
            if (dist > bestDist && dist <= hero.AttackRange)
            {
                bestCell = cell;
                bestDist = dist;
            }
            else if (dist == bestDist)
            {
                float currDist = Mathf.FloorToInt(Vector2.Distance(cell, neighbor));
                float bestDistToNeighbor = Mathf.FloorToInt(Vector2.Distance(bestCell, neighbor));

                if (currDist > bestDistToNeighbor)
                    bestCell = cell;
            }
        }

        hero.SetStartCell(Board.instance.GetCell((int)bestCell.x, (int)bestCell.y));
    }

    private List<Vector2> GetFreeCellsForMelee()
    {
        int row = owner.Tag == GameTag.Human ? 3 : 4;
        List<Vector2> cells = new();

        for (int col = 0; col < 8; col++)
        {
            if (!Board.instance.GetCell(row, col).IsOccupied)
                cells.Add(new Vector2(row, col));

            if (cells.Count == 0 && col == 7)
            {
                col = 0;
                row += owner.Tag == GameTag.Human ? -1 : 1;
            }
        }

        return cells;
    }

    private List<Vector2> GetFreeCellsForRanged()
    {
        int lastRow = owner.Tag == GameTag.Human ? 0 : 5;
        int endRow = owner.Tag == GameTag.Human ? 2 : 7;
        List<Vector2> cells = new();

        for (int row = lastRow; row <= endRow; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (!Board.instance.GetCell(row, col).IsOccupied)
                    cells.Add(new Vector2(row, col));
            }
        }

        return cells;
    }

    private Vector2 GetMainDangerPoint()
    {
        int maxDanger = int.MinValue;
        List<Vector2> candidates = new();

        for (int row = (owner.Tag == GameTag.Human ? 4 : 0); row < (owner.Tag == GameTag.Human ? 7 : 3); row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int danger = dangerBoard[row, col];
                if (danger > maxDanger)
                {
                    maxDanger = danger;
                    candidates.Clear();
                    candidates.Add(new Vector2(row, col));
                }
                else if (danger == maxDanger)
                {
                    candidates.Add(new Vector2(row, col));
                }
            }
        }

        Vector2 target = candidates[0];
        int regionDanger = CountRegionDanger(target);

        foreach (var point in candidates)
        {
            int danger = CountRegionDanger(point);
            if (danger > regionDanger)
            {
                regionDanger = danger;
                target = point;
            }
        }

        return target;
    }

    private int CountRegionDanger(Vector2 point)
    {
        int regionDanger = 0;

        for (int i = -1; i <= 1; i++)
        {
            int row = (int)point.x + i;
            if (row < 0 || row > 7)
                continue;

            for (int j = -1; j <= 1; j++)
            {
                int col = (int)point.y + j;
                if (col < 0 || col > 7)
                    continue;

                regionDanger += dangerBoard[row, col];
            }
        }

        return regionDanger;
    }

    private Vector2 GetMostDangerousNeighbor(Vector2 point)
    {
        Vector2 mostDangerous = point;
        int maxDanger = -1;

        for (int i = -1; i <= 1; i++)
        {
            int row = (int)point.x + i;
            if (row < 0 || row > 7)
                continue;

            for (int j = -1; j <= 1; j++)
            {
                int col = (int)point.y + j;
                if (col < 0 || col > 7)
                    continue;

                int danger = dangerBoard[row, col];
                if (danger > maxDanger)
                {
                    maxDanger = danger;
                    mostDangerous = new Vector2(row, col);
                }
            }
        }

        return mostDangerous;
    }
}
