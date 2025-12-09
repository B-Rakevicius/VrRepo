using UnityEngine;
using TMPro;
using System.Collections;

public class WaveUI : MonoBehaviour
{
    public static WaveUI Instance;
    public GameObject canvasObject, canvasObject2;
    public TextMeshProUGUI frontText;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI backText;
    private void Awake()
    {
        Instance = this;
        canvasObject.SetActive(false);
        canvasObject2.SetActive(false);
    }
    public void WaveShortMessage(string msg)
    {
        canvasObject.SetActive(true);
        canvasObject2.SetActive(true);

        frontText.text = msg;
        leftText.text = msg;
        rightText.text = msg;
        backText.text = msg;
        StartCoroutine(HideDelayed());
    }
    private IEnumerator HideDelayed()
    {
        yield return new WaitForSeconds(2.5f);
        Hide();
    }
    public void Hide()
    {
        canvasObject.SetActive(false);
        canvasObject2.SetActive(false);
    }
}
