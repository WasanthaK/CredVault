namespace CredVault.Mobile.Models;

/// <summary>
/// Payment initiation request
/// </summary>
public class PaymentRequestDto
{
    public required string PayerId { get; set; }
    public required string PayeeId { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Payment response DTO
/// </summary>
public class PaymentResponseDto
{
    public required string PaymentId { get; set; }
    public required string TransactionId { get; set; }
    public required string PayerId { get; set; }
    public required string PayeeId { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Payment status query
/// </summary>
public class PaymentStatusDto
{
    public required string PaymentId { get; set; }
    public required string Status { get; set; }
    public bool IsComplete { get; set; }
    public bool IsFailed { get; set; }
    public string? StatusMessage { get; set; }
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Transaction history query
/// </summary>
public class TransactionQueryDto
{
    public string? UserId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Currency { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Transaction history response
/// </summary>
public class TransactionHistoryDto
{
    public required string TransactionId { get; set; }
    public required string PaymentId { get; set; }
    public required string Type { get; set; } // "payment", "refund", "reversal"
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
    public string? Description { get; set; }
    public string? PartyId { get; set; } // Other party (payer or payee)
    public string? PartyName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Payment refund request
/// </summary>
public class RefundRequestDto
{
    public required string PaymentId { get; set; }
    public decimal? Amount { get; set; } // Partial refund if specified
    public string? Reason { get; set; }
}

/// <summary>
/// Payment refund response
/// </summary>
public class RefundResponseDto
{
    public required string RefundId { get; set; }
    public required string PaymentId { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Payment account balance
/// </summary>
public class AccountBalanceDto
{
    public required string AccountId { get; set; }
    public required string UserId { get; set; }
    public required decimal AvailableBalance { get; set; }
    public required decimal PendingBalance { get; set; }
    public required string Currency { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Payment statistics
/// </summary>
public class PaymentStatisticsDto
{
    public required string UserId { get; set; }
    public int TotalPayments { get; set; }
    public int CompletedPayments { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal TotalAmountSent { get; set; }
    public decimal TotalAmountReceived { get; set; }
    public required string Currency { get; set; }
    public Dictionary<string, decimal>? AmountsByStatus { get; set; }
}
