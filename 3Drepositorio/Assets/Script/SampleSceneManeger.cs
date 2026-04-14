using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayManager : MonoBehaviour
{
    public PlayerInput playerInput;

    private void Start()
    {
        GameManager.Instance.SetState(GameManager.GameState.Gameplay);

        GameManager.Instance.SetupPlayerInput(playerInput);
    }
}