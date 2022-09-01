using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{
    [SerializeField] string url;

    // Update is called once per frame
    public void Open()
    {
        Application.OpenURL(url);
    }
}
