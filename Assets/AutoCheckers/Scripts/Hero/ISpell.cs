using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpell
{
    bool CanCast();

    void CastSpell();

    void PassiveSpell();

    void ResetAbility();
}
