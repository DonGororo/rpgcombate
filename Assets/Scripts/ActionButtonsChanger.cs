using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ActionButtonsChanger : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(255, 255, 255, 255);

    }

    public void OnDeselect(BaseEventData eventData)
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0, 0, 0, 255);

    }
}
