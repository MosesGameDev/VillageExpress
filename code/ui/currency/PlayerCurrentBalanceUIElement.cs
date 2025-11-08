using TMPro;
using UnityEngine;

public class PlayerCurrentBalanceUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentBalanceText;


    private void OnEnable()
    {
        BankNoteCurrencyManager.OnCurrentBalanceUpdated += BankNoteCurrencyManager_OnCurrentBalanceUpdated;
    }


    private void BankNoteCurrencyManager_OnCurrentBalanceUpdated(int obj)
    {
        currentBalanceText.SetText($"KSH {obj} /-");
    }

    private void OnDisable()
    {
        BankNoteCurrencyManager.OnCurrentBalanceUpdated -= BankNoteCurrencyManager_OnCurrentBalanceUpdated;
    }

}
