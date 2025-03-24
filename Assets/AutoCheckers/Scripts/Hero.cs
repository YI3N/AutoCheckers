using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AutoCheckers;
using UnityEngine.UI;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class Hero : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private Slider manaBar;
    [SerializeField]
    private GameObject upgradeUI;
    [SerializeField]
    private GameObject silenceIcon;

    [Header("Данные")]
    [SerializeField]
    private int id;
    [SerializeField]
    private int cost;
    [SerializeField]
    private Rarity rarity;
    [SerializeField]
    private HeroClass heroClass;
    [SerializeField]
    private Race race;

    [Header("Характеристики")]
    [SerializeField]
    private List<int> health;
    [SerializeField]
    private int healthRegeneration;
    [SerializeField]
    private List<int> minDamage;
    [SerializeField]
    private List<int> maxDamage;
    [SerializeField]
    private int mana;
    [SerializeField]
    private int manaRegeneration;
    [SerializeField]
    private int armor;
    [SerializeField]
    private int magicalResistance;
    [SerializeField]
    private int attackRange;
    [SerializeField]
    private int moveDistance = 2;

    public int ID
    {
        get { return id; }
    }
    public int Cost
    {
        get { return cost; }
    }
    public Rarity Rarity
    {
        get { return rarity; }
    }
    public HeroClass HeroClass
    {
        get { return heroClass; }
    }
    public Race Race
    {
        get { return race; } 
    }

    public int Upgrades{ get; private set; } = 0;
    public int MaxHealth
    {
        get { return health[Upgrades] + bonusHealth; }
    }
    public int CurrentHealth { get; private set; }
    public int MaxMana
    {
        get { return mana; }
    }
    public int CurrentMana { get; private set; }
    public int Damage
    {
        get { return Random.Range(minDamage[Upgrades], maxDamage[Upgrades]); }
    }
    public int Armor
    {
        get { return armor + bonusArmor; }
    }
    public float PhysicalReduction
    {
        get { return 1 - (0.052f * Armor / (0.9f + 0.048f * Armor));  }
    }
    public float MagicalResistance
    {
        get { return 1 - (magicalResistance + bonusMagicalResistance) /100;  }
    }
    public int Level
    {
        get { return Cost + Upgrades * 2; }
    }

    public IAbility ClassAbility { get; private set; }
    public IAbility RaceAbility { get; private set; }
    public Hero TargetEnemy { get; private set; }
    public BoardCell CurrentCell { get; private set; }
    public BoardCell StartCell { get; private set; }

    public Player Owner { get; private set; }
    public Player Opponent { get; private set; }

    public int AttackStatistics { get; private set; }
    public int DefenceStatistics { get; private set; }

    private ISpell spell;
    private List<Action> onAttackEvents = new List<Action>();
    private List<Action> onHitEvents = new List<Action>();

    private int bonusHealth = 0;
    private int bonusArmor = 0;
    private int bonusMagicalResistance = 0;
    private bool isSilent = false;

    private int damageDealt = 0;
    private int damageTook = 0;

    public void SetFriendsAndFoes(GameTag tag)
    {
        Owner = (tag == GameTag.Human) ? GameManager.instance.Human : GameManager.instance.AI;
        Opponent = (tag == GameTag.Human) ? GameManager.instance.AI : GameManager.instance.Human;

        gameObject.tag = Owner.Tag.ToString();
    }

    void Awake()
    {
        CurrentHealth = MaxHealth;
        spell = GetComponent<ISpell>();

        IAbility[] abilities = GetComponents<IAbility>();

        if (abilities.Length >= 2)
        {
            ClassAbility = abilities[0];
            RaceAbility = abilities[1];
        }
    }
    
    void Start()
    {
        SnapToCell(StartCell);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetHero()
    {
        CurrentCell.Deoccupy();
        CurrentCell = StartCell;
        CurrentCell.Occupy(this);

        damageDealt = 0;
        damageTook = 0;

        bonusArmor = 0;
        bonusHealth = 0;
        bonusMagicalResistance = 0;

        isSilent = false;
        silenceIcon.SetActive(false);

        spell.ResetAbility();

        TargetEnemy = null;

        CurrentHealth = MaxHealth;
        CurrentMana = 0;

        healthBar.value = CurrentHealth / (float)MaxHealth;
        manaBar.value = CurrentMana / (float)MaxMana;
    }    

    public void SetStartCell(BoardCell cell)
    {
        if (StartCell != null)
            if (StartCell.tag == nameof(GameTag.HumanBench) || StartCell.tag == nameof(GameTag.AIBench) && cell.tag == nameof(GameTag.Board))
            {
                StartCell.Deoccupy();
                Owner.MoveHeroToBoard(gameObject);
            }
            else if (StartCell.tag == nameof(GameTag.Board) && (cell.tag == nameof(GameTag.HumanBench) || cell.tag == nameof(GameTag.AIBench)))
            {
                StartCell.Deoccupy();
                Owner.MoveHeroToBench(gameObject);
            }
            else
            {
                StartCell.Deoccupy();
            }

        StartCell = cell;
        CurrentCell = StartCell;

        StartCell.Occupy(this);
    }

    private void SnapToCell(BoardCell cell)
    {
        transform.position = cell.GetWorldPosition();
    }

    public void GainMaxHealth(int health)
    {
        bonusHealth += health;
        CurrentHealth += bonusHealth;

        healthBar.value = CurrentHealth / (float)MaxHealth;
    }

    public void GainMana(int mana)
    {
        CurrentMana += Mathf.Clamp(mana, 0, 50) / 5;
        CurrentMana = Mathf.Clamp(CurrentMana, 0, 100);

        manaBar.value = CurrentMana / (float)MaxMana;
    }

    public void GainArmor(int armor)
    {
        bonusArmor += armor;
    }

    public void GainMagicalResistance(int magicalResistance)
    {
        bonusMagicalResistance += magicalResistance;
    }

    public void GainOnAttackEvent(Action func)
    {
        onAttackEvents.Add(func);
    }

    public void RemoveOnAttackEvent(Action func)
    {
        onAttackEvents.Remove(func);
    }

    public void GetSilenced(int seconds)
    {
        if (!isSilent)
        {
            isSilent = true;
            silenceIcon.SetActive(true);
            StartCoroutine(RemoveSilence(seconds));
        }
    }

    private IEnumerator RemoveSilence(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        isSilent = false;
        silenceIcon.SetActive(false);
    }

    public void SetTargetEnemy(Hero hero)
    {
        TargetEnemy = hero;
    }

    public void TakeAction()
    {
        CurrentHealth += healthRegeneration;
        CurrentMana += manaRegeneration;

        spell.PassiveSpell();

        if (TargetEnemy != null && !TargetEnemy.gameObject.activeSelf)
            TargetEnemy = null;

        if (CanCastSpell())
        {
            CurrentMana = 0;
            spell.CastSpell();
        }
        else if (CanAttackTarget())
            Attack();
        else
            ChooseTarget();
    }

    private bool CanCastSpell()
    {
        return spell.CanCast() && CurrentMana >= 100 && !isSilent;
    }

    private bool CanAttackTarget()
    {
        return TargetEnemy != null &&
               Mathf.FloorToInt(Vector2.Distance(CurrentCell.GetBoardPosition(), TargetEnemy.CurrentCell.GetBoardPosition())) <= attackRange;
    }

    private void Attack()
    {
        damageDealt = TargetEnemy.OnHit(Damage, GameTag.Physical);
        AttackStatistics += damageDealt;

        GainMana(Mathf.Clamp(damageDealt, 0, 10));

        TriggerOnAttackEvents();
    }

    private void TriggerOnAttackEvents()
    {
        foreach (var func in onAttackEvents)
        {
            func.Invoke();
        }
    }

    public void TakeDamage()
    {
        if (damageTook > 0)
        {
            CurrentHealth -= damageTook;
            DefenceStatistics += damageTook;

            if (CurrentHealth <= 0)
            {
                Owner.HeroDied();
                gameObject.SetActive(false);
            }

            GainMana(damageTook);
            damageTook = 0;
            healthBar.value = CurrentHealth / (float)MaxHealth;
        }
    }

    public int OnHit(int damage, GameTag attackType)
    {
        damage = ApplyDamageReduction(damage, attackType);

        damageTook += damage;
        return damage;
    }

    private int ApplyDamageReduction(int damage, GameTag attackType)
    {
        switch (attackType)
        {
            case GameTag.Physical:
                return Mathf.RoundToInt(damage * PhysicalReduction);
            case GameTag.Magical:
                return Mathf.RoundToInt(damage * MagicalResistance);
            default:
                return damage;
        }
    }

    private void ChooseTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(Opponent.Tag.ToString());
        float nearestDistance = Mathf.Infinity;
        List<BoardCell> possibleMoves = new List<BoardCell>();

        foreach (GameObject enemyObject in enemies)
        {
            Hero enemy = enemyObject.GetComponent<Hero>();

            if (enemy.StartCell.IsBench == true)
                continue;
            if (enemy.gameObject.activeSelf == false)
                continue;

            float distance = Mathf.FloorToInt(Vector2.Distance(CurrentCell.GetBoardPosition(), enemy.CurrentCell.GetBoardPosition()));
            if (distance < nearestDistance)
            {
                possibleMoves = Board.instance.GetAvailableCells(enemy.CurrentCell, CurrentCell, moveDistance, attackRange);
                TargetEnemy = enemy;
                nearestDistance = distance;
            }
        }

        if (possibleMoves.Count != 0)
            ChooseMoveLocation(possibleMoves);
    }

    private void ChooseMoveLocation(List<BoardCell> possibleMoves)
    {
        BoardCell bestMove = null;
        float bestDistance = Mathf.Infinity;
        bool canAttack = false;

        foreach (BoardCell possibleMove in possibleMoves)
        {
            float attackDistance = Mathf.FloorToInt(Vector2.Distance(possibleMove.GetBoardPosition(), TargetEnemy.CurrentCell.GetBoardPosition()));
            float moveDistance = Mathf.FloorToInt(Vector2.Distance(CurrentCell.GetBoardPosition(), possibleMove.GetBoardPosition()));

            if (!canAttack && attackDistance <= attackRange)
            {
                canAttack = true;
                bestDistance = Mathf.Infinity;
            }

            if (canAttack && attackDistance <= attackRange && moveDistance < bestDistance)
            {
                bestDistance = moveDistance;
                bestMove = possibleMove;
            }
            else if (!canAttack && attackDistance < bestDistance)
            {
                bestDistance = attackDistance;
                bestMove = possibleMove;
            }
        }

        if (bestMove != null)
            Move(bestMove);
    }

    private void Move(BoardCell moveTarget)
    {
        if (CurrentCell != moveTarget)
        {
            CurrentCell.Deoccupy();
            CurrentCell = moveTarget;
            CurrentCell.Occupy(this);
        }
    }

    public void CheckHeroUpgrade()
    {
        if (Upgrades < 3)
        {
            List<GameObject> heroes = Owner.HeroesOnBoard.Concat(Owner.HeroesOnBench).ToList();
            List<GameObject> upgradeHeroes = new List<GameObject>();

            foreach (GameObject hero in heroes)
            {
                if (ID == hero.GetComponent<Hero>().ID && Upgrades == hero.GetComponent<Hero>().Upgrades)
                {
                    upgradeHeroes.Add(hero);
                    if (upgradeHeroes.Count >= 3)
                        break;
                }
            }

            if (upgradeHeroes.Count >= 3)
            {
                upgradeHeroes[0].GetComponent<Hero>().Combine();
                DestroyImmediate(upgradeHeroes[1]);
                DestroyImmediate(upgradeHeroes[2]);
            }

            Owner.HeroesOnBoard.RemoveAll(item => item == null);
            Owner.HeroesOnBench.RemoveAll(item => item == null);
        }
    }

    private void Combine()
    {
        Upgrades++;
        CurrentHealth = MaxHealth;

        int i = 0;
        foreach (Transform child in upgradeUI.transform)
        {
            if (i <= Upgrades)
                child.gameObject.SetActive(true);
            i++;
        }

        CheckHeroUpgrade();
    }

    private void OnDestroy()
    {
        if (StartCell != null)
            StartCell.Deoccupy();
    }
}
