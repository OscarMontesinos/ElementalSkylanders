using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HabilityUIIndicator : MonoBehaviour
{
    public Mode mode;
    public Slider slider;
    public TextMeshProUGUI text;
    public GameObject chargesGO;
    public TextMeshProUGUI charges;
    public GameObject content;
    public Image image;

    public enum Mode
    {
        normal, charges
    }

    public void UIUpdate(float maxCD, float cd, int charges)
    {
        if(mode == Mode.normal && charges > 0)
        {
            mode = Mode.charges;
            chargesGO.SetActive(true);
        }
        this.charges.text = charges.ToString();
        slider.maxValue = maxCD;
        slider.value = cd;
        if (cd > 0)
        {
            text.text = cd.ToString("F0");
        }
        else
        {
            text.text = "";
        }
    }

    public void UpdateImage(Sprite image)
    {
        this.image.sprite = image;
    }

    public void Activate(bool value)
    {
        content.SetActive(value);
    }
}
