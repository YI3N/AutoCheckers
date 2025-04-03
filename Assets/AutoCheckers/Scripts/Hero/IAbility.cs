using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbility
{
    static int lvlThreshold;
    static int MaxLvl;

    void ActivateAbility(int heroes);

    void DeactivateAbility(int heroes);

    int GetLvlThreshold();
    int GetMaxLvl();
}
