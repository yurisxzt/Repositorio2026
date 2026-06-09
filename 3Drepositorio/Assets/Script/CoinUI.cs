using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI coinText;

    private void OnEnable()
    {
        PlayerObserverManager.OnCoinChanged += UpdateUI;
    }

    private void OnDisable()
    {
        PlayerObserverManager.OnCoinChanged -= UpdateUI;
    }

    private void UpdateUI(int amount)
    {
        coinText.text = "Moedas: " + amount;
    }
}