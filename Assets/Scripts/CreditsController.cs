using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Start()
    {
        backButton.Select();
    }
    
    [UsedImplicitly]
    public void OnReturnToMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
