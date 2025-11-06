using System.Text.Json.Serialization;

namespace banken.Domain
{
    // Klass som representerar ett bankkonto med grundläggande operationer
    public class BankAccount : IBankAccount
    {
        // Unikt id för kontot, skapas automatiskt vid nyinstansiering
        public Guid Id { get; private set; } = Guid.NewGuid();
        // Kontonamn som kan visas och ändras
        public string Name { get; set; }
        // Typ av konto (t.ex. Savings eller Checking)
        public AccountType AccountType { get; private set; }
        // Valuta som kontot använder (t.ex. SEK eller USD)
        public string Currency { get; private set; }
        // Aktuellt saldo för kontot
        public decimal Balance { get; private set; }
        // Tidpunkt när kontot senast uppdaterades
        public DateTime LastUpdated { get; private set; }
        // Privata listan av transaktioner för kontot
        private readonly List<Transaction> _transactions = new();

        // Konstruktor som anropas när ett nytt konto skapas i appen
        public BankAccount(string name, AccountType accountType, string currency, decimal initialBalance)
        {
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = initialBalance;
            LastUpdated = DateTime.UtcNow; // spara tid i UTC
        }

        // Konstruktor som används av JsonSerializer vid deserialisering
        [JsonConstructor]
        public BankAccount(Guid id, string name, AccountType accountType, string currency, 
            decimal balance, DateTime lastUpdated)
        {
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = balance;
            Id = id; // behåll id som laddats från storage
            LastUpdated = lastUpdated; // använd den inspelade tiden
        }

        // Metod för uttag. Validerar belopp och kastar undantag vid fel
        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds for withdrawal.");

            Balance -= amount; // dra av beloppet
            LastUpdated = DateTime.UtcNow; // uppdatera tid
            // Lägg till transaktion i kontots privata historik
            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.Withdraw,
                Amount = amount,
                BalanceAfter = Balance, // inspelat saldo efter uttag
                FromAccountId = Id,
                ToAccountId = Guid.Empty,
                TimeStamp = DateTime.UtcNow
            });
        }

        // Metod för insättning. Validerar belopp och lägger till transaktion
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            Balance += amount; // lägg till belopp
            LastUpdated = DateTime.UtcNow; // uppdatera tid
            // Lägg till transaktion i kontots privata historik
            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.Deposit,
                Amount = amount,
                BalanceAfter = Balance, // inspelat saldo efter insättning
                FromAccountId = null,
                ToAccountId = Id,
                TimeStamp = DateTime.UtcNow
            });
        }

        // Metod för överföring från detta konto till ett annat konto
        public void TransferTo(BankAccount toAccount, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds for transfer.");

            // Dra pengar från avsändarkontot
            Balance -= amount;
            LastUpdated = DateTime.UtcNow;
            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.TransferOut,
                Amount = amount,
                BalanceAfter = Balance, // inspelat saldo på avsändarkontot efter överföring
                FromAccountId = Id,
                ToAccountId = toAccount.Id,        // viktigt att spara mottagarens id
                TimeStamp = DateTime.UtcNow
            });

            // Sätt in pengar på mottagarkontot
            toAccount.Balance += amount;
            toAccount.LastUpdated = DateTime.UtcNow;
            toAccount._transactions.Add(new Transaction
            {
                TransactionType = TransactionType.TransferIn,
                Amount = amount,
                BalanceAfter = toAccount.Balance, // inspelat saldo på mottagarens konto efter överföring
                FromAccountId = Id,
                ToAccountId = toAccount.Id,
                TimeStamp = DateTime.UtcNow
            });
        }

        // Returnerar en läsbar vy över kontots transaktioner
        public IReadOnlyList<Transaction> GetTransactions() => _transactions.AsReadOnly();

    }
}
