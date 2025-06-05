using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public virtual void OnClosedUI()
    {
        InputManager.Instance.EscapeDisplayPressed.Invoke(); // UI 닫기 이벤트 호출
    }
}
