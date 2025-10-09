namespace banken.Domain
{
    public class BankAccount : IBankAccount
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public string Currency { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public void Withdraw(decimal amount)
        {
            throw new NotImplementedException();
        }
        public void Deposit(decimal amount)
        {
            throw new NotImplementedException();
        }
    }
}
