using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using banken.Domain;
using banken.Interface;

namespace banken.Services
{
    // Service som hanterar konton och håller dem i minnet + i localStorage via IStorageService
    public class AccountService : IAccountService
    {
        // Nycklar för localStorage/lagring
        private const string AccountsKey = "accounts";
        private const string TransactionsKey = "transactions";
        private const string StorageKey = "banken.accounts";

        // Intern lista med BankAccount (konkreta typer)
        private readonly List<BankAccount> _accounts = new();
        // Abstraktion mot lagring (t.ex. localStorage i Blazor WASM)
        private readonly IStorageService _storageService;
        // Global lista med transaktioner för snabb åtkomst
        private readonly List<Transaction> _transactions = new();

        // Konstruktor med injicerad storage-tjänst
        public AccountService(IStorageService storageService) => _storageService = storageService;

        private bool isLoaded; // flagga som signalerar att data redan laddats

        // Initieringsmetod som säkerställer att _accounts och _transactions är laddade från lagring
        private async Task IsInitialized()
        {
            if (isLoaded)
                return; // redan initierad

            // Försök läsa konton från storage
            var fromStorage = await _storageService.GetItemAsync<List<BankAccount>>(StorageKey);
            _accounts.Clear();

            if (fromStorage is { Count: > 0 })
            {
                // Ta bort dubbletter baserat på namn (case-insensitive) och lägg till i intern lista
                _accounts.AddRange(fromStorage
                    .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList());
            }

            // Läs in eventuella sparade transaktioner
            var storedTransactions = await _storageService.GetItemAsync<List<Transaction>>("banken.transactions");
            if (storedTransactions is { Count: > 0 })
            {
                _transactions.Clear();
                _transactions.AddRange(storedTransactions);
            }

            isLoaded = true; // markera som initierad
        }

        // Hjälpmetod för att spara konton
        private Task SaveAsync() => _storageService.SetItemAsync(StorageKey, _accounts);

        private async Task SaveAccounts()
        {
            await _storageService.SetItemAsync("banken.accounts", _accounts);
        }

        // Tar bort ett konto med matchande namn
        public async Task DeleteAccount(string name)
        {
            await IsInitialized();

            var toRemove = _accounts.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (toRemove is not null)
            {
                _accounts.Remove(toRemove);
                await SaveAsync(); // spara uppdaterad lista
            }
        }

        // Skapar ett nytt konto och sparar det
        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, string currency, decimal initialBalance)
        {
            await IsInitialized();

            var account = new BankAccount(name, accountType, currency, initialBalance);

            _accounts.Add(account);
            await SaveAsync();

            return account;
        }

        // Returnerar listan med konton (som IBankAccount)
        public async Task<List<IBankAccount>> GetAccounts()
        {
            await IsInitialized();
            return _accounts.Cast<IBankAccount>().ToList();
        }

        // Utför en överföring mellan två konton
        public async Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            await IsInitialized();

            var fromAccount = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == fromAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {fromAccountId} not found");

            var toAccount = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == toAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {toAccountId} not found");

            // Utför själva överföringen i domänobjektet
            fromAccount.TransferTo(toAccount, amount);

            // Hämta de två senaste transaktionerna (en ut och en in)
            var latestTransactions = new List<Transaction>
            {
                fromAccount.GetTransactions().Last(),
                toAccount.GetTransactions().Last()
            };

            // Lägg till dem i den globala transaktionslistan
            _transactions.AddRange(latestTransactions);

            // Spara både konton och transaktioner i storage
            await _storageService.SetItemAsync("banken.accounts", _accounts);
            await _storageService.SetItemAsync("banken.transactions", _transactions);
        }

        // Insättning via service
        public async Task Deposit(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found");

            account.Deposit(amount);

            // Spara senaste transaktionen
            _transactions.Add(account.GetTransactions().Last());

            await _storageService.SetItemAsync("banken.accounts", _accounts);
            await _storageService.SetItemAsync("banken.transactions", _transactions);
        }

        // Uttag via service
        public async Task Withdraw(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(x => x.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found");

            account.Withdraw(amount); 

            // Spara senaste transaktionen
            _transactions.Add(account.GetTransactions().Last());

            await _storageService.SetItemAsync("banken.accounts", _accounts);
            await _storageService.SetItemAsync("banken.transactions", _transactions);
        }

        // Hämta alla transaktioner (först från storage, annars bygg från konton)
        public async Task<List<Transaction>> GetAllTransactions()
        {
            await IsInitialized();

            var storedTransactions = await _storageService.GetItemAsync<List<Transaction>>("banken.transactions");
            if (storedTransactions != null && storedTransactions.Any())
            {
                return storedTransactions.OrderByDescending(t => t.TimeStamp).ToList();
            }

            var transactions = _accounts
                .OfType<BankAccount>()
                .SelectMany(a => a.GetTransactions())
                .OrderByDescending(t => t.TimeStamp)
                .ToList();

            await _storageService.SetItemAsync("banken.transactions", transactions);

            return transactions;
        }

        // Rensa alla transaktioner både globalt och i varje konto
        public async Task ClearAllTransactions()
        {
            await IsInitialized();

            _transactions.Clear();

            foreach (var account in _accounts.OfType<BankAccount>())
            {
                var privateListField = typeof(BankAccount)
                    .GetField("_transactions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (privateListField?.GetValue(account) is List<Transaction> list)
                    list.Clear();
            }

            await _storageService.SetItemAsync("banken.transactions", _transactions);
            await SaveAccounts();
        }
    }
}