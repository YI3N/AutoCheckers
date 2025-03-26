using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    [Header("Параметры")]
    [SerializeField]
    private int row;
    [SerializeField]
    private int col;
    [SerializeField]
    private bool isBench = false;

    private Vector3 worldPosition;
    public Hero OccupiedHero { get; private set; }

    public bool IsOccupied { get; private set; } = false;
    public int Row
    {
        get { return row; }
    }
    public int Col
    {
        get { return col; }
    }
    public bool IsBench
    {
        get { return isBench; }
    }

    public Vector2 GetBoardPosition()
    {
        return new Vector2(row, col);
    }

    public Vector3 GetWorldPosition()
    {
        return worldPosition;
    }

    public void Occupy(Hero hero)
    {
        hero.transform.position = worldPosition;
        OccupiedHero = hero;
        IsOccupied = true;
    }

    public void Deoccupy()
    {
        OccupiedHero = null;
        IsOccupied = false;
    }

    private void Awake()
    {
        worldPosition = transform.position;
    }
}
