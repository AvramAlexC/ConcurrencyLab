namespace ConcurrencyLab.Services;

/// <summary>
/// Demonstrates thread-safe balance management using a lock.
/// Used by shared-state endpoints as a practical example.
/// </summary>
public class BankAccountService
{
    private decimal _balance = 1000m;
    private readonly object _lock = new();

    public decimal Balance
    {
        get { lock (_lock) return _balance; }
    }

    public bool Transfer(decimal amount)
    {
        lock (_lock)
        {
            if (_balance + amount < 0)
                return false;

            _balance += amount;
            return true;
        }
    }
}
