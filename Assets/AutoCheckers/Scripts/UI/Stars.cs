using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stars : MonoBehaviour
{
    public void TurnOnStars(int amount)
    {
        foreach (var child in gameObject.transform.Cast<Transform>().Take(amount+1))
        {
            child.gameObject.SetActive(true);
        }
    }
}
