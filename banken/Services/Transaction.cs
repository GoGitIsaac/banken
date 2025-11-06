namespace banken.Domain
{
    // Typ av transaktion
    public enum TransactionType
    {
        Deposit,
        Withdraw,
        TransferIn,
        TransferOut
    }

    // Enkel DTO/entitet som representerar en transaktion
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unikt id för transaktionen
        public DateTime TimeStamp { get; set; } // Tidpunkt för transaktionen
        public decimal Amount { get; set; } // Transaktionsbelopp
        public decimal BalanceAfter { get; set; } // Inspelat saldo efter transaktionen
        public Guid? FromAccountId { get; set; } // Avsändarkonto (kan vara null för insättning)
        public Guid ToAccountId { get; set; } // Mottagarkonto
        public TransactionType TransactionType { get; set; } // Typ av transaktion
    }
}
