using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbility
{
    void ActivateAbility(int heroes);

    void DeactivateAbility(int heroes);
}
