using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AutoCheckers;

public class HeroCard : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private AbilityIcon heroClassSection;
    [SerializeField]
    private AbilityIcon raceSection;
    [SerializeField]
    private TMP_Text costText;
    [SerializeField]
    private Button buyButton;

    [Header("Шаблон Героя")]
    [SerializeField]
    private GameObject heroPrefab;

    private GameObject heroRender;

    public GameObject HeroPrefab
    {
        get { return heroPrefab; }
    }

    public void SetHeroRender(GameObject heroRender)
    {
        this.heroRender = heroRender;
    }

    void Start()
    {
        costText.text = heroPrefab.GetComponent<Hero>().Cost.ToString();
        HeroClass heroClass = heroPrefab.GetComponent<Hero>().HeroClass;
        Race race = heroPrefab.GetComponent<Hero>().Race;

        heroClassSection.SetAbility(heroClass, heroClass.ToString());
        raceSection.SetAbility(race, race.ToString());

        buyButton.onClick.AddListener(TryToHumanPurchase);
    }

    public void TryToHumanPurchase()
    {
        int cost = heroPrefab.GetComponent<Hero>().Cost;

        if (GameManager.instance.Human.Money >= cost)
            PurchaceHero(GameTag.Human);
        else
            OnHumanPurchseFailed();
    }

    public void PurchaceHero(GameTag tag)
    {
        int cost = heroPrefab.GetComponent<Hero>().Cost;

        Player player = (tag == GameTag.Human) ? GameManager.instance.Human : GameManager.instance.AI;

        player.Purchase(cost);

        GameObject hero = Instantiate(heroPrefab);
        hero.name = heroPrefab.name;

        Board.instance.SetHero(hero, tag);
        Shop.instance.RemoveFromShop(tag, this.gameObject, heroRender);
    }

    private void OnHumanPurchseFailed()
    {
        buyButton.image.color = Color.red;
        buyButton.GetComponentInChildren<TMP_Text>().text = "Мало монет";

        StartCoroutine(BuyButtonReset());
    }

    private IEnumerator BuyButtonReset()
    {
        yield return new WaitForSeconds(1);

        buyButton.image.color = Color.gray;
        buyButton.GetComponentInChildren<TMP_Text>().text = "Купить";
    }
}
