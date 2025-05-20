using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopChances : MonoBehaviour
{
    [SerializeField]
    private TMP_Text common;
    [SerializeField]
    private TMP_Text uncommon;
    [SerializeField]
    private TMP_Text rare;
    [SerializeField]
    private TMP_Text mythic;
    [SerializeField]
    private TMP_Text legendary;

    public void SetChances(float[] chances)
    {
        common.text = (chances[0] * 100).ToString() + "%";
        uncommon.text = (chances[1] * 100).ToString() + "%";
        rare.text = (chances[2] * 100).ToString() + "%";
        mythic.text = (chances[3] * 100).ToString() + "%";
        legendary.text = (chances[4] * 100).ToString() + "%";
    }
}
