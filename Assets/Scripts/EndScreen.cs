using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EndScreen : MonoBehaviour
{

    public GameObject winScreen;

    public GameObject loseScreen;
    
    public void Start()
    {
        
        if (BattleSystem.state == BattleState.WON)
        {
            winScreen.SetActive(true);
        }
        else if (BattleSystem.state == BattleState.LOST)
        {
            loseScreen.SetActive(true);
        }

    }


    public void RepeatButton()
    {
        SceneManager.LoadScene("Battle 2 Electric boongaloo");
    }

    public void ExitGameButton()
    {
        UnityEditor.EditorApplication.isPlaying = false;

        Application.Quit();
    }

}
