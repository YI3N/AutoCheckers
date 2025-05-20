using AutoCheckers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroStatistic : MonoBehaviour
{
    [SerializeField]
    private RawImage heroIcon;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Image fillSlider;
    [SerializeField]
    private TMP_Text statisticText;
    [SerializeField]
    private Stars upgrades;

    public int Statistic { get; private set; }

    public void SetStatistic(Hero hero, float total, bool attackStatistic)
    {
        heroIcon.texture = hero.gameObject.transform.Find("HeroPlane").GetComponent<Renderer>().material.mainTexture;

        Statistic = attackStatistic ? hero.AttackStatistics : hero.DefenceStatistics;
        statisticText.text = Statistic.ToString();
        slider.value = Statistic / total;

        fillSlider.color = attackStatistic ? Color.red : Color.blue;

        upgrades.TurnOnStars(hero.Upgrades);
    }
}
