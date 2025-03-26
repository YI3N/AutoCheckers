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
    private GameObject abilityPrefab;
    [SerializeField]
    private GameObject heroStatisticPrefab;

    [SerializeField]
    private TMP_Text humanExp;
    [SerializeField]
    private TMP_Text humanLVL;
    [SerializeField]
    private TMP_Text humanMoney;
    [SerializeField]
    private TMP_Text humanStreak;
    [SerializeField]
    private TMP_Text humanWin;
    [SerializeField]
    private TMP_Text humanHealth;
    [SerializeField]
    private Slider humanHealthBar;
    [SerializeField]
    private GameObject humanClass;
    [SerializeField]
    private GameObject humanRace;

    [SerializeField]
    private TMP_Text aiExp;
    [SerializeField]
    private TMP_Text aiLVL;
    [SerializeField]
    private TMP_Text aiMoney;
    [SerializeField]
    private TMP_Text aiStreak;
    [SerializeField]
    private TMP_Text aiWin;
    [SerializeField]
    private TMP_Text aiHealth;
    [SerializeField]
    private Slider aiHealthBar;
    [SerializeField]
    private GameObject aiClass;
    [SerializeField]
    private GameObject aiRace;

    [SerializeField]
    private GameObject statistics;
    [SerializeField]
    private GameObject panels;
    [SerializeField]
    private Button humanAttackButton;
    [SerializeField]
    private GameObject humanAttackPanel;
    [SerializeField]
    private Button humanDefenceButton;
    [SerializeField]
    private GameObject humanDefencePanel;
    [SerializeField]
    private Button aiAttackButton;
    [SerializeField]
    private GameObject aiAttackPanel;
    [SerializeField]
    private Button aiDefenceButton;
    [SerializeField]
    private GameObject aiDefencePanel;

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
        statistics.SetActive(true);

        playButton.onClick.AddListener(BeginGame);
        startButton.onClick.AddListener(StartRound);

        shopOpenButton.onClick.AddListener(OpenShop);
        shopCloseButton.onClick.AddListener(CloseShop);
        shopRerollButton.onClick.AddListener(Shop.instance.TryToRerollHumanShop);
        shopEXPButton.onClick.AddListener(Shop.instance.TryToBuyEXPHumanShop);

        humanAttackButton.onClick.AddListener(() => SwitchStatistics(0));
        humanDefenceButton.onClick.AddListener(() => SwitchStatistics(1));
        aiAttackButton.onClick.AddListener(() => SwitchStatistics(2));
        aiDefenceButton.onClick.AddListener(() => SwitchStatistics(3));

        pauseUI.SetActive(false);
        playersUI.SetActive(false);
        shopUI.SetActive(false);
        statistics.SetActive(false);
    }

    private (Player player, TMP_Text exp, TMP_Text lvl, TMP_Text money, TMP_Text streak, TMP_Text win, 
        TMP_Text health, Slider healthBar, GameObject classSection, GameObject raceSection) GetUIData(GameTag tag)
    {
        if (tag == GameTag.Human)
            return (GameManager.instance.Human, humanExp, humanLVL, humanMoney, humanStreak, humanWin, humanHealth, humanHealthBar, humanClass, humanRace);
        else
            return (GameManager.instance.AI, aiExp, aiLVL, aiMoney, aiStreak, aiWin, aiHealth, aiHealthBar, aiClass, aiRace);
    }

    public void UpdatePlayerUI(GameTag tag)
    {
        var (player, exp, lvl, money, streak, win, health, healthBar, classSection, raceSection) = GetUIData(tag);

        exp.text = player.Exp.ToString() + "/" + player.MaxExp.ToString();
        lvl.text = "Ур. " + player.Level.ToString();
        money.text = player.Money.ToString();
        streak.text = player.WinStreak.ToString();
        win.text = player.Wins.ToString();
        health.text = player.CurrentHealth.ToString();
        healthBar.value = player.CurrentHealth / (float)player.MaxHealth;

        foreach (Transform child in classSection.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in raceSection.transform)
        {
            Destroy(child.gameObject);
        }

        SetAbilities(tag, player.ClassHeroes);
        SetAbilities(tag, player.RaceHeroes);
    }

    public void SetStatistics(Player player)
    {
        GameObject attackPanel = player.Tag == GameTag.Human ? humanAttackPanel : aiAttackPanel;
        GameObject defencePanel = player.Tag == GameTag.Human ? humanDefencePanel : aiDefencePanel;

        foreach (Transform stat in attackPanel.transform)
            Destroy(stat.gameObject);
        foreach (Transform stat in defencePanel.transform)
            Destroy(stat.gameObject);

        foreach (GameObject hero in player.HeroesOnBoard)
        {
            GameObject heroAttackStatistic = Instantiate(heroStatisticPrefab, attackPanel.transform);
            GameObject heroDefenceStatistic = Instantiate(heroStatisticPrefab, defencePanel.transform);

            heroAttackStatistic.GetComponent<HeroStatistic>().SetStatistic(hero.GetComponent<Hero>(), player.AttackStatistics, true);
            heroDefenceStatistic.GetComponent<HeroStatistic>().SetStatistic(hero.GetComponent<Hero>(), player.DefenceStatistics, false);
        }

        foreach (Transform panel in panels.transform)
        {
            var children = panel.Cast<Transform>().OrderByDescending(child => child.GetComponent<HeroStatistic>().Statistic).ToList();

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetSiblingIndex(i);
            }
        }
    }

    private void SetAbilities(GameTag tag, Dictionary<string, int> abilities)
    {
        foreach (KeyValuePair<string, int> ability in abilities)
        {
            if (ability.Value <= 0)
                continue;

            int threshold = 3;
            if (ability.Key == nameof(Race.Orc))
                threshold = 2;

            string amount = ability.Value + "/" + threshold * (1 + ability.Value / threshold);

            Enum abilityType = null;
            GameObject abilitySection = null;
            if (Enum.TryParse(ability.Key, out HeroClass heroClass))
            {
                abilityType = heroClass;
                abilitySection = tag == GameTag.Human ? humanClass : aiClass;

            }
            else if (Enum.TryParse(ability.Key, out Race race))
            {
                abilityType = race;
                abilitySection = tag == GameTag.Human ? humanRace : aiRace;
            }

            GameObject abilityIcon = Instantiate(abilityPrefab, abilitySection.transform);
            abilityIcon.GetComponent<AbilityIcon>().SetAbility(abilityType, amount);

        }
    }

    private void BeginGame()
    {
        menuUI.SetActive(false);
        playersUI.SetActive(true);
        statistics.SetActive(true);

        UpdatePlayerUI(GameTag.Human);
        UpdatePlayerUI(GameTag.AI);

        OpenShop();
        GameManager.instance.BeginGame();
    }

    private void StartRound()
    {
        AIZone.SetActive(false);
        pauseUI.SetActive(false);
        statistics.SetActive(false);

        GameManager.instance.StartRound();
    }

    public void EndRound()
    {
        round.text = "Раунд - " + GameManager.instance.Round;
        pauseUI.SetActive(true);
        AIZone.SetActive(true);
        statistics.SetActive(true);
    }

    private void OpenShop()
    {
        shopUI.SetActive(true);
        pauseUI.SetActive(false);
        sellPlane.SetActive(false);
        statistics.SetActive(false);
        round.gameObject.SetActive(false);
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        pauseUI.SetActive(true);
        sellPlane.SetActive(true);
        statistics.SetActive(true);
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

    private void SwitchStatistics(int tab)
    {
        ColorBlock on = humanAttackButton.colors;
        on.normalColor = Color.white;

        ColorBlock off = humanAttackButton.colors;
        off.normalColor = new Color(69f / 255f, 69f / 255f, 69f / 255f);

        // Массивы для удобства
        GameObject[] panels = { humanAttackPanel, humanDefencePanel, aiAttackPanel, aiDefencePanel };
        Button[] buttons = { humanAttackButton, humanDefenceButton, aiAttackButton, aiDefenceButton };

        // Переключаем активные панели
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == tab);
            buttons[i].colors = (i == tab) ? on : off;
        }
    }
}
