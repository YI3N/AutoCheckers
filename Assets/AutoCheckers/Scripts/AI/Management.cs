using AutoCheckers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Management : MonoBehaviour
{
    private Player owner;
    private Player opponent;
    private Analytics analytics;

    public Management(Player player)
    {
        owner = player;
        opponent = owner.Tag == GameTag.Human ? GameManager.instance.AI : GameManager.instance.Human;
    }

    void Awake()
    {
        analytics = new Analytics(owner);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PredictIncome()
    {
        float guarantyIncome = Mathf.Clamp(owner.Money / 10, 0, 5);
    }

    public void PredictBattleOutcome()
    {
        float selfDanger = analytics.DangerPlayers[owner.Tag.ToString()];
        float opponentDanger = analytics.DangerPlayers[opponent.Tag.ToString()];
        float mad = analytics.CalculateMAD();

        if (opponentDanger > selfDanger + mad)
        {
            //bad
        }
        else if (opponentDanger < selfDanger - mad)
        {
            //good
        }
        else if (selfDanger >= opponentDanger)
        {
            //good
        }
        else
        {
            //bad
        }
    }

    private void DicideToWinInvest()
    {

    }

    private void WinOutcome()
    {

    }

    private void LooseOutcome()
    {

    }

    private void CountFreeMoney()
    {

    }

    private void DicideToBuyHero()
    {

    }

    private void DicideToSellHero()
    {

    }

    private void DicideToBuyEXP()
    {

    }

    private void DicideToLockShop()
    {

    }
}
