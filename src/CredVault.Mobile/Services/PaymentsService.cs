using CredVault.Mobile.Models;
using Microsoft.Extensions.Logging;
using Refit;
using System.Net;

namespace CredVault.Mobile.Services;

public class PaymentsService
{
    private readonly IPaymentsApiClient _apiClient;
    private readonly ILogger<PaymentsService> _logger;

    public PaymentsService(IPaymentsApiClient apiClient, ILogger<PaymentsService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServiceResult<PaymentResponseDto>> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<PaymentResponseDto>.Failure("Payment request cannot be null");

            _logger.LogInformation("Initiating payment: {Amount} {Currency}", request.Amount, request.Currency);
            var response = await _apiClient.InitiatePaymentAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully initiated payment: {PaymentId}", response.Data.PaymentId);
                return ServiceResult<PaymentResponseDto>.Success(response.Data);
            }

            return ServiceResult<PaymentResponseDto>.Failure(response.Message ?? "Failed to initiate payment", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error initiating payment: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<PaymentResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error initiating payment");
            return ServiceResult<PaymentResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<PaymentResponseDto>> GetPaymentByIdAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                return ServiceResult<PaymentResponseDto>.Failure("Payment ID cannot be empty");

            var response = await _apiClient.GetPaymentByIdAsync(paymentId);
            
            if (response.Success && response.Data != null)
                return ServiceResult<PaymentResponseDto>.Success(response.Data);

            return ServiceResult<PaymentResponseDto>.Failure(response.Message ?? "Payment not found", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.NotFound)
        {
            return ServiceResult<PaymentResponseDto>.Failure("Payment not found");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching payment: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<PaymentResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching payment");
            return ServiceResult<PaymentResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<PaymentStatusDto>> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                return ServiceResult<PaymentStatusDto>.Failure("Payment ID cannot be empty");

            var response = await _apiClient.GetPaymentStatusAsync(paymentId);
            
            if (response.Success && response.Data != null)
                return ServiceResult<PaymentStatusDto>.Success(response.Data);

            return ServiceResult<PaymentStatusDto>.Failure(response.Message ?? "Failed to fetch status", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching payment status: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<PaymentStatusDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching payment status");
            return ServiceResult<PaymentStatusDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<bool>> CancelPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                return ServiceResult<bool>.Failure("Payment ID cannot be empty");

            _logger.LogInformation("Cancelling payment: {PaymentId}", paymentId);
            var response = await _apiClient.CancelPaymentAsync(paymentId);
            
            if (response.Success)
                return ServiceResult<bool>.Success(response.Data);

            return ServiceResult<bool>.Failure(response.Message ?? "Failed to cancel payment", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error cancelling payment: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error cancelling payment");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<RefundResponseDto>> RefundPaymentAsync(string paymentId, RefundRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                return ServiceResult<RefundResponseDto>.Failure("Payment ID cannot be empty");
            if (request == null)
                return ServiceResult<RefundResponseDto>.Failure("Refund request cannot be null");

            _logger.LogInformation("Processing refund for payment: {PaymentId}", paymentId);
            var response = await _apiClient.RefundPaymentAsync(paymentId, request);
            
            if (response.Success && response.Data != null)
                return ServiceResult<RefundResponseDto>.Success(response.Data);

            return ServiceResult<RefundResponseDto>.Failure(response.Message ?? "Failed to process refund", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error processing refund: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<RefundResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing refund");
            return ServiceResult<RefundResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<List<TransactionHistoryDto>>> GetTransactionsAsync(TransactionQueryDto? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetTransactionsAsync(query);
            
            if (response.Success && response.Data != null)
                return ServiceResult<List<TransactionHistoryDto>>.Success(response.Data);

            return ServiceResult<List<TransactionHistoryDto>>.Failure(response.Message ?? "Failed to fetch transactions", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching transactions: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<TransactionHistoryDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching transactions");
            return ServiceResult<List<TransactionHistoryDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<List<TransactionHistoryDto>>> GetUserTransactionsAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<List<TransactionHistoryDto>>.Failure("User ID cannot be empty");

            var response = await _apiClient.GetUserTransactionsAsync(userId, page, pageSize);
            
            if (response.Success && response.Data != null)
                return ServiceResult<List<TransactionHistoryDto>>.Success(response.Data);

            return ServiceResult<List<TransactionHistoryDto>>.Failure(response.Message ?? "Failed to fetch user transactions", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching user transactions: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<TransactionHistoryDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user transactions");
            return ServiceResult<List<TransactionHistoryDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<AccountBalanceDto>> GetAccountBalanceAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return ServiceResult<AccountBalanceDto>.Failure("Account ID cannot be empty");

            var response = await _apiClient.GetAccountBalanceAsync(accountId);
            
            if (response.Success && response.Data != null)
                return ServiceResult<AccountBalanceDto>.Success(response.Data);

            return ServiceResult<AccountBalanceDto>.Failure(response.Message ?? "Failed to fetch balance", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching balance: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<AccountBalanceDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching balance");
            return ServiceResult<AccountBalanceDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<PaymentStatisticsDto>> GetPaymentStatisticsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<PaymentStatisticsDto>.Failure("User ID cannot be empty");

            var response = await _apiClient.GetPaymentStatisticsAsync(userId);
            
            if (response.Success && response.Data != null)
                return ServiceResult<PaymentStatisticsDto>.Success(response.Data);

            return ServiceResult<PaymentStatisticsDto>.Failure(response.Message ?? "Failed to fetch statistics", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching statistics: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<PaymentStatisticsDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching statistics");
            return ServiceResult<PaymentStatisticsDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<GovStackHealthResponse>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetHealthAsync();
            
            if (response != null)
                return ServiceResult<GovStackHealthResponse>.Success(response);

            return ServiceResult<GovStackHealthResponse>.Failure("Failed to get health status");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error checking health: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<GovStackHealthResponse>.Failure($"API error: {apiEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking health");
            return ServiceResult<GovStackHealthResponse>.Failure("An unexpected error occurred");
        }
    }

    private string[]? GetErrorDetails(ApiException apiEx)
    {
        if (string.IsNullOrWhiteSpace(apiEx.Content))
            return null;

        try
        {
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<GovStackErrorResponse>(apiEx.Content);
            if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.ErrorDescription))
                return new[] { errorResponse.ErrorDescription };
        }
        catch
        {
            return new[] { apiEx.Content };
        }

        return null;
    }
}
