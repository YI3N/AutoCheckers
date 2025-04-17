using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KnightAbility : MonoBehaviour, IAbility
{
    private readonly int shieldTime = 3;
    private readonly int bonusMagicalResistance = 70;
    private readonly int bonusArmor = 30;
    private readonly List<int> shieldChance = new List<int>() { 0, 30 };

    private Hero hero;
    private int currentLvL;

    public static readonly int lvlThreshold = 2;
    public static readonly int maxLvl = 2;

    private bool isCooldown = false;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void ActivateAbility(int heroes)
    {
        if (heroes <= lvlThreshold)
            return;

        currentLvL = heroes / lvlThreshold;
        hero.GainOnAttackEvent(TryToApplyDivineShield);
    }

    public void DeactivateAbility(int heroes)
    {
        if (heroes <= lvlThreshold)
            return;

        currentLvL = heroes / lvlThreshold;

        hero.RemoveOnAttackEvent(TryToApplyDivineShield);
    }

    private void TryToApplyDivineShield()
    {
        if (!isCooldown && shieldChance[currentLvL] >= Random.Range(0, 100))
        {
            isCooldown = true;

            hero.GainMagicalResistance(bonusMagicalResistance);
            hero.GainArmor(bonusArmor);

            StartCoroutine(Deactivate());
        }
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(shieldTime);

        hero.GainMagicalResistance(-bonusMagicalResistance);
        hero.GainArmor(-bonusArmor);

        isCooldown = false;
    }

    public int GetLvlThreshold()
    {
        return lvlThreshold;
    }

    public int GetMaxLvl()
    {
        return maxLvl;
    }
}
