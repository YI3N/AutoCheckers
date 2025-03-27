using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AutoCheckers;
using System.Linq;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    [SerializeField]
    private GameObject menuUI;
    [SerializeField]
    private GameObject pauseUI;
    [SerializeField]
    private GameObject playersUI;
    [SerializeField]
    private GameObject shopUI;

    [SerializeField]
    private GameObject sellPlane;

    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button shopOpenButton;
    [SerializeField]
    private Button shopCloseButton;
    [SerializeField]
    private Button shopRerollButton;
    [SerializeField]
    private Button shopEXPButton;

    [SerializeField]
    private GameObject AIZone;
    [SerializeField]
    private TMP_Text round;
    [SerializeField]
    private TMP_Text sellPrice;

    [SerializeField]
    private PlayerPanel humanPanel;
    [SerializeField]
    private PlayerPanel aiPanel;

    [SerializeField]
    private StatisticsPanel statisticsPanel;

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
        menuUI.SetActive(true);
        pauseUI.SetActive(true);
        playersUI.SetActive(true);
        shopUI.SetActive(true);
        statisticsPanel.SetActive(true);

        playButton.onClick.AddListener(BeginGame);
        startButton.onClick.AddListener(StartRound);

        shopOpenButton.onClick.AddListener(OpenShop);
        shopCloseButton.onClick.AddListener(CloseShop);
        shopRerollButton.onClick.AddListener(Shop.instance.TryToRerollHumanShop);
        shopEXPButton.onClick.AddListener(Shop.instance.TryToBuyEXPHumanShop);

        pauseUI.SetActive(false);
        playersUI.SetActive(false);
        shopUI.SetActive(false);
        statisticsPanel.SetActive(false);
    }

    public void UpdatePlayerUI(Player player)
    {
        if (player.Tag == GameTag.Human)
            humanPanel.UpdatePlayerPanel(player);
        else if (player.Tag == GameTag.AI)
            aiPanel.UpdatePlayerPanel(player);
    }

    public void SetStatistics(Player player)
    {
        statisticsPanel.SetPlayerStatistics(player);
    }

    private void BeginGame()
    {
        menuUI.SetActive(false);
        playersUI.SetActive(true);
        statisticsPanel.SetActive(true);

        UpdatePlayerUI(GameManager.instance.Human);
        UpdatePlayerUI(GameManager.instance.AI);

        humanPanel.SetPlayerName("Игрок");
        aiPanel.SetPlayerName("ИИ");

        OpenShop();
        GameManager.instance.BeginGame();
    }

    private void StartRound()
    {
        AIZone.SetActive(false);
        pauseUI.SetActive(false);
        statisticsPanel.SetActive(false);

        GameManager.instance.StartRound();
    }

    public void EndRound()
    {
        round.text = "Раунд - " + GameManager.instance.Round;
        pauseUI.SetActive(true);
        AIZone.SetActive(true);
        statisticsPanel.SetActive(true);
    }

    private void OpenShop()
    {
        shopUI.SetActive(true);
        pauseUI.SetActive(false);
        sellPlane.SetActive(false);
        statisticsPanel.SetActive(false);
        round.gameObject.SetActive(false);
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        pauseUI.SetActive(true);
        sellPlane.SetActive(true);
        statisticsPanel.SetActive(true);
        round.gameObject.SetActive(true);
    }

    public void ShowSellPrice(int price)
    {
        sellPrice.text = price.ToString();
        sellPrice.gameObject.SetActive(true);
    }

    public void HideSellPrice()
    {
        sellPrice.gameObject.SetActive(false);
    }

    public void OnRerollFailed()
    {
        shopRerollButton.image.color = Color.red;
        shopRerollButton.GetComponentInChildren<TMP_Text>().text = "Мало монет";

        StartCoroutine(RerollReset());
    }

    private IEnumerator RerollReset()
    {
        yield return new WaitForSeconds(1);

        shopRerollButton.image.color = Color.gray;
        shopRerollButton.GetComponentInChildren<TMP_Text>().text = "Обновить\n(2 монеты)";
    }

    public void OnEXPFailed()
    {
        shopEXPButton.image.color = Color.red;
        shopEXPButton.GetComponentInChildren<TMP_Text>().text = "Мало монет";

        StartCoroutine(EXPReset());
    }

    private IEnumerator EXPReset()
    {
        yield return new WaitForSeconds(1);

        shopEXPButton.image.color = Color.gray;
        shopEXPButton.GetComponentInChildren<TMP_Text>().text = "Купить 4 опыта\n(4 монеты)";
    }
}
