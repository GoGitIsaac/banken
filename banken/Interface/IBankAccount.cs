namespace banken.Interface
{

    // Interface containing the BankAccount Methods
    public interface IBankAccount
    {
       Guid Id { get; }
       string Name { get; }
       string Currency { get; }
       decimal Balance { get; }
       DateTime LastUpdated { get; }
       
       void Withdraw(decimal amount);
       void Deposit(decimal amount);
    }
}
