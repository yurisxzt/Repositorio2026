using System;

public static class PlayerOM
{
    public static Action<int> OnCoinsChanged;

    public static void NotifyCoinsChanged(int quantidade)
    {
        OnCoinsChanged?.Invoke(quantidade);
    }
}