using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Config : MonoBehaviour
{
    [Header("Refs UI")]
    [SerializeField] private GameObject sensitivityPanel;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;

    [Header("Gameplay refs")]
    [SerializeField] private Jogador jogador;

    private const string PREF_KEY = "MouseSensitivity";

    void Awake()
    {
        if (sensitivityPanel != null)
            sensitivityPanel.SetActive(false);
    }

    void Start()
    {
        float saved = PlayerPrefs.GetFloat(PREF_KEY, jogador != null ? jogador.mouseSensitivity : 5f);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 20f;

            sensitivitySlider.value = saved;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (jogador != null)
            jogador.mouseSensitivity = saved;

        UpdateValueText(saved);
    }

    private void OnSensitivityChanged(float value)
    {
        if (jogador != null)
            jogador.mouseSensitivity = value;

        PlayerPrefs.SetFloat(PREF_KEY, value);
        PlayerPrefs.Save();

        UpdateValueText(value);
    }

    private void UpdateValueText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = $"Sensibilidade: {value:F1}";
    }

    public void ShowSensitivityPanel()
    {
        if (sensitivityPanel != null)
        {
            sensitivityPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void HideSensitivityPanel()
    {
        if (sensitivityPanel != null)
        {
            sensitivityPanel.SetActive(false);
        }
    }
}
