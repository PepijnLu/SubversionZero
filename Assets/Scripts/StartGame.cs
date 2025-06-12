using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    [SerializeField] GameObject cutscene;
    [SerializeField] Image textBox, background;
    [SerializeField] Sprite dinnnerSprite, muderSprite;
    [SerializeField] TextMeshProUGUI textBoxText;
    public void StartCutscene(GameObject _startButton)
    {
        _startButton.SetActive(false);
        StartCoroutine(PlayIntroCutscene());
    }

    IEnumerator PlayIntroCutscene()
    {
        cutscene.SetActive(true);

        yield return new WaitForSeconds(1);

        yield return GenericFunctions.instance.FadeImage(textBox, 1, 1);
        textBoxText.text = "It was just an innocent get together with some old friends, in a cabin we had rented online.";

        yield return new WaitForSeconds(6);

        StartCoroutine(GenericFunctions.instance.FadeText(textBoxText, 1, 0));
        yield return GenericFunctions.instance.FadeImage(textBox, 1, 0);
        textBoxText.text = "";

        background.sprite = dinnnerSprite;
        textBoxText.text = "A reunion I had put together, so that we could all catch up after how much time had passed since high school, but...";

         yield return new WaitForSeconds(2);

        StartCoroutine(GenericFunctions.instance.FadeText(textBoxText, 1, 1));
        yield return GenericFunctions.instance.FadeImage(textBox, 1, 1);

        yield return new WaitForSeconds(6);

        StartCoroutine(GenericFunctions.instance.FadeText(textBoxText, 1, 0));
        yield return GenericFunctions.instance.FadeImage(textBox, 1, 0);
        textBoxText.text = "";

        background.sprite = muderSprite;
        textBoxText.text = "I had no idea it would end up with one of my friends dead.";

        yield return new WaitForSeconds(2);

        StartCoroutine(GenericFunctions.instance.FadeText(textBoxText, 1, 1));
        yield return GenericFunctions.instance.FadeImage(textBox, 1, 1);

        yield return new WaitForSeconds(4);

        textBoxText.text = "Find out how Bridget died. Use Tab to open your mental board, and click to take pictures of interesting objects.";

        yield return new WaitForSeconds(5);

        textBoxText.text = "Connect the evidence by clicking on the right words in the description of a polaroid, and clicking and dragging the pins together wherever you find a connection.";

        yield return new WaitForSeconds(7);

        StartCoroutine(GenericFunctions.instance.FadeText(textBoxText, 4, 0));
        StartCoroutine(GenericFunctions.instance.FadeImage(textBox, 4, 0));
        yield return GenericFunctions.instance.FadeImage(background, 4, 0);

        SceneManager.LoadScene("Pepijn2");
    }
}
