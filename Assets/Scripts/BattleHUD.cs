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

    // Iconos de Debufos
    public static int grindMax =9;
    public static Image[] myIconGrinds;

    public bool defenseActive;
    public static Sprite defenseIcon;
    public static Sprite blindIcon;

    private void Awake()
    {
        myIconGrinds = new Image[grindMax];
        defenseIcon = Resources.Load<Sprite>("defense");
        blindIcon = Resources.Load<Sprite>("blind");
        SetGrindLayout();
    }


    public void SetHUD(BattleUnit battleUnit)
    {
        nameText.text = battleUnit.unitName;

        hpSlider.maxValue = battleUnit.maxHP;
        hpSlider.value = battleUnit.currentHP;
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
        if (mpSlider == null) return;
        mpSlider.value = mp;
        mpText.text = mp + "/" + mpSlider.maxValue;
    }

    #region Icon Mechanics
    

    public void SetGrindLayout()
    {
        for (int i = 0; i < grindMax; i++)
        {
            foreach (var image in gameObject.GetComponentsInChildren<Image>(true))
            {
                if (image.name == "Buff " + (i + 1))
                {
                    myIconGrinds[i] = image;

                    Debug.Log("true");
                    myIconGrinds[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
    public void PutIconIn(string debuff)
    {
        for (int i = 0; i < grindMax; i++)
        {
            
            if(myIconGrinds[i].gameObject.activeInHierarchy == false)
            {
                myIconGrinds[i].gameObject.SetActive(true);

                switch (debuff)
                {
                    case "defense":
                        myIconGrinds[i].sprite = defenseIcon;
                        break;

                    case "blind":
                        myIconGrinds[i].sprite = blindIcon;
                        break;
                }
                
                i = grindMax;
            }
        }
    }


    public void PutIconOut(string debuff)
    {
        for (int i = 0; i < grindMax; i++)
        {

            if (myIconGrinds[i].sprite.name == debuff)
            {
                myIconGrinds[i].sprite = null;

                myIconGrinds[i].gameObject.SetActive(false);

                i = grindMax;
            }
        }
    }
    
    #endregion
}
