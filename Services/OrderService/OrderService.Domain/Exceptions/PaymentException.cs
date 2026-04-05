namespace OrderService.Domain.Exceptions;

public class PaymentException : Exception
{
    public PaymentException(string message)
        : base(message)
    {
    }
}

public sealed class PaymentAlreadyProcessedException : PaymentException
{
    public PaymentAlreadyProcessedException(Guid paymentId)
        : base($"Payment '{paymentId}' is already processed and cannot be modified.")
    {
    }
}

public sealed class InvalidRefundAmountException : PaymentException
{
    public InvalidRefundAmountException(decimal refundAmount, decimal originalAmount)
        : base($"Refund amount '{refundAmount}' exceeds original payment amount '{originalAmount}' or is invalid.")
    {
    }
}