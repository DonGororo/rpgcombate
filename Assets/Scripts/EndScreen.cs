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
            endText.text = "The evil was defeated. Would you like to repeat your sacred mission?";
        }
        else if (BattleSystem.state == BattleState.LOST)
        {
            endText.text = "Do not falter hero! Only you can defeat this wicked devil. Shall you try again?";
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
