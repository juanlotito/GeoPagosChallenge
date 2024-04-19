using Dapper;
using Npgsql;
using PublicApi.Models.Customer;
using PublicApi.Models.Payment;
using PublicApi.Repositories.Interface;
using System.Data;
public class PaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;

    public PaymentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<PaymentRequest> GetPaymentRequestByIdAsync(int id)
    {
        using (var _db = new NpgsqlConnection(_connectionString)) 
        {
            var query = $"SELECT * FROM PaymentRequests WHERE PaymentRequestId = {id}";

            return await _db.QueryFirstOrDefaultAsync<PaymentRequest>(query);
        }       
    }

    public async Task<IEnumerable<ApprovedPayment>> GetAllAuthorizedPaymentsAsync()
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = "SELECT * FROM fn_GetAuthorizedPayments();";

            return await _db.QueryAsync<ApprovedPayment>(query);
        }
    }

    public async Task<bool> GetIsConfirmed(int paymentId)
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = "SELECT fn_IsConfirmed(@PaymentId)";

            var parameters = new DynamicParameters();
            parameters.Add("@PaymentId", paymentId, DbType.Int32);

            var isConfirmed = await _db.QueryFirstOrDefaultAsync<bool>(query, parameters);

            return isConfirmed;
        }

    }

    public async Task<int> AddPaymentRequest(PaymentRequest paymentRequest)
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = @"SELECT fn_AddPaymentRequest(@CustomerId, @Amount, @PaymentTypesId, @IsConfirmed, @StatusId);";

            var parameters = new DynamicParameters();
            parameters.Add("CustomerId", paymentRequest.CustomerId, DbType.Int32);
            parameters.Add("Amount", paymentRequest.Amount, DbType.Decimal);
            parameters.Add("PaymentTypesId", paymentRequest.PaymentTypesId, DbType.Int32);
            parameters.Add("IsConfirmed", paymentRequest.IsConfirmed, DbType.Boolean);
            parameters.Add("StatusId", paymentRequest.StatusId, DbType.Int32);

            int insertedId = await _db.QueryFirstOrDefaultAsync<int>(query, parameters);
            return insertedId;
        }
    }

    public async Task UpdatePaymentStatus(int paymentRequestId, int statusId, string? details)
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = @"SELECT fn_updatepaymentstatus(@PaymentRequestId, @StatusId, @_details);";

            var parameters = new DynamicParameters();
            parameters.Add("PaymentRequestId", paymentRequestId, DbType.Int32);
            parameters.Add("StatusId", statusId, DbType.Int32);
            parameters.Add("_details", details ?? string.Empty, DbType.String);

            await _db.ExecuteAsync(query, parameters);
        }
    }

    public async Task ConfirmPayment(int paymentRequestId) 
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = @"SELECT fn_ConfirmPayment(@payment_request_id)";

            var parameters = new DynamicParameters();
            parameters.Add("payment_request_id", paymentRequestId, DbType.Int32);

            await _db.ExecuteAsync(query, parameters);
        }
    }

    public async Task<Customer> GetClientById(int clientId)
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = @"SELECT * FROM fn_GetClientById(@customer_id);";

            var parameters = new DynamicParameters();
            parameters.Add("customer_id", clientId, DbType.Int32);

            Customer customer = await _db.QueryFirstOrDefaultAsync<Customer>(query, parameters);

            return customer;
        }
    }

    public async Task UpdatePaymentRequest(int paymentRequestId, int newStatusId, bool isConfirmed)
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = "SELECT fn_UpdatePaymentRequest(@PaymentRequestId, @NewStatusId, @IsConfirmed);";
            var parameters = new DynamicParameters(new
            {
                PaymentRequestId = paymentRequestId,
                NewStatusId = newStatusId,
                IsConfirmed = isConfirmed
            });

            await _db.ExecuteAsync(query, parameters);
        }
    }

    public async Task ReversePayment(int paymentRequestId)
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            var query = "SELECT fn_ReversePayment(@PaymentRequestId);";

            var parameters = new DynamicParameters(new { PaymentRequestId = paymentRequestId });

            await _db.ExecuteAsync(query, parameters);
        }
    }

    public async Task AddApprovedPayment(int paymentId)
    {
        using (var _db = new NpgsqlConnection(_connectionString))
        {
            PaymentRequest paymentRequest = await GetPaymentRequestByIdAsync(paymentId);

            var query = "SELECT fn_AddApprovedPayment(@PaymentRequestId, @CustomerId, @Amount, @PaymentTypesId);";
            var parameters = new DynamicParameters(new
            {
                PaymentRequestId = paymentId,
                CustomerId = paymentRequest.CustomerId,
                Amount = paymentRequest.Amount,
                PaymentTypesId = paymentRequest.PaymentTypesId
            });

            await _db.ExecuteAsync(query, parameters);
        }
    }
}
