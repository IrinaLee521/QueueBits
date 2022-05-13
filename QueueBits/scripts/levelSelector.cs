using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelSelector : MonoBehaviour
{
    public void loadlevel(string level)
    {
        SceneManager.LoadScene(level);
    }
}
