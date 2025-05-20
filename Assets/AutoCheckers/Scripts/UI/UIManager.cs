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
using System.Reflection;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    [SerializeField]
    private GameObject menuUI;
    [SerializeField]
    private GameObject resultsUI;
    [SerializeField]
    private GameObject pauseUI;
    [SerializeField]
    private GameObject playersUI;
    [SerializeField]
    private GameObject shopUI;

    [SerializeField]
    private TMP_Dropdown player1;
    [SerializeField]
    private TMP_Dropdown player2;

    [SerializeField]
    private Button menuButton;
    [SerializeField]
    private TMP_Text result;

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
    private Slider timerBar;
    [SerializeField]
    private TMP_Text timer;
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
        resultsUI.SetActive(true);

        playButton.onClick.AddListener(BeginGame);
        //startButton.onClick.AddListener(StartRound);

        shopOpenButton.onClick.AddListener(OpenShop);
        shopCloseButton.onClick.AddListener(CloseShop);
        shopRerollButton.onClick.AddListener(Shop.instance.TryToRerollHumanShop);
        shopEXPButton.onClick.AddListener(Shop.instance.TryToBuyEXPHumanShop);
        menuButton.onClick.AddListener(GameManager.instance.EndGame);

        pauseUI.SetActive(false);
        playersUI.SetActive(false);
        shopUI.SetActive(false);
        statisticsPanel.SetActive(false);
        resultsUI.SetActive(false);
    }

    public void UpdateTimer(float time)
    {
        timer.text = time.ToString("0");
        timerBar.value = time / GameManager.instance.pauseDelay;
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

        int index1 = player1.value;
        int index2 = player2.value;

        string name1 = player1.options[index1].text;
        string name2 = player2.options[index2].text;

        if (name1 == name2)
        {
            name1 += " 1";
            name2 += " 2";
        }

        humanPanel.SetPlayerName(name1);
        aiPanel.SetPlayerName(name2);

        OpenShop();
        GameManager.instance.BeginGame(player1.options[index1].text, player2.options[index2].text);
    }

    public void ShowResult(GameTag looser)
    {
        if (looser == GameTag.Human)
            result.text = aiPanel.GetPlayerName() + " победил!";
        else
            result.text = humanPanel.GetPlayerName() + " победил!";

        playersUI.SetActive(false);
        shopUI.SetActive(false);
        pauseUI.SetActive(false);

        resultsUI.SetActive(true);
    }

    public void StartRound()
    {
        CloseShop();
        AIZone.SetActive(false);
        pauseUI.SetActive(false);
        statisticsPanel.SetActive(false);
        timerBar.gameObject.SetActive(false);

        GameManager.instance.StartRound();
    }

    public void EndRound()
    {
        round.text = "Раунд - " + GameManager.instance.Round;
        pauseUI.SetActive(true);
        AIZone.SetActive(true);
        statisticsPanel.SetActive(true);
        timerBar.gameObject.SetActive(true);
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
