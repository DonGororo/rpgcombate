using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] Slider hpSlider;
    [SerializeField] TextMeshProUGUI hpText;

    [SerializeField] Slider mpSlider;
    [SerializeField] TextMeshProUGUI mpText;  

    public void SetHUD(BattleUnit battleUnit)
    {
        nameText.text = battleUnit.unitName;

        hpSlider.value = hpSlider.maxValue = battleUnit.maxHP;
        hpText.text = battleUnit.maxHP + "/" + battleUnit.maxHP;

        //Como el enemigo no tiene una barra de mana visible, si no esta a?adida simplemente no se ejecutara
        if (mpSlider != null || mpText != null)
        {
            mpSlider.value = mpSlider.maxValue = battleUnit.maxMP;
            mpText.text = battleUnit.maxMP + "/" + battleUnit.maxMP;
        }
    }

    public void UpdateHP(int hp)
    {
        hpSlider.value = hp;
        hpText.text = hp + "/" + hpSlider.maxValue;
    }

    public void UpdateMP(int mp)
    {
        mpSlider.value = mp;
        mpText.text = mp + "/" + mpSlider.maxValue;
    }

}
