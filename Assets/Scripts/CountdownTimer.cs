using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerLabel;

    public void UpdateTimeLeft(float timeLeft)
    {
        timerLabel.text = $"Time left: {timeLeft:0.00}";
    }
}
