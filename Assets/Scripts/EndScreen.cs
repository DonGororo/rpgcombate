using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EndScreen : MonoBehaviour
{
    
    public void RepeatButton()
    {
        SceneManager.LoadScene("Battle 2 Electric boongaloo");
    }

    public void ExitGameButton()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

}
