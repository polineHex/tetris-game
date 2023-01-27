using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; //to restart the game

public class PlayerDamage : MonoBehaviour
{
    private TextMeshProUGUI lifeText;

    private int lifeCount;

   


    // Start is called before the first frame update
    void Awake()
    {
        lifeText = GameObject.Find("LifeText").GetComponent<TextMeshProUGUI>();
        lifeCount = 3;
        lifeText.SetText("x" + lifeCount);




    }

    private void Start()
    {
        Time.timeScale = 1f;
    }


    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(2f);

        SceneManager.LoadScene("Tetris");
    }

    public void EndGame()
    {
        Time.timeScale = 0f;
        //StartCoroutine(RestartGame());
    }

  













}//class
