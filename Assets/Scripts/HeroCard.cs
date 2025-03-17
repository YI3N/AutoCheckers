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
    private TMP_Text classText;
    [SerializeField]
    private TMP_Text raceText;
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
        classText.text = heroPrefab.GetComponent<Hero>().HeroClass.ToString();
        raceText.text = heroPrefab.GetComponent<Hero>().Race.ToString();
        costText.text = heroPrefab.GetComponent<Hero>().Cost.ToString();

        buyButton.onClick.AddListener(TryToHumanPurchase);
    }

    void Update()
    {
        
    }

    public void TryToHumanPurchase()
    {
        int cost = heroPrefab.GetComponent<Hero>().Cost;

        if (GameManager.instance.Human.Money >= cost)
        {
            GameManager.instance.Human.Purchase(cost);
            Board.instance.SetHumanHero(Instantiate(heroPrefab));
            Shop.instance.RemoveFromHumanShop(this.gameObject, heroRender);
        }
        else
        {
            OnPurchseFailed();
        }
    }

    private void OnPurchseFailed()
    {
        buyButton.image.color = Color.red;
        buyButton.GetComponentInChildren<TMP_Text>().text = "Мало монет";

        StartCoroutine(ButtonReset());
    }

    private IEnumerator ButtonReset()
    {
        yield return new WaitForSeconds(1);

        buyButton.image.color = Color.gray;
        buyButton.GetComponentInChildren<TMP_Text>().text = "Купить";
    }
}
