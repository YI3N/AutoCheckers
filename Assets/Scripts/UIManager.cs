using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AutoCheckers;
using System.Linq;

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
    private GameObject buffPrefab;

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
        playButton.onClick.AddListener(BeginGame);
        startButton.onClick.AddListener(StartRound);

        shopOpenButton.onClick.AddListener(OpenShop);
        shopCloseButton.onClick.AddListener(CloseShop);
        shopRerollButton.onClick.AddListener(Shop.instance.TryToRerollHumanShop);
        shopEXPButton.onClick.AddListener(Shop.instance.TryToEXPHumanShop);

        pauseUI.SetActive(false);
        playersUI.SetActive(false);
        shopUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdatePlayerUI(GameTag tag)
    {
        if (tag == GameTag.Human)
        {
            UpdateHumanUI();
        }
        else if (tag == GameTag.AI)
        {
            UpdateAIUI();
        }
    }

    public void UpdateHumanUI()
    {
        humanExp.text = GameManager.instance.Human.Exp.ToString() + "/" + GameManager.instance.Human.MaxExp.ToString();
        humanLVL.text = "Ур. " + GameManager.instance.Human.Level.ToString();
        humanMoney.text = GameManager.instance.Human.Money.ToString();
        humanStreak.text = GameManager.instance.Human.WinStreak.ToString();
        humanWin.text = GameManager.instance.Human.Wins.ToString();
        humanHealth.text = GameManager.instance.Human.CurrentHealth.ToString();
        humanHealthBar.value = GameManager.instance.Human.CurrentHealth / (float)GameManager.instance.Human.MaxHealth;

        GameManager.instance.Human.ClassHeroes.OrderByDescending(pair => pair.Value);
        GameManager.instance.Human.RaceHeroes.OrderByDescending(pair => pair.Value);

        foreach (Transform child in humanClass.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in humanRace.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (KeyValuePair<string, int> heroClass in GameManager.instance.Human.ClassHeroes)
        {
            if (heroClass.Value > 0)
            {
                int threshold = 3;
                GameObject classText = Instantiate(buffPrefab, humanClass.transform, true);
                classText.GetComponent<TMP_Text>().text = heroClass.Key + "\n" + heroClass.Value + "/" + threshold * (1 + (heroClass.Value / threshold));
            }
            
        }
        foreach (KeyValuePair<string, int> heroRace in GameManager.instance.Human.RaceHeroes)
        {
            if (heroRace.Value > 0)
            {
                int threshold = 3;
                if (heroRace.Key == nameof(Race.Orc))
                    threshold = 2;

                GameObject raceText = Instantiate(buffPrefab, humanRace.transform, true);
                raceText.GetComponent<TMP_Text>().text = heroRace.Key + "\n" + heroRace.Value + "/" + threshold * (1 + (heroRace.Value / threshold));
            }
        }
    }

    public void UpdateAIUI()
    {
        aiExp.text = GameManager.instance.AI.Exp.ToString() + "/" + GameManager.instance.AI.MaxExp.ToString();
        aiLVL.text = "Ур. " + GameManager.instance.AI.Level.ToString();
        aiMoney.text = GameManager.instance.AI.Money.ToString();
        aiStreak.text = GameManager.instance.AI.WinStreak.ToString();
        aiWin.text = GameManager.instance.AI.Wins.ToString();
        aiHealth.text = GameManager.instance.AI.CurrentHealth.ToString();
        aiHealthBar.value = GameManager.instance.AI.CurrentHealth / (float)GameManager.instance.AI.MaxHealth;

        foreach (Transform child in aiClass.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in aiRace.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (KeyValuePair<string, int> heroClass in GameManager.instance.AI.ClassHeroes)
        {
            if (heroClass.Value > 0)
            {
                int threshold = 3;
                GameObject classText = Instantiate(buffPrefab, aiClass.transform, true);
                classText.GetComponent<TMP_Text>().text = heroClass.Key + "\n" + heroClass.Value + "/" + threshold * (1 + (heroClass.Value / threshold));
            }

        }
        foreach (KeyValuePair<string, int> heroRace in GameManager.instance.AI.RaceHeroes)
        {
            if (heroRace.Value > 0)
            {
                int threshold = 3;
                if (heroRace.Key == nameof(Race.Orc))
                    threshold = 2;

                GameObject raceText = Instantiate(buffPrefab, aiRace.transform, true);
                raceText.GetComponent<TMP_Text>().text = heroRace.Key + "\n" + heroRace.Value + "/" + threshold * (1 + (heroRace.Value / threshold));
            }
        }
    }

    private void BeginGame()
    {
        menuUI.SetActive(false);
        playersUI.SetActive(true);
        UpdateHumanUI();
        UpdateAIUI();
        OpenShop();
        GameManager.instance.BeginGame();
    }

    private void StartRound()
    {
        AIZone.SetActive(false);
        pauseUI.SetActive(false);
        GameManager.instance.StartRound();
    }

    public void EndRound()
    {
        round.text = "Раунд - " + GameManager.instance.Round;
        pauseUI.SetActive(true);
        AIZone.SetActive(true);
    }

    private void OpenShop()
    {
        shopUI.SetActive(true);
        pauseUI.SetActive(false);
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        pauseUI.SetActive(true);
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
