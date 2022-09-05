using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Used in the Social Media Buttons in the EndScreenS
/// </summary>
public class OpenURL : MonoBehaviour
{
    [SerializeField] string url;

    // Update is called once per frame
    public void Open()
    {
        Application.OpenURL(url);
    }
}
