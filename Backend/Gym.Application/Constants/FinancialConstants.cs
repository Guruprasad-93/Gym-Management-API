namespace Gym.Application.Constants;

public static class PayrollStatuses
{
    public const string Draft = "Draft";
    public const string Approved = "Approved";
    public const string Paid = "Paid";

    public static readonly IReadOnlyList<string> All = [Draft, Approved, Paid];
}

public static class EmployeeTypes
{
    public const string Trainer = "Trainer";
    public const string GymAdmin = "GymAdmin";

    public static readonly IReadOnlyList<string> All = [Trainer, GymAdmin];
}

public static class ExpensePaymentMethods
{
    public const string Cash = "Cash";
    public const string Card = "Card";
    public const string Upi = "UPI";
    public const string BankTransfer = "BankTransfer";
    public const string Other = "Other";

    public static readonly IReadOnlyList<string> All = [Cash, Card, Upi, BankTransfer, Other];
}
