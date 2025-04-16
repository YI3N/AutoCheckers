using AutoCheckers;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop instance = null;

    [Header("Герои")]
    [SerializeField]
    private List<GameObject> allCards;
    [SerializeField]
    private List<GameObject> commonCards;
    [SerializeField]
    private List<GameObject> uncommonCards;
    [SerializeField]
    private List<GameObject> rareCards;
    [SerializeField]
    private List<GameObject> mythicCards;
    [SerializeField]
    private List<GameObject> legendaryCards;

    [Space(10)]
    [SerializeField]
    private List<GameObject> humanRenderPoints;
    [SerializeField]
    private List<RenderTexture> humanMaterials;
    [SerializeField]
    private ShopChances humanChances;
    [SerializeField]
    private LockButton lockButton;

    [Space(10)]
    [SerializeField]
    private GameObject AIContent;
    [SerializeField]
    private List<GameObject> AIRenderPoints;
    [SerializeField]
    private List<RenderTexture> AIMaterials;
    [SerializeField]
    private ShopChances aiChances;

    public int RerollCost { get; private set; } = 2;
    public int EXPCost { get; private set; } = 5;
    private int expGain = 4;

    private List<GameObject> humanShop = new List<GameObject>();
    private List<GameObject> humanRenders = new List<GameObject>();
    private List<GameObject> AIShop = new List<GameObject>();
    private List<GameObject> AIRenders = new List<GameObject>();

    public bool HumanLocked { get; private set; } = false;
    public bool AILocked { get; private set; } = false;

    private readonly List<float[]> chances = new()
    {
        new float[] { 1,    0,      0,      0,      0 },
        new float[] { .7f,  .3f,    0,      0,      0 },
        new float[] { .6f,  .35f,   .05f,   0,      0 },
        new float[] { .5f,  .35f,   .15f,   0,      0 },
        new float[] { .4f,  .35f,   .23f,   .02f,   0 },
        new float[] { .33f, .3f,    .3f,    .07f,   0 },
        new float[] { .3f,  .3f,    .3f,    .1f,    0 },
        new float[] { .24f, .3f,    .3f,    .15f,   .01f },
        new float[] { .22f, .25f,   .3f,    .2f,    .03f },
        new float[] { .19f, .25f,   .25f,   .25f,   .06f }
    };

    private readonly int commonAmount = 45;
    private readonly int uncommonAmount = 30;
    private readonly int rareAmount = 25;
    private readonly int mythicAmount = 15;
    private readonly int legendaryAmount = 10;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance == this)
            Destroy(this);

        GenerateShop(GameTag.Human);
        GenerateShop(GameTag.AI);
        UpdateChances(GameManager.instance.Human);
        UpdateChances(GameManager.instance.AI);
    }
    void Start()
    {

    }

    public void UpdateChances(Player player)
    {
        if (player.Tag == GameTag.Human)
            humanChances.SetChances(chances[player.Level]);
        else if (player.Tag == GameTag.AI)
            aiChances.SetChances(chances[player.Level]);
    }

    private void AddHeroToPull(Hero hero)
    {
        List<GameObject> cardList = GetHeroRarityList(hero.Rarity);

        GameObject heroCard = null;
        foreach (GameObject card in allCards)
        {
            if (card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>().ID == hero.ID)
            {
                heroCard = card;
                break;
            }
        }

        for (int i = 0; i < (int)Mathf.Pow(hero.CombineThreshold, hero.Upgrades); i++)
            cardList.Add(heroCard);
    }

    public void SellHero(Hero hero, Player player)
    {
        player.GainMoney(hero.Level);
        AddHeroToPull(hero);
        Destroy(hero.gameObject);
    }

    private List<GameObject> GetRandomHeroList(int playerLevel)
    {
        float chance = Random.Range(0f, 1f);

        if (chance <= chances[playerLevel][4] && legendaryCards.Count != 0)
            return legendaryCards;
        else if (chance <= chances[playerLevel][3] && mythicCards.Count != 0)
            return mythicCards;
        else if (chance <= chances[playerLevel][2] && rareCards.Count != 0)
            return rareCards;
        else if (chance <= chances[playerLevel][1] && uncommonCards.Count != 0)
            return uncommonCards;
        else if (commonCards.Count != 0)
            return commonCards;
        else if (uncommonCards.Count != 0)
            return uncommonCards;
        else if (rareCards.Count != 0)
            return rareCards;
        else if (mythicCards.Count != 0)
            return mythicCards;
        else if (legendaryCards.Count != 0)
            return legendaryCards;
        else
            return null;
    }

    private (Player player, GameObject content, List<GameObject> shop, bool locked,
        List<GameObject> renderPoints, List<GameObject> renders, List<RenderTexture> materials) GetShopData(GameTag tag)
    {
        if (tag == GameTag.Human)
            return (GameManager.instance.Human, gameObject, humanShop, HumanLocked, humanRenderPoints, humanRenders, humanMaterials);
        else
            return (GameManager.instance.AI, AIContent, AIShop, AILocked, AIRenderPoints, AIRenders, AIMaterials);
    }

    public List<GameObject> GetCurrentHeroShop(GameTag tag)
    {
        List<GameObject> heroes = new List<GameObject>();
        if (tag == GameTag.Human)
        {
            foreach (GameObject card in humanShop)
            {
                GameObject piece = card.GetComponent<HeroCard>().HeroPrefab;
                heroes.Add(piece);
            }
        }
        else
        {
            foreach (GameObject card in AIShop)
            {
                GameObject piece = card.GetComponent<HeroCard>().HeroPrefab;
                heroes.Add(piece);
            }
        }

        return heroes;
    }

    public List<GameObject> GetCurrentCardShop(GameTag tag)
    {
        if (tag == GameTag.Human)
            return humanShop;
        else
            return AIShop;
    }

    public void LockShop(GameTag tag)
    {
        if (tag == GameTag.Human)
        {
            HumanLocked = !HumanLocked;
            lockButton.ChangeButton();
        }
        else
            AILocked = !AILocked;
    }

    private void GenerateShop(GameTag tag)
    {
        var (player, content, shop, locked, renderPoints, renders, materials) = GetShopData(tag);

        if (locked)
        {
            locked = false;
            lockButton.ChangeButton();
            return;
        }

        for (int i = 0; i < renderPoints.Count; i++)
        {
            if (renderPoints[i].transform.childCount < 2)
            {
                List<GameObject> randomList = GetRandomHeroList(player.Level);
                if (randomList == null)
                    break;

                int randomNumber = Random.Range(0, randomList.Count);

                GameObject heroCard = Instantiate(randomList[randomNumber], content.transform);
                randomList.RemoveAt(randomNumber);

                GameObject heroRender = Instantiate(heroCard.GetComponent<HeroCard>().HeroPrefab, renderPoints[i].transform, true);
                heroRender.transform.position = renderPoints[i].transform.position;
                heroRender.layer = 6;
                heroRender.transform.Find("HeroPlane").gameObject.layer = 6;
                Destroy(heroRender.GetComponent<Hero>());

                heroCard.GetComponent<HeroCard>().SetHeroRender(heroRender);
                heroCard.transform.Find("HeroRender").GetComponent<RawImage>().texture = materials[i];
                heroCard.GetComponent<Image>().color = heroRender.GetComponent<MeshRenderer>().material.color;

                if (tag == GameTag.AI)
                    Destroy(heroCard.transform.Find("BuyButton").gameObject);

                shop.Add(heroCard);
                renders.Add(heroRender);
            }
        }
    }

    public void RemoveFromShop(GameTag tag, GameObject card, GameObject render)
    {
        var (_, _, shop, _, _, renders, _) = GetShopData(tag);

        shop.Remove(card);
        renders.Remove(render);

        Destroy(card);
        Destroy(render);
    }

    private void ClearShop(GameTag tag)
    {
        var (_, _, shop, _, _, renders, _) = GetShopData(tag);

        foreach (GameObject card in shop)
        {
            AddHeroToPull(card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>());
            DestroyImmediate(card);
        }
        shop.Clear();

        foreach (GameObject render in renders)
        {
            DestroyImmediate(render);
        }
        renders.Clear();
    }

    public void TryToRerollHumanShop()
    {
        if (GameManager.instance.Human.Money >= RerollCost)
            RerollShop(GameTag.Human, true);
        else
            UIManager.instance.OnRerollFailed();
    }

    public void RerollShop(GameTag tag, bool isPaid)
    {
        var (player, _, _, locked, _, _, _) = GetShopData(tag);

        if (locked)
            return;

        if (isPaid)
            player.Purchase(RerollCost);

        ClearShop(tag);
        GenerateShop(tag);
    }

    public void TryToBuyEXPHumanShop()
    {
        if (GameManager.instance.Human.Money >= EXPCost)
            BuyEXP(GameTag.Human);
        else
            UIManager.instance.OnEXPFailed();
    }

    public void BuyEXP(GameTag tag)
    {
        var (player, _, _, _, _, _, _) = GetShopData(tag);
        player.Purchase(EXPCost);
        player.GainEXP(expGain);
    }

    public float GetCurrentRaritySpawnChance(int level, Rarity rarity)
    {
        return chances[level][((int)rarity)];
    }

    public List<GameObject> GetHeroRarityList(Rarity rarity)
    {
        if ((int)rarity == 0)
            return commonCards;
        else if ((int)rarity == 1)
            return uncommonCards;
        else if ((int)rarity == 2)
            return rareCards;
        else if ((int)rarity == 3)
            return mythicCards;
        else if ((int)rarity == 4)
            return legendaryCards;
        else
            return null;
    }
}
