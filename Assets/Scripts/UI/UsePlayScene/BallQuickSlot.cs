using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class BallQuickSlot : MonoBehaviour
{
    public BallType checkingBall;
    public Image BackgroundImage;
    public Image BallIcon;
    public TMP_Text BallCount;

    void Start()
    {
        InventoryManager.Instance.OnInventoryChanged += BallUIUpdate;
        BallUIUpdate();
    }

    public void BallUIUpdate()
    {
        var dict = InventoryManager.Instance.InvenItemDict;
        bool found = false;

        foreach (var pair in dict)
        {
            if (pair.Value is BallItem)
            {
                var target = (pair.Value as BallItem).CountableItemData as BallItemData;
                if (target.ballType == checkingBall)
                {
                    BallCount.text = (pair.Value as BallItem).Amount.ToString();
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            BallCount.text = "-";
        }
    }
}
