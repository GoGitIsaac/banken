namespace banken.Interface
{

    // Interface som beskriver vad ett bankkonto måste exponera
    public interface IBankAccount
    {
       Guid Id { get; } // Unikt id för kontot
       string Name { get; } // Kontots namn
       AccountType AccountType { get; } // Typ av konto (enum)
       string Currency { get; } // Valuta för kontot
       decimal Balance { get; } // Aktuellt saldo
       DateTime LastUpdated { get; } // Senaste uppdateringstidpunkt
       
       void Withdraw(decimal amount); // Uttag från kontot
       void Deposit(decimal amount); // Insättning till kontot
       void TransferTo(BankAccount toAccount, decimal amount); // Överföring till annat konto (konkret typ används här)
    }
}
