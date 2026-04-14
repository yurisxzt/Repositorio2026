using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Iniciando,
        MenuPrincipal,
        Gameplay
    }

    public GameState currentState;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetState(GameState.Iniciando);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log("Estado atual: " + currentState);
    }

    // Controle de cenas (SÓ o GameManager pode usar isso)
    public void LoadScene(string sceneName)
    {
        if (PodeTrocarCena())
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private bool PodeTrocarCena()
    {
        // Você pode colocar regras aqui depois
        return true;
    }

    // Alocação de input (simplificado)
    public void SetupPlayerInput(PlayerInput player)
    {
        player.SwitchCurrentControlScheme("Keyboard&Mouse");
    }
}
