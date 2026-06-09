using UnityEngine;

public class PlayerCoins : MonoBehaviour
{
    private int coins;

    private void OnEnable()
    {
        PlayerObserverManager.OnCoinCollected += CollectCoin;
    }

    private void OnDisable()
    {
        PlayerObserverManager.OnCoinCollected -= CollectCoin;
    }

    private void CollectCoin()
    {
        coins++;

        PlayerObserverManager.NotifyCoinChanged(coins);
    }
}