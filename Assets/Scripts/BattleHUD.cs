using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Basic HUD data
/// </summary>
public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] Slider hpSlider;
    [SerializeField] TextMeshProUGUI hpText;

    [SerializeField] Slider mpSlider;
    [SerializeField] TextMeshProUGUI mpText;

    public GameObject[] buffsIcons;

    public void SetHUD(BattleUnit battleUnit, bool itsEnemy)
    {
        nameText.text = battleUnit.unitName;

        hpSlider.maxValue = battleUnit.maxHP;
        hpSlider.value = battleUnit.currentHP;
        hpText.text = battleUnit.maxHP + "/" + battleUnit.maxHP;

        mpSlider.maxValue = battleUnit.maxMP;
        mpSlider.value = battleUnit.currentMP;
        mpText.text = battleUnit.maxMP + "/" + battleUnit.maxMP;

        //  If the HUD is for an enemy, MP data will no show to the player
        if (itsEnemy)
        {
            mpSlider.gameObject.SetActive(false);
            mpText.gameObject.SetActive(false);
        }
    }

    public void UpdateHP(int hp)
    {
        hpSlider.value = hp;
        hpText.text = hp + "/" + hpSlider.maxValue;
    }

    public void UpdateMP(int mp)
    {
        if (mpSlider == null) return;
        mpSlider.value = mp;
        mpText.text = mp + "/" + mpSlider.maxValue;
    }

}
