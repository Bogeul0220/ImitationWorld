using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_Craft : InteractionObjectBase
{
    public override void OnInteract()
    {
        UIManager.Instance.DisplayInteractCraft();
    }
}
