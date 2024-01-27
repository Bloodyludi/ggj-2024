using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] private InputAction pauseAction;
    [SerializeField] private GameController gameController;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button resumeButton;

    private bool isShowing;
    
    public void Awake()
    {
        pauseAction.performed += PausePressed;
    }

    public void OnEnable()
    {
        pauseAction.Enable();
    }

    public void OnDisable()
    {
        pauseAction.Disable();
    }
    
    private void PausePressed(InputAction.CallbackContext context)
    {
        if (gameController.IsGameOver)
        {
            return;
        }

        if (isShowing)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Show()
    {
        isShowing = true;
        
        gameController.PauseGame(true);

        canvasGroup.alpha = 1f;
        
        resumeButton.Select();
    }
    
    [UsedImplicitly]
    public void Hide()
    {
        gameController.PauseGame(false);
        
        canvasGroup.alpha = 0f;

        isShowing = false;
    }

    [UsedImplicitly]
    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
