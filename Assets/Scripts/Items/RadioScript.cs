using TMPro;
using UnityEngine;

public class RadioScript : MonoBehaviour
{
    public TextMeshProUGUI textTL, textBL, textTMid, textTR, textBR;
    private Transform endTL, endBL, endTMid, endTR, endBR;
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
    }

    private void InitializeCanvasGroups()
    {
        TextMeshProUGUI[] texts = { textTL, textBL, textTMid, textTR, textBR };
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
        TextMeshProUGUI[] texts = { textTL, textBL, textTMid, textTR, textBR };
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
        TextMeshProUGUI[] texts = { textTL, textBL, textTMid, textTR, textBR };
        pickupPositions = new Vector3[5];

        // Set up pickup positions based on your specifications
        if (textTL != null) pickupPositions[0] = new Vector3(0.13f, 0.25f, 0.171f);
        if (textTMid != null) pickupPositions[2] = new Vector3(0f, 0.25f, 0.171f);
        if (textTR != null) pickupPositions[3] = new Vector3(-0.13f, 0.25f, 0.171f);
        if (textBL != null) pickupPositions[1] = new Vector3(-0.13f, 0.15f, 0.171f);
        if (textBR != null) pickupPositions[4] = new Vector3(0.13f, 0.15f, 0.171f);
    }

    private float timer = 0f;
    private float updateInterval = 0.3f;

    void Update()
    {
        if (textTL.enabled == true)
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
        // Your text update logic here
    }

    public void onPickUp()
    {
        isPickedUp = true;
        SetupPickupPositions();

        // Immediately move and scale texts for pickup view
        TextMeshProUGUI[] texts = { textTL, textBL, textTMid, textTR, textBR };

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && pickupPositions[i] != null)
            {
                texts[i].transform.localPosition = pickupPositions[i];
                texts[i].transform.localScale = pickupScale;
            }
        }

        // Show texts if they were visible before pickup
        if (isCurrentlyVisible || isAnimating)
        {
            SetTextsEnabled(true);
            SetAlpha(1f);
        }
    }

    public void onPutDown()
    {
        isPickedUp = false;

        // Restore original scales and positions
        TextMeshProUGUI[] texts = { textTL, textBL, textTMid, textTR, textBR };

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                texts[i].transform.localScale = originalScales[i];
                texts[i].transform.localPosition = originalPositions[i];
            }
        }

        // Fade out texts on put down
        if (!isCurrentlyVisible && !isAnimating) return;
        StartHoverMove(false);
    }

    public void onHoverShow()
    {
        if (isCurrentlyVisible) return;

        updateTexts();
        SetTextsEnabled(true);
        SetAlpha(1f);

        // If picked up, show texts immediately at pickup positions
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

        // If picked up, don't hide on hover exit
        if (isPickedUp) return;

        StartHoverMove(false);
    }

    private void StartHoverMove(bool goingIn)
    {
        // Don't animate position if picked up - just handle visibility
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
        textTL.enabled = enabled;
        textBL.enabled = enabled;
        textTMid.enabled = enabled;
        textTR.enabled = enabled;
        textBR.enabled = enabled;
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
        // Don't animate positions if picked up
        if (isPickedUp) return;

        TextMeshProUGUI[] texts = { textTL, textBL, textTMid, textTR, textBR };

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                if (showing)
                {
                    Vector3 startPosition = new Vector3(0f, 0.2f, 0f);
                    texts[i].transform.localPosition = Vector3.Lerp(startPosition, targetPositions[i], progress);
                }
                else
                {
                    // When hiding, move back to start position (only if not picked up)
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
        TextMeshProUGUI[] texts = { textTL, textBL, textTMid, textTR, textBR };
        targetPositions = new Vector3[5];

        if (textTL != null) targetPositions[0] = textTL.transform.localPosition;
        if (textBL != null) targetPositions[1] = textBL.transform.localPosition;
        if (textTMid != null) targetPositions[2] = textTMid.transform.localPosition;
        if (textTR != null) targetPositions[3] = textTR.transform.localPosition;
        if (textBR != null) targetPositions[4] = textBR.transform.localPosition;

        endTL = textTL?.transform;
        endBL = textBL?.transform;
        endTMid = textTMid?.transform;
        endTR = textTR?.transform;
        endBR = textBR?.transform;
    }
}