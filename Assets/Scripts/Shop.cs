using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using AutoCheckers;
using TMPro;

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
    private List<GameObject> renderPoints;
    [SerializeField]
    private List<RenderTexture> materials;

    private int rerollCost = 2;
    private int expCost = 4;

    private List<GameObject> humanShop = new List<GameObject>();
    private List<GameObject> renders = new List<GameObject>();
    private List<GameObject> AIShop = new List<GameObject>();

    private float[,] chances = {
        {1,     0,      0,      0,      0 },
        {.7f,   .3f,    0,      0,      0},
        {.6f,   .35f,   .05f,   0,      0},
        {.5f,   .35f,   .15f,   0,      0},
        {.4f,   .35f,   .23f,   .02f,   0},
        {.33f,  .3f,    .3f,    .07f,   0},
        {.3f,   .3f,    .3f,    .1f,    0},
        {.24f,  .3f,    .3f,    .15f,   .01f},
        {.22f,  .25f,   .3f,    .2f,    .03f},
        {.19f,  .25f,   .25f,   .25f,   .06f}
    };

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
        GenerateHumanShop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddHeroToPull(int id, int upgrades, int rarity)
    {
        List<GameObject> cardList = null;
        if (rarity == 0)
            cardList = commonCards;
        else if (rarity == 1)
            cardList = uncommonCards;
        else if (rarity == 2)
            cardList = rareCards;
        else if (rarity == 3)
            cardList = mythicCards;
        else if (rarity == 4)
            cardList = legendaryCards;

        GameObject heroCard = null;
        foreach (GameObject card in allCards)
        {
            if (card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>().ID == id)
            {
                heroCard = card;
                break;
            }
        }

        for (int i = 0; i <= upgrades; i++)
            cardList.Add(heroCard);
    }

    public void SellHero(Hero hero, Player player)
    {
        player.GainMoney(hero.Level);
        AddHeroToPull(hero.ID, hero.Level, (int)hero.Rarity);
        DestroyImmediate(hero.gameObject);
    }

    public void GenerateHumanShop()
    {
        for (int i = 0; i < renderPoints.Count; i++)
        {
            if (renderPoints[i].transform.childCount < 2)
            {
                float chance = Random.Range(0f, 1f);
                int playerLevel = GameManager.instance.Human.Level;
                List<GameObject> randomList = null;

                if (chance <= chances[playerLevel, 4] && legendaryCards.Count != 0)
                    randomList = legendaryCards;
                else if (chance <= chances[playerLevel, 3] && mythicCards.Count != 0)
                    randomList = mythicCards;
                else if (chance <= chances[playerLevel, 2] && rareCards.Count != 0)
                    randomList = rareCards;
                else if (chance <= chances[playerLevel, 1] && uncommonCards.Count != 0)
                    randomList = uncommonCards;
                else if (commonCards.Count != 0)
                    randomList = commonCards;
                else if (uncommonCards.Count != 0)
                    randomList = uncommonCards;
                else if (rareCards.Count != 0)
                    randomList = rareCards;
                else if (mythicCards.Count != 0)
                    randomList = mythicCards;
                else if (legendaryCards.Count != 0)
                    randomList = legendaryCards;
                else
                    break;

                int randomNumber = Random.Range(0, randomList.Count);

                GameObject heroCard = Instantiate(randomList[randomNumber], gameObject.transform);
                randomList.RemoveAt(randomNumber);

                GameObject heroRender = Instantiate(heroCard.GetComponent<HeroCard>().HeroPrefab, renderPoints[i].transform, true);
                heroRender.transform.position = renderPoints[i].transform.position;
                heroRender.layer = 6;
                heroRender.transform.Find("HeroPlane").gameObject.layer = 6;
                Destroy(heroRender.GetComponent<Hero>());

                heroCard.GetComponent<HeroCard>().SetHeroRender(heroRender);
                heroCard.transform.Find("HeroRender").GetComponent<RawImage>().texture = materials[i];
                heroCard.GetComponent<Image>().color = heroRender.GetComponent<MeshRenderer>().material.color;

                humanShop.Add(heroCard);
                renders.Add(heroRender);
            }
        }
    }

    public void RemoveFromHumanShop(GameObject card, GameObject render)
    {
        humanShop.Remove(card);
        renders.Remove(render);

        Destroy(card);
        Destroy(render);
    }

    private void ClearHumanShop()
    {
        foreach (GameObject card in humanShop)
        {
            AddHeroToPull(card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>().ID,
                card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>().Upgrades,
                (int)card.GetComponent<HeroCard>().HeroPrefab.GetComponent<Hero>().Rarity);
            DestroyImmediate(card);
        }
        humanShop.Clear();

        foreach (GameObject render in renders)
        {
            DestroyImmediate(render);
        }
        renders.Clear();
    }

    public void TryToRerollHumanShop()
    {
        if (GameManager.instance.Human.Money >= rerollCost)
        {
            GameManager.instance.Human.Purchase(rerollCost);
            ClearHumanShop();
            GenerateHumanShop();
        }
        else
        {
            UIManager.instance.OnRerollFailed();
        }
    }

    public void TryToEXPHumanShop()
    {
        if (GameManager.instance.Human.Money >= expCost)
        {
            GameManager.instance.Human.Purchase(expCost);
            GameManager.instance.Human.GainEXP(expCost);
        }
        else
        {
            UIManager.instance.OnEXPFailed();
        }
    }
}
