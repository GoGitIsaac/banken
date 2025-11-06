using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using banken.Domain;

namespace banken.Interface
{
    // Interface som beskriver de operationer AccountService ska erbjuda
    public interface IAccountService
    {
        // Skapar ett nytt konto med angivet namn, typ, valuta och startbelopp
        Task<IBankAccount> CreateAccount(string name, AccountType accountType, string currency, decimal initialBalance);

        // Hämtar en lista med alla konton (som IBankAccount)
        Task<List<IBankAccount>> GetAccounts();

        // Tar bort ett konto baserat på namn
        Task DeleteAccount(string name);

        // Överför ett belopp mellan två konton (identifierade med deras Guid)
        Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount);

        // Insättning via servicen
        Task Deposit(Guid accountId, decimal amount);

        // Uttag via servicen
        Task Withdraw(Guid accountId, decimal amount);

        // Hämta alla globalt sparade transaktioner
        Task<List<Transaction>> GetAllTransactions();

        // Rensa alla transaktioner (globala + i konton)
        Task ClearAllTransactions();
    }
}
