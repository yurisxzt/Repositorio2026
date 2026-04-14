using UnityEngine;

public class SplashController : MonoBehaviour
{
    private void Start()
    {
        Invoke("IrParaMenu", 2f); // espera 2 segundos
    }

    void IrParaMenu()
    {
        GameManager.Instance.LoadScene("MenuPrincipal");
    }
}