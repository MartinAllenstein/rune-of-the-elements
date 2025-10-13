using System;
using UnityEngine;
using UnityEngine.UI;

public class BaseUI : MonoBehaviour
{
    [SerializeField] private Image healthBarImage;

    private void Start()
    {
        if (TheBase.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        TheBase.Instance.OnHealthChanged += TheBase_OnHealthChanged;

        healthBarImage.fillAmount = 1f;
    }

    private void TheBase_OnHealthChanged(object sender, EventArgs e)
    {
        // HP UI
        healthBarImage.fillAmount = TheBase.Instance.GetHealthNormalized();
    }

    private void OnDestroy()
    {
        if (TheBase.Instance != null)
        {
            TheBase.Instance.OnHealthChanged -= TheBase_OnHealthChanged;
        }
    }
}