using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private RectTransform flagTemplate;
    [SerializeField] private RectTransform flagContainer;
    //[SerializeField] private RectTransform movingHeadIcon;

    private WaveManager waveManager;
    private List<RectTransform> flagsList = new List<RectTransform>();

    private void Awake()
    {
        if (flagTemplate != null) flagTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();
        
        if (progressSlider != null) 
        {
            progressSlider.value = 0f;
            progressSlider.interactable = false;
        }

        CreateFlags();
    }

    private void CreateFlags()
    {
        if (waveManager == null || flagTemplate == null || flagContainer == null) return;

        int totalWaves = waveManager.GetTotalWaves();
        float width = flagContainer.rect.width;
        
        foreach (var flag in flagsList) Destroy(flag.gameObject);
        flagsList.Clear();

        for (int i = 0; i < totalWaves; i++)
        {
            RectTransform flag = Instantiate(flagTemplate, flagContainer);
            flag.gameObject.SetActive(true);
            
            float normalizedPos = (float)i / totalWaves;
            
            flag.anchorMin = new Vector2(normalizedPos, 0.5f);
            flag.anchorMax = new Vector2(normalizedPos, 0.5f);
            flag.anchoredPosition = Vector2.zero;

            flagsList.Add(flag);
        }
    }

    private void Update()
    {
        if (waveManager == null) return;
        
        float progress = waveManager.GetTimeProgressNormalized();

        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Lerp(progressSlider.value, progress, Time.deltaTime * 5f);
        }

        // if (movingHeadIcon != null && progressSlider != null)
        // {
        //     float sliderWidth = progressSlider.GetComponent<RectTransform>().rect.width;
        //
        //     float xPos = sliderWidth * progressSlider.value;
        //     movingHeadIcon.anchoredPosition = new Vector2(xPos, movingHeadIcon.anchoredPosition.y);
        // }
    }
}