using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using banken.Domain;
using banken.Interface;

namespace banken.Services
{
    public class AccountService : IAccountService
    {
        private const string AccountsKey = "accounts";
        private const string TransactionsKey = "transactions";
        private const string StorageKey = "banken.accounts";

        private readonly List<BankAccount> _accounts = new();
        private readonly IStorageService _storageService;
        private readonly List<Transaction> _transactions = new();

        public AccountService(IStorageService storageService) => _storageService = storageService;

        private bool isLoaded;

        private async Task IsInitialized()
        {
            if (isLoaded)
                return;

            var fromStorage = await _storageService.GetItemAsync<List<BankAccount>>(StorageKey);
            _accounts.Clear();

            if (fromStorage is { Count: > 0 })
            {
                _accounts.AddRange(fromStorage
                    .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList());
            }

            // 🔹 NYTT: Läs in transaktioner
            var storedTransactions = await _storageService.GetItemAsync<List<Transaction>>("banken.transactions");
            if (storedTransactions is { Count: > 0 })
            {
                _transactions.Clear();
                _transactions.AddRange(storedTransactions);
            }

            isLoaded = true;
        }

        private Task SaveAsync() => _storageService.SetItemAsync(StorageKey, _accounts);

        private async Task SaveAccounts()
        {
            await _storageService.SetItemAsync("banken.accounts", _accounts);
        }

        public async Task DeleteAccount(string name)
        {
            await IsInitialized();

            // Ta bort konto med matchande namn
            var toRemove = _accounts.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (toRemove is not null)
            {
                _accounts.Remove(toRemove);
                await SaveAsync(); // 🔹 Viktigt! Uppdaterar lagringen
            }
        }

        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, string currency, decimal initialBalance)
        {
            await IsInitialized();

            var account = new BankAccount(name, accountType, currency, initialBalance);

            _accounts.Add(account);
            await SaveAsync();

            return account;
        }

        public async Task<List<IBankAccount>> GetAccounts()
        {
            await IsInitialized();
            return _accounts.Cast<IBankAccount>().ToList();
        }

        public async Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            await IsInitialized();

            var fromAccount = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == fromAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {fromAccountId} not found");

            var toAccount = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == toAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {toAccountId} not found");

            // Utför själva överföringen
            fromAccount.TransferTo(toAccount, amount);

            // 🔹 Endast de senaste två transaktionerna (en in och en ut)
            var latestTransactions = new List<Transaction>
            {
                fromAccount.GetTransactions().Last(),
                toAccount.GetTransactions().Last()
            };

            // Lägg till dem i den globala listan
            _transactions.AddRange(latestTransactions);

            // 🔹 Spara både konton och transaktioner
            await _storageService.SetItemAsync("banken.accounts", _accounts);
            await _storageService.SetItemAsync("banken.transactions", _transactions);
        }

        // Nytt: insättning
        public async Task Deposit(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found");

            account.Deposit(amount); // kan kasta ArgumentException för <= 0

            // Spara senaste transaktionen
            _transactions.Add(account.GetTransactions().Last());

            await _storageService.SetItemAsync("banken.accounts", _accounts);
            await _storageService.SetItemAsync("banken.transactions", _transactions);
        }

        // Nytt: uttag
        public async Task Withdraw(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found");

            account.Withdraw(amount); // kan kasta ArgumentException eller InvalidOperationException

            // Spara senaste transaktionen
            _transactions.Add(account.GetTransactions().Last());

            await _storageService.SetItemAsync("banken.accounts", _accounts);
            await _storageService.SetItemAsync("banken.transactions", _transactions);
        }

        public async Task<List<Transaction>> GetAllTransactions()
        {
            await IsInitialized();

            // 🔹 Försök läsa från local storage först
            var storedTransactions = await _storageService.GetItemAsync<List<Transaction>>("banken.transactions");
            if (storedTransactions != null && storedTransactions.Any())
            {
                return storedTransactions.OrderByDescending(t => t.TimeStamp).ToList();
            }

            // 🔹 Om inget finns i storage, bygg listan som vanligt
            var transactions = _accounts
                .OfType<BankAccount>()
                .SelectMany(a => a.GetTransactions())
                .OrderByDescending(t => t.TimeStamp)
                .ToList();

            // 🔹 Spara till local storage
            await _storageService.SetItemAsync("banken.transactions", transactions);

            return transactions;
        }

        public async Task ClearAllTransactions()
        {
            await IsInitialized();

            _transactions.Clear();

            // Rensa transaktioner i varje konto
            foreach (var account in _accounts.OfType<BankAccount>())
            {
                var privateListField = typeof(BankAccount)
                    .GetField("_transactions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (privateListField?.GetValue(account) is List<Transaction> list)
                    list.Clear();
            }

            // Spara till localStorage
            await _storageService.SetItemAsync("banken.transactions", _transactions);
            await SaveAccounts();
        }
    }
}