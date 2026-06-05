using UnityEngine;

public class PlayerCoins : MonoBehaviour
{
    private int moedas = 0;

    public void AddCoin()
    {
        moedas++;

        Debug.Log("Moedas: " + moedas);

        PlayerOM.NotifyCoinsChanged(moedas);
    }
}