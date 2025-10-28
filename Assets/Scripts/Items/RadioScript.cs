using TMPro;
using UnityEngine;
public class RadioScript : MonoBehaviour
{
    public TextMeshProUGUI textTL, textBL, textTMid, textTR, textBR;

    [Header("Text Field Configuration")]
    [SerializeField] private TextFieldCount textFieldCount = TextFieldCount.Five;
    public enum TextFieldCount
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5
    }
    private float animationDuration = 0.5f;
    private float fadeOutDuration = 0.2f;
    private Vector3[] targetPositions;
    private Vector3[] pickupPositions;
    private Vector3[] originalScales;
    private Vector3 pickupScale = new Vector3(5.468751e-05f, 6.944445e-05f, 1f);
    private bool isAnimating = false;
    private bool isShowing = false;
    private float animationTimer = 0f;
    private bool isCurrentlyVisible = false;
    private CanvasGroup[] canvasGroups;
    private bool isPickedUp = false;
    private Vector3[] originalPositions;
    void Start()
    {
        InitializeCanvasGroups();
        saveTextFinalCoords();
        SaveOriginalScales();
        exitHoverHide();
        SetTextsEnabled(false);
        UpdateUsableTextFieldsInfo();
    }
    [Header("Usable Text Fields")]
    [SerializeField, TextArea(1, 3)] private string usableTextFieldsInfo = "";
    private void OnValidate()
    {
        UpdateUsableTextFieldsInfo();
    }
    private void UpdateUsableTextFieldsInfo()
    {
        switch (textFieldCount)
        {
            case TextFieldCount.One:
                usableTextFieldsInfo = "TMid";
                break;
            case TextFieldCount.Two:
                usableTextFieldsInfo = "TL, TR";
                break;
            case TextFieldCount.Three:
                usableTextFieldsInfo = "TL, TMid, TR";
                break;
            case TextFieldCount.Four:
                usableTextFieldsInfo = "TL, BL, TR, BR";
                break;
            case TextFieldCount.Five:
                usableTextFieldsInfo = "TL, BL, TMid, TR, BR";
                break;
        }
    }
    private void InitializeCanvasGroups()
    {
        TextMeshProUGUI[] texts = GetActiveTexts();
        canvasGroups = new CanvasGroup[texts.Length];

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                canvasGroups[i] = texts[i].GetComponent<CanvasGroup>();
                if (canvasGroups[i] == null)
                {
                    canvasGroups[i] = texts[i].gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
    }
    private void SaveOriginalScales()
    {
        TextMeshProUGUI[] texts = GetActiveTexts();
        originalScales = new Vector3[texts.Length];
        originalPositions = new Vector3[texts.Length];

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                originalScales[i] = texts[i].transform.localScale;
                originalPositions[i] = texts[i].transform.localPosition;
            }
        }
    }
    private void SetupPickupPositions()
    {
        TextMeshProUGUI[] texts = GetActiveTexts();
        pickupPositions = new Vector3[texts.Length];

        switch ((int)textFieldCount)
        {
            case 1:
                if (texts.Length > 0 && texts[0] != null)
                    pickupPositions[0] = new Vector3(0f, 0.25f, 0.171f);
                break;

            case 2:
                if (texts.Length > 0 && texts[0] != null)
                    pickupPositions[0] = new Vector3(0.13f, 0.25f, 0.171f); // TL
                if (texts.Length > 1 && texts[1] != null)
                    pickupPositions[1] = new Vector3(-0.13f, 0.25f, 0.171f); // TR
                break;

            case 3:
                if (texts.Length > 0 && texts[0] != null)
                    pickupPositions[0] = new Vector3(0.13f, 0.25f, 0.171f); // TL
                if (texts.Length > 1 && texts[1] != null)
                    pickupPositions[1] = new Vector3(0f, 0.25f, 0.171f); // TMid
                if (texts.Length > 2 && texts[2] != null)
                    pickupPositions[2] = new Vector3(-0.13f, 0.25f, 0.171f); // TR
                break;

            case 4:
                if (texts.Length > 0 && texts[0] != null)
                    pickupPositions[0] = new Vector3(0.13f, 0.25f, 0.171f); // TL
                if (texts.Length > 1 && texts[1] != null)
                    pickupPositions[1] = new Vector3(-0.13f, 0.15f, 0.171f); // BL
                if (texts.Length > 2 && texts[2] != null)
                    pickupPositions[2] = new Vector3(-0.13f, 0.25f, 0.171f); // TR
                if (texts.Length > 3 && texts[3] != null)
                    pickupPositions[3] = new Vector3(0.13f, 0.15f, 0.171f); // BR
                break;

            case 5:
                if (texts.Length > 0 && texts[0] != null)
                    pickupPositions[0] = new Vector3(0.13f, 0.25f, 0.171f); // TL
                if (texts.Length > 1 && texts[1] != null)
                    pickupPositions[1] = new Vector3(-0.13f, 0.15f, 0.171f); // BL
                if (texts.Length > 2 && texts[2] != null)
                    pickupPositions[2] = new Vector3(0f, 0.25f, 0.171f); // TMid
                if (texts.Length > 3 && texts[3] != null)
                    pickupPositions[3] = new Vector3(-0.13f, 0.25f, 0.171f); // TR
                if (texts.Length > 4 && texts[4] != null)
                    pickupPositions[4] = new Vector3(0.13f, 0.15f, 0.171f); // BR
                break;
        }
    }
    private TextMeshProUGUI[] GetActiveTexts()
    {
        switch ((int)textFieldCount)
        {
            case 1:
                return new TextMeshProUGUI[] { textTMid };
            case 2:
                return new TextMeshProUGUI[] { textTL, textTR };
            case 3:
                return new TextMeshProUGUI[] { textTL, textTMid, textTR };
            case 4:
                return new TextMeshProUGUI[] { textTL, textBL, textTR, textBR };
            case 5:
                return new TextMeshProUGUI[] { textTL, textBL, textTMid, textTR, textBR };
            default:
                return new TextMeshProUGUI[] { textTMid };
        }
    }
    private float timer = 0f;
    private float updateInterval = 0.3f;
    void Update()
    {
        TextMeshProUGUI[] activeTexts = GetActiveTexts();
        bool anyTextEnabled = false;

        foreach (var text in activeTexts)
        {
            if (text != null && text.enabled)
            {
                anyTextEnabled = true;
                break;
            }
        }
        if (anyTextEnabled)
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                updateTexts();
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }

        if (isAnimating)
        {
            animationTimer += Time.deltaTime;

            if (isShowing)
            {
                float progress = Mathf.Clamp01(animationTimer / animationDuration);
                float easedProgress = EaseInOut(progress);
                AnimateTexts(easedProgress, true);
                SetAlpha(1f);
                if (progress >= 1f)
                {
                    CompleteAnimation(true);
                }
            }
            else
            {
                float progress = Mathf.Clamp01(animationTimer / fadeOutDuration);
                float alpha = Mathf.Lerp(1f, 0f, progress);
                SetAlpha(alpha);

                if (progress >= 1f)
                {
                    CompleteAnimation(false);
                }
            }
        }
    }
    private void updateTexts()
    {

    }
    public void onPickUp()
    {
        isPickedUp = true;
        SetupPickupPositions();
        TextMeshProUGUI[] texts = GetActiveTexts();

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && i < pickupPositions.Length && pickupPositions[i] != null)
            {
                texts[i].transform.localPosition = pickupPositions[i];
                texts[i].transform.localScale = pickupScale;
            }
        }
        if (isCurrentlyVisible || isAnimating)
        {
            SetTextsEnabled(true);
            SetAlpha(1f);
        }
    }
    public void onPutDown()
    {
        isPickedUp = false;
        TextMeshProUGUI[] texts = GetActiveTexts();

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && i < originalScales.Length)
            {
                texts[i].transform.localScale = originalScales[i];
                texts[i].transform.localPosition = originalPositions[i];
            }
        }
        if (!isCurrentlyVisible && !isAnimating) return;
        StartHoverMove(false);
    }
    public void onHoverShow()
    {
        if (isCurrentlyVisible) return;

        updateTexts();
        SetTextsEnabled(true);
        SetAlpha(1f);
        if (isPickedUp)
        {
            isCurrentlyVisible = true;
        }
        else
        {
            StartHoverMove(true);
        }
    }
    public void exitHoverHide()
    {
        if (!isCurrentlyVisible && !isAnimating) return;
        if (isPickedUp) return;
        StartHoverMove(false);
    }
    private void StartHoverMove(bool goingIn)
    {
        if (isPickedUp && goingIn)
        {
            isCurrentlyVisible = true;
            return;
        }
        isAnimating = true;
        isShowing = goingIn;
        animationTimer = 0f;
    }
    private void CompleteAnimation(bool visible)
    {
        isAnimating = false;
        animationTimer = 0f;
        isCurrentlyVisible = visible;
        if (!visible)
        {
            SetTextsEnabled(false);
        }
    }
    private void SetTextsEnabled(bool enabled)
    {
        TextMeshProUGUI[] texts = GetActiveTexts();
        foreach (var text in texts)
        {
            if (text != null)
            {
                text.enabled = enabled;
            }
        }
    }
    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            if (canvasGroups[i] != null)
            {
                canvasGroups[i].alpha = alpha;
            }
        }
    }
    private void AnimateTexts(float progress, bool showing)
    {
        if (isPickedUp) return;
        TextMeshProUGUI[] texts = GetActiveTexts();
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && i < targetPositions.Length)
            {
                if (showing)
                {
                    Vector3 startPosition = new Vector3(0f, 0.2f, 0f);
                    texts[i].transform.localPosition = Vector3.Lerp(startPosition, targetPositions[i], progress);
                }
                else
                {
                    texts[i].transform.localPosition = Vector3.Lerp(targetPositions[i], new Vector3(0f, 0.2f, 0f), progress);
                }
            }
        }
    }
    private float EaseInOut(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }
    private void saveTextFinalCoords()
    {
        TextMeshProUGUI[] texts = GetActiveTexts();
        targetPositions = new Vector3[texts.Length];

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                targetPositions[i] = texts[i].transform.localPosition;
            }
        }
    }
}