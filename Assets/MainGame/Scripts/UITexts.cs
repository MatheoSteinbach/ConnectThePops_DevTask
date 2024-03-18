using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITexts : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private Slider slider;

    private void Start()
    {
        scoreText.text = "";
        multiplierText.text = "";
        levelText.text = "1";
        nextLevelText.text = "2";
        slider.value = 0;
    }
    public void UpdateUI(int _score, int _multiplier, int _level, int _progress)
    {
        scoreText.text = _score.ToString();
        multiplierText.text = "x"+_multiplier.ToString();
        levelText.text = _level.ToString();
        var x = _level + 1;
        nextLevelText.text = x.ToString();
        slider.value = _progress;

    }
}
