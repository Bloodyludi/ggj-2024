using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Animator animator;

    private static readonly int ShowHash = Animator.StringToHash("Show");
    private static readonly int Player1Won = Animator.StringToHash("Player1Won");
    private static readonly int Player2Won = Animator.StringToHash("Player2Won");
    private static readonly int Draw = Animator.StringToHash("Draw");

    public void Show(GameResult result)
    {
        animator.SetBool(ShowHash, true);
        animator.SetBool(Player1Won, result == GameResult.Player1Wins);
        animator.SetBool(Player2Won, result == GameResult.Player2Wins);
        animator.SetBool(Draw, result == GameResult.Draw);

        replayButton.Select();
    }

    public void Replay()
    {
        // animator.SetBool(ShowHash, false);
        SceneManager.LoadScene("SampleScene");
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}