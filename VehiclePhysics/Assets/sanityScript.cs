using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  a script for my sanity
/// </summary>
public class sanityScript : MonoBehaviour
{
    public float timeScale;

    void Update()
    {
        Time.timeScale = timeScale;
    }
}
