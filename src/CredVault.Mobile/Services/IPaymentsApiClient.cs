using CredVault.Mobile.Models;
using Refit;

namespace CredVault.Mobile.Services;

public interface IPaymentsApiClient
{
    // Payment operations
    [Post("/api/v1/payments")]
    Task<ApiResponseDto<PaymentResponseDto>> InitiatePaymentAsync([Body] PaymentRequestDto request);
    
    [Get("/api/v1/payments/{paymentId}")]
    Task<ApiResponseDto<PaymentResponseDto>> GetPaymentByIdAsync(string paymentId);
    
    [Get("/api/v1/payments/{paymentId}/status")]
    Task<ApiResponseDto<PaymentStatusDto>> GetPaymentStatusAsync(string paymentId);
    
    [Post("/api/v1/payments/{paymentId}/cancel")]
    Task<BooleanApiResponseDto> CancelPaymentAsync(string paymentId);
    
    // Refund operations
    [Post("/api/v1/payments/{paymentId}/refund")]
    Task<ApiResponseDto<RefundResponseDto>> RefundPaymentAsync(string paymentId, [Body] RefundRequestDto request);
    
    [Get("/api/v1/payments/refunds/{refundId}")]
    Task<ApiResponseDto<RefundResponseDto>> GetRefundByIdAsync(string refundId);
    
    // Transaction history
    [Get("/api/v1/payments/transactions")]
    Task<ApiResponseDto<List<TransactionHistoryDto>>> GetTransactionsAsync([Query] TransactionQueryDto? query = null);
    
    [Get("/api/v1/payments/user/{userId}/transactions")]
    Task<ApiResponseDto<List<TransactionHistoryDto>>> GetUserTransactionsAsync(string userId, [Query] int page = 1, [Query] int pageSize = 20);
    
    // Account & Balance
    [Get("/api/v1/payments/account/{accountId}/balance")]
    Task<ApiResponseDto<AccountBalanceDto>> GetAccountBalanceAsync(string accountId);
    
    [Get("/api/v1/payments/user/{userId}/balance")]
    Task<ApiResponseDto<AccountBalanceDto>> GetUserBalanceAsync(string userId);
    
    // Statistics
    [Get("/api/v1/payments/user/{userId}/statistics")]
    Task<ApiResponseDto<PaymentStatisticsDto>> GetPaymentStatisticsAsync(string userId);
    
    // Health check
    [Get("/api/v1/payments/health")]
    Task<GovStackHealthResponse> GetHealthAsync();
}
