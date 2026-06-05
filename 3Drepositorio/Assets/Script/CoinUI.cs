using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textoMoedas;

    private void OnEnable()
    {
        PlayerOM.OnCoinsChanged += AtualizarTexto;
    }

    private void OnDisable()
    {
        PlayerOM.OnCoinsChanged -= AtualizarTexto;
    }

    private void AtualizarTexto(int moedas)
    {
        textoMoedas.text = "Moedas: " + moedas;
    }
}