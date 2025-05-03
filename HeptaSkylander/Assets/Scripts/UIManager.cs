using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI dmgText;
    public Slider shieldSlider;
    public TextMeshProUGUI shieldText;
    public Slider stunSlider;
    public List<HabilityUIIndicator> habIndicators = new List<HabilityUIIndicator>();

    public GameObject pauseMenu;
    public GameObject habIndicatorsGamepad;
    public GameObject habIndicatorsKeyboard;

    public PjBase ch;

    private void Awake()
    {
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (ch != null)
        {
            UpdateHpBars();
        }

        UpdateHabIndicators();

        
    }

    public void Pause()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = GameManager.Instance.ingameSpeed;
            pauseMenu.SetActive(false); 
            Cursor.visible = false;
        }
        else
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
            Cursor.visible = true;
        }
    }

    void UpdateHpBars()
    {
        hpSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = GetElementColor();

        hpSlider.value = ch.stats.hp;
        hpSlider.maxValue = ch.stats.mHp;
        hpText.text = ch.stats.hp.ToString("F0");

        ch.stunnBar = stunSlider;

        shieldSlider.value = ch.stats.shield;
        shieldSlider.maxValue = ch.stats.mHp*1.5f;
        if (ch.stats.shield > 0)
        {
            shieldText.text = ch.stats.shield.ToString("F0");
        }
        else
        {
            shieldText.text = "";
        }

        stunSlider.value = ch.stunTime;
    }

    Color32 GetElementColor()
    {
        switch (ch.element)
        {
            case HitData.Element.wind:
                return GameManager.Instance.windColor; 
            case HitData.Element.water:
                return GameManager.Instance.waterColor; 
            case HitData.Element.fire:
                return GameManager.Instance.fireColor; 
            case HitData.Element.crystal:
                return GameManager.Instance.crystalColor; 
            case HitData.Element.nature:
                return GameManager.Instance.natureColor; 
            case HitData.Element.desert:
                return GameManager.Instance.desertColor; 
            case HitData.Element.ice:
                return GameManager.Instance.iceColor; 
            case HitData.Element.lightning:
                return GameManager.Instance.lightningColor;
            default:
                return hpSlider.colors.normalColor;
        }
        
    }


    public void UpdateHabIndicatorsImages()
    {
        if (ch.hab1Image)
        {
            habIndicators[0].UpdateImage(ch.hab1Image);
            habIndicators[2].UpdateImage(ch.hab1Image);
        }
        if (ch.hab2Image)
        {
            habIndicators[1].UpdateImage(ch.hab2Image);
            habIndicators[3].UpdateImage(ch.hab2Image);
        }
    }


    void UpdateHabIndicators()
    {
        if (ch.controller)
        {
            if (ch.controller.isUsingGamepad)
            {
                habIndicatorsGamepad.SetActive(true);
                habIndicatorsKeyboard.SetActive(false);
            }
            else
            {
                habIndicatorsKeyboard.SetActive(true);
                habIndicatorsGamepad.SetActive(false);
            }
        }

        if (ch != null && ch.stats.hp > 0)
        {
            habIndicators[0].UIUpdate(ch.hab1Cd, ch.currentHab1Cd);
            habIndicators[2].UIUpdate(ch.hab1Cd, ch.currentHab1Cd);
            habIndicators[1].UIUpdate(ch.hab2Cd, ch.currentHab2Cd);
            habIndicators[3].UIUpdate(ch.hab2Cd, ch.currentHab2Cd);
        }
    }

    public void UpdateDamageText()
    {
        dmgText.text = ch.dmgDealed.ToString("F0");
    }
}
