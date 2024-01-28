using TMPro;
using UnityEngine;

public class ComboCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text comboLabel;

    public void UpdateComboCount(int count)
    {
        comboLabel.text = $"Combo: {count:00}";
    }
}
