using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using banken.Domain;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using banken.Domain;

namespace banken.Interface
{
    public interface IAccountService
    {
        Task<IBankAccount> CreateAccount(string name, AccountType accountType, string currency, decimal initialBalance);
        Task<List<IBankAccount>> GetAccounts();
        Task DeleteAccount(string name);
        Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount);

        // Nytt: stöd för insättning och uttag via servicen
        Task Deposit(Guid accountId, decimal amount);
        Task Withdraw(Guid accountId, decimal amount);

        Task<List<Transaction>> GetAllTransactions();
        Task ClearAllTransactions();
    }
}
