using AutoCheckers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject heroStatisticPrefab;

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


    void Start()
    {
        humanAttackButton.onClick.AddListener(() => SwitchStatistics(0));
        humanDefenceButton.onClick.AddListener(() => SwitchStatistics(1));
        aiAttackButton.onClick.AddListener(() => SwitchStatistics(2));
        aiDefenceButton.onClick.AddListener(() => SwitchStatistics(3));
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

    public void SetPlayerStatistics(Player player)
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

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
