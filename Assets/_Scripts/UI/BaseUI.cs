using System;
using UnityEngine;
using UnityEngine.UI;

public class BaseUI : MonoBehaviour
{
    [SerializeField] private Image healthBarImage;
    [SerializeField] private TheBase targetBase;

    private void Start()
    {
        if (targetBase == null)
        {
            gameObject.SetActive(false);
            return;
        }

        targetBase.OnHealthChanged += TheBase_OnHealthChanged;

        healthBarImage.fillAmount = targetBase.GetHealthNormalized();
    }

    private void TheBase_OnHealthChanged(object sender, EventArgs e)
    {
        // HP UI
        healthBarImage.fillAmount = targetBase.GetHealthNormalized();
    }

    private void OnDestroy()
    {
        if (targetBase != null)
        {
            targetBase.OnHealthChanged -= TheBase_OnHealthChanged;
        }
    }
}