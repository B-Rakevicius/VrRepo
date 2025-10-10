using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
public class PointManager : MonoBehaviour
{
    public TextMeshProUGUI slainScore ,pointScore, enemyBuffer;
    public static PointManager Instance { get; private set; }
    public EnemySpawner enemyspawner; // enemy spawner class / gameobject, reference for internal variable "totalEnemies" calculations, used to keep track of current total enemies by removing one in the slainEnemy methods
    public int currentSlain = -1, currentPoints = 0;

    //public MyController myController;  // only used to upgrade movespeed in this script, otherwise useless
    public Volume PostProcessingVolumeObject; // used to change various post processing variables in the volume gameobject, used to enable chromatic aberration when leveling
    public GameObject devElements, uiElements, lvlUpElements, diedElements; // various UI elements displayed on screen, maybe should have this in UIManager or something

    // All leveling logic variables, currently just trying to get the game to run, later could implement leveling for replayability
    /*
    public int levelUpPoints = 10, currentLevel = 1;
    public GameObject levelUpUI;
    private bool isLevelingUp = false;
    public int dmgUp = 0, rangeUp = 0, cdUp = 0, speedUp = 0;
    public int meleeUp =0, lightUp =0, anvilUp=0, fireUp=0, aoeUp=0;
    public float lvlScalingRate = 1.2f;
    private UpgradeType[] levelUpOptions = new UpgradeType[3];
    */
    public enum UpgradeType
    {
        DmgUp,
        RangeUp,
        CdUp,
        SpeedUp,
        MeleeUp,
        LightUp,
        AnvilUp,
        FireUp,
        AoeUp
    }
    public TextMeshProUGUI Upgrade1, Upgrade2, Upgrade3;
    public UnityEngine.UI.Image Up1Icon, Up2Icon, Up3Icon;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    public void addPoint()
    {
        currentPoints++;
        //pointScore.text = "Points " + currentSlain;
        //checkForLevelup();
    }
    public void addPoint(int amount)
    {
        currentPoints += amount;
        //UIupdate();
        //checkForLevelup();
    }
    public void addSlain()
    {
        currentSlain++;
        //enemyspawner.totalEnemies--;
        UIupdate();
    }
    /*
    public void checkForLevelup()
    {
        while (currentPoints >= levelUpPoints)
        {
            currentPoints -= levelUpPoints;
            currentLevel++;
            levelUpPoints = Mathf.CeilToInt(levelUpPoints * lvlScalingRate);
            Time.timeScale = 0f;
            GenerateRandomLevelUpOptions();
            if (PostProcessingVolumeObject.profile.TryGet<ChromaticAberrationPP>(out var chromaticEffect))
            {
                StartCoroutine(FadeInChromaticAberration(1f, 3f));
            }
            if (levelUpUI != null)
            {
                levelUpUI.SetActive(true);
            }
            isLevelingUp = true;
            return;
        }
    }
    public IEnumerator FadeInChromaticAberration(float duration, float targetValue)
    {
        if (!PostProcessingVolumeObject.profile.TryGet<ChromaticAberrationPP>(out var chromaticEffect))
            yield break;

        chromaticEffect._isEnabled.value = true;

        float timeElapsed = 0f;
        float startValue = chromaticEffect._aberrationAmount.value;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float t = timeElapsed / duration;
            chromaticEffect._aberrationAmount.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }

        chromaticEffect._aberrationAmount.value = targetValue;
    }
    ///////////////////////////
    private void GenerateRandomLevelUpOptions()
    {
        List<UpgradeType> allUpgrades = new List<UpgradeType>
        {
            UpgradeType.DmgUp,
            UpgradeType.RangeUp,
            UpgradeType.CdUp,
            UpgradeType.SpeedUp,
        };
        //if (dmgUp < int.MaxValue) allUpgrades.Add(UpgradeType.DmgUp);
        //if (rangeUp < int.MaxValue) allUpgrades.Add(UpgradeType.RangeUp);
        //if (cdUp < int.MaxValue) allUpgrades.Add(UpgradeType.CdUp);
        //if (speedUp < int.MaxValue) allUpgrades.Add(UpgradeType.SpeedUp);
        // if cd up needs a cap? too fast might bug out anvil
        if (meleeUp < 5) allUpgrades.Add(UpgradeType.MeleeUp);
        if (lightUp < 5) allUpgrades.Add(UpgradeType.LightUp);
        if (anvilUp < 5) allUpgrades.Add(UpgradeType.AnvilUp);
        if (fireUp < 5) allUpgrades.Add(UpgradeType.FireUp);
        if (aoeUp < 5) allUpgrades.Add(UpgradeType.AoeUp);
        for (int i = 0; i < 3; i++)
        {
            int index = UnityEngine.Random.Range(0, allUpgrades.Count);
            levelUpOptions[i] = allUpgrades[index];
            allUpgrades.RemoveAt(index);
        }
        Debug.Log("Choose one: ");
        Debug.Log("1 - " + levelUpOptions[0]);
        Debug.Log("2 - " + levelUpOptions[1]);
        Debug.Log("3 - " + levelUpOptions[2]);

        if (Upgrade1 != null)
        {
            UpgradeText(Upgrade1, levelUpOptions[0]);
            UpgradeImage(Up1Icon, levelUpOptions[0]);
        }
        if (Upgrade2 != null)
        {
            UpgradeText(Upgrade2, levelUpOptions[1]);
            UpgradeImage(Up2Icon, levelUpOptions[1]);
        }
        if (Upgrade3 != null)
        {
            UpgradeText(Upgrade3, levelUpOptions[2]);
            UpgradeImage(Up3Icon, levelUpOptions[2]);
        }
    }
    private void UpgradeText(TextMeshProUGUI textBox, UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.DmgUp:
                textBox.text = "Upgrade all damage dealt, currently damage is increased by " + dmgUp + "%";
                break;
            case UpgradeType.RangeUp:
                textBox.text = "Upgrade range of attacks, currently range is increased by " + rangeUp + "%";
                break;
            case UpgradeType.CdUp:
                textBox.text = "Upgrade refresh rate of attacks, currently attacks refresh faster by " + cdUp + "%";
                break;
            case UpgradeType.SpeedUp:
                textBox.text = "Upgrade character speed, currently speed is increased by " + speedUp + "%";
                break;
            case UpgradeType.MeleeUp:
                if (meleeUp == 0)
                    textBox.text = "A melee attack which slices in front of the player";
                else
                    textBox.text = "Melee upgrade text... " + meleeUp;
                break;
            case UpgradeType.LightUp:
                if (lightUp == 0)
                    textBox.text = "A lightning attack that arcs and hits up to 3 targets";
                else
                    textBox.text = "Lightning upgrade text... " + lightUp;
                break;
            case UpgradeType.AnvilUp:
                if (anvilUp == 0)
                    textBox.text = "An anvil falls upon a random target, hitting adjacent targets";
                else
                    textBox.text = "Anvil upgrade text... " + anvilUp;
                break;
            case UpgradeType.FireUp:
                if (fireUp == 0)
                    textBox.text = "A breath of fire that shoots toward the closest target";
                else
                    textBox.text = "Fireball upgrade text... " + fireUp;
                break;
            case UpgradeType.AoeUp:
                if (aoeUp == 0)
                    textBox.text = "A circle of fire that periodically hits targets close to the player";
                else
                    textBox.text = "Aoe upgrade text... " + aoeUp;
                break;
        }
        // method to handle all else textboxes
        // or 5 if statements in each...
    }
    private void UpgradeImage(UnityEngine.UI.Image image, UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.DmgUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_0");
                break;
            case UpgradeType.RangeUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_0");
                break;
            case UpgradeType.CdUp: 
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_0");
                break;
            case UpgradeType.SpeedUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_0");
                break;
            case UpgradeType.MeleeUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_2");
                break;
            case UpgradeType.LightUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_4");
                break;
            case UpgradeType.AnvilUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_8");
                break;
            case UpgradeType.FireUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_6");
                break;
            case UpgradeType.AoeUp:
                image.sprite = Resources.Load<Sprite>("Sprites/pxArt_0");
                break;
        }
    }
    private void CheckLevelUpSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ApplyLevelUpOption(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ApplyLevelUpOption(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ApplyLevelUpOption(3);
        }
    }
    private void ApplyLevelUpOption(int option)
    {
        if (option < 1 || option > 3) return;

        UpgradeType chosenUpgrade = levelUpOptions[option - 1];
        switch (chosenUpgrade)
        {
            case UpgradeType.DmgUp: dmgUp += 20; break;
            case UpgradeType.RangeUp: rangeUp += 20; break;
            case UpgradeType.CdUp: cdUp += 10; break;
            case UpgradeType.SpeedUp: speedUp += 20;
                myController.MaxStableMoveSpeed = 10 + speedUp / 10; // 10base
                // call method to update speed
                break;
            case UpgradeType.MeleeUp: meleeUp += 1;
                if (meleeUp == 1)
                    addNewAttacks(0);
                updateAttack(0);
                break;
            case UpgradeType.LightUp: lightUp += 1;
                if ( lightUp == 1)
                    addNewAttacks(1);
                updateAttack(1);
                break;
            case UpgradeType.AnvilUp: anvilUp += 1;
                if (anvilUp == 1)
                    addNewAttacks(2);
                updateAttack(2);
                break;
            case UpgradeType.FireUp: fireUp += 1;
                if (fireUp == 1)
                    addNewAttacks(3);
                updateAttack(3);
                break;
            case UpgradeType.AoeUp: aoeUp += 1;
                if (aoeUp == 1)
                    addNewAttacks(4);
                updateAttack(4);
                break;
        }

        Debug.Log($"Applied upgrade: {chosenUpgrade}");

        Time.timeScale = 1f;
        if (PostProcessingVolumeObject.profile.TryGet<ChromaticAberrationPP>(out var chromaticEffect))
        {
            StartCoroutine(FadeInChromaticAberration(1f, 0f));
        }
        if (levelUpUI != null)
        {
            levelUpUI.SetActive(false);
        }

        isLevelingUp = false;
        UIupdate();
        checkForLevelup();
    }
    private void addNewAttacks(int attackNumber)
    {
        if (attackNumber == 0)
        {
            if (!PlayerCombat.Instance.activeAttacks.Contains(AttackDatabase.Instance.allAttacks[0]))
            {//melee
                PlayerCombat.Instance.AddAttack(AttackDatabase.Instance.allAttacks[0]);
            }
        }
        if (attackNumber == 1)
        {
            if (!PlayerCombat.Instance.activeAttacks.Contains(AttackDatabase.Instance.allAttacks[1]))
            {//lightning
                PlayerCombat.Instance.AddAttack(AttackDatabase.Instance.allAttacks[1]);
            }
        }
        else if (attackNumber == 2)
        {
            if (!PlayerCombat.Instance.activeAttacks.Contains(AttackDatabase.Instance.allAttacks[2]))
            {//anvil
                PlayerCombat.Instance.AddAttack(AttackDatabase.Instance.allAttacks[2]);
            }
        }
        else if (attackNumber == 3)
        {
            if (!PlayerCombat.Instance.activeAttacks.Contains(AttackDatabase.Instance.allAttacks[3]))
            {//fireball
                PlayerCombat.Instance.AddAttack(AttackDatabase.Instance.allAttacks[3]);
            }
        }
        else if (attackNumber == 4)
        {
            if (!PlayerCombat.Instance.activeAttacks.Contains(AttackDatabase.Instance.allAttacks[4]))
            {//aoe
                PlayerCombat.Instance.AddAttack(AttackDatabase.Instance.allAttacks[4]);
            }
        }
    }
    private void updateAttack(int attackNumber)
    {
        switch (attackNumber)
        {
            case 0:
                //melee
                Debug.Log("Upgraded melee attack but it doesn't do much yet");
                break;
            case 1:
                //light
                Debug.Log("Upgraded lightning attack but it doesn't do much yet");
                break;
            case 2:
                //anvil
                Debug.Log("Upgraded anvil attack but it doesn't do much yet");
                break;
            case 3:
                //fire
                Debug.Log("Upgraded fireball attack but it doesn't do much yet");
                break;
            case 4:
                //aoe
                Debug.Log("Upgraded aoe attack but it doesn't do much yet");
                break;
        }
    }
    */
    public void UIupdate()
    {
        enemyBuffer.text = "Buffer " + enemyspawner.spawnBuffer;
        pointScore.text = "Points " + currentPoints;
        slainScore.text = "Slain " + currentSlain;
        //AttackUIManager.Instance.checkForNew();
    }
    void Update()
    {
        /*
        enemyBuffer.text = "Buffer " + enemyspawner.spawnBuffer;
        if (isLevelingUp)
        {
            CheckLevelUpSelection();
        }
        */
    }
}
