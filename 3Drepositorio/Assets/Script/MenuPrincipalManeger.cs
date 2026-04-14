using UnityEngine;

public class MenuPrincipalUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.SetState(GameManager.GameState.MenuPrincipal);
    }

    public void IniciarJogo()
    {
        GameManager.Instance.LoadScene("SampleScene");
    }

    public void SairJogo()
    {
        Application.Quit();
    }
}
