using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool IsInputEnabled = true;

    private void Start()
    {
        GameManager.IsInputEnabled = true;
    }
}
