using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startGameButton;

    private void Start()
    {
        Time.timeScale = 1f;

        startGameButton.Select();
    }

    //Linked through the inspector
    [UsedImplicitly]
    public void OnStartGameButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    [UsedImplicitly]
    public void OnCreditsButtonClicked()
    {
        SceneManager.LoadScene("Credits");
    }

    [UsedImplicitly]
    public void OnHowToButtonClicked()
    {
        SceneManager.LoadScene("HowTo");
    }

    [UsedImplicitly]
    public void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}