using System;

public static class PlayerObserverManager
{
    public static Action OnCoinCollected;

    public static Action<int> OnCoinChanged;

    public static void NotifyCoinCollected()
    {
        OnCoinCollected?.Invoke();
    }

    public static void NotifyCoinChanged(int amount)
    {
        OnCoinChanged?.Invoke(amount);
    }
}