using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public static WaveUI Instance;

    public GameObject canvasObject;

    public TextMeshProUGUI frontText;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI backText;

    private void Awake()
    {
        Instance = this;
        canvasObject.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        canvasObject.SetActive(true);

        frontText.text = msg;
        leftText.text = msg;
        rightText.text = msg;
        backText.text = msg;
    }

    public void Hide()
    {
        canvasObject.SetActive(false);
    }
}
