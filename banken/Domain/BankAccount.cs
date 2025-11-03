using System.Text.Json.Serialization;

namespace banken.Domain
{
    public class BankAccount : IBankAccount
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public AccountType AccountType { get; private set; }
        public string Currency { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime LastUpdated { get; private set; }
        private readonly List<Transaction> _transactions = new();

        // Konstruktor
        public BankAccount(string name, AccountType accountType, string currency, decimal initialBalance)
        {
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = initialBalance;
            LastUpdated = DateTime.UtcNow;
        }
        [JsonConstructor]
        public BankAccount(Guid id, string name, AccountType accountType, string currency, 
            decimal balance, DateTime lastUpdated)
        {
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = balance;
            Id = id;
            LastUpdated = lastUpdated;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds for withdrawal.");

            Balance -= amount;
            LastUpdated = DateTime.UtcNow;
            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.Withdraw,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                ToAccountId = Guid.Empty,
                TimeStamp = DateTime.UtcNow
            });
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            Balance += amount;
            LastUpdated = DateTime.UtcNow;
            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.Deposit,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = null,
                ToAccountId = Id,
                TimeStamp = DateTime.UtcNow
            });
        }

        public void TransferTo(BankAccount toAccount, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds for transfer.");

            // Från vilket konto
            Balance -= amount;
            LastUpdated = DateTime.UtcNow;
            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.TransferOut,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                ToAccountId = toAccount.Id,        // <-- Viktigt: sätt mottagarens Id här
                TimeStamp = DateTime.UtcNow
            });

            // till vilket konto
            toAccount.Balance += amount;
            toAccount.LastUpdated = DateTime.UtcNow;
            toAccount._transactions.Add(new Transaction
            {
                TransactionType = TransactionType.TransferIn,
                Amount = amount,
                BalanceAfter = toAccount.Balance,
                FromAccountId = Id,
                ToAccountId = toAccount.Id,
                TimeStamp = DateTime.UtcNow
            });
        }



        public IReadOnlyList<Transaction> GetTransactions() => _transactions.AsReadOnly();

    }
}
