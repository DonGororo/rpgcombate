using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class EndScreen : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI endText;
    
    public void Start()
    {
        
        if (BattleSystem.state == BattleState.WON)
        {
            endText.text = "Congratulations, you have won both the battle and all the enemies. Want to repear the experience?";
        }
        else if (BattleSystem.state == BattleState.LOST)
        {
            endText.text = "You may have lost, but at least you will never be alone.  Will you submit or try your chances again?";
        }

    }


    public void RepeatButton()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGameButton()
    {
        UnityEditor.EditorApplication.isPlaying = false;

        Application.Quit();
    }

}
