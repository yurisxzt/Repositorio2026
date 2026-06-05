using UnityEngine;

public class MenuPrincipalUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.SetState(GameManager.GameState.MenuPrincipal);
    }

    public void IniciarJogo()
    {
        GameManager.Instance.LoadGameplay();
    }

    public void SairJogo()
    {
        Application.Quit();
    }
}