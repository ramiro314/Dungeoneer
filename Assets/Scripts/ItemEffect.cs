﻿using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public abstract class ItemEffect : MonoBehaviour
{
    public MyThirdPersonUserControl caster;

    public abstract void UseItem();
}
