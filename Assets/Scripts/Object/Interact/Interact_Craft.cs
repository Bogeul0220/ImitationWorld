using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_Craft : InteractionObjectBase
{
    public override void InitInteractObject()
    {
        interactionObject = UIManager.Instance.craftingPanelDisplay;
    }
}
