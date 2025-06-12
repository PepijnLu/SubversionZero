using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ending : MonoBehaviour
{
    bool ending;
    [SerializeField] private float timeInMinutes = 15f;
    [SerializeField] float timeRemaining;
    private bool timerRunning = false;
    [SerializeField] Image black;
    [SerializeField] TextMeshProUGUI text1, text2, text3, text4, text5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartTimer();   
    }
    public void EndGame()
    {
        if(!ending) StartCoroutine(EndGameCutscene());
    }

    public void Update()
    {
        if(!ending)
        {
            if(GameManager.instance.succesfulConnections >= 3)
            {
                StartCoroutine(EndGameCutscene());
            }

            if (timerRunning)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0f)
                {
                    timerRunning = false;
                    timeRemaining = 0f;
                    TimerEnded();
                }
            }
        }   
    }

    IEnumerator EndGameCutscene()
    {
        ending = true;
        GameManager.instance.isTransitioning = true;

        yield return GenericFunctions.instance.FadeImage(black, 5, 1);
        yield return new WaitForSeconds(1);
        yield return GenericFunctions.instance.FadeText(text1, 3, 1);
        yield return new WaitForSeconds(2);
        yield return GenericFunctions.instance.FadeText(text2, 3, 1);
        yield return new WaitForSeconds(2);
        yield return GenericFunctions.instance.FadeText(text3, 3, 1);
        yield return new WaitForSeconds(4);

        StartCoroutine(GenericFunctions.instance.FadeText(text1, 3, 0));
        StartCoroutine(GenericFunctions.instance.FadeText(text2, 3, 0));
        yield return GenericFunctions.instance.FadeText(text3, 3, 0);

        yield return GenericFunctions.instance.FadeText(text4, 3, 1);
        yield return new WaitForSeconds(1);
        yield return GenericFunctions.instance.FadeText(text5, 3, 1);
        yield return new WaitForSeconds(10);

        SceneManager.LoadScene("MainMenu");
    }

    public void StartTimer()
    {
        timeRemaining = timeInMinutes * 60f;
        timerRunning = true;
    }

    private void TimerEnded()
    {
        StartCoroutine(EndGameCutscene());
    }
}
