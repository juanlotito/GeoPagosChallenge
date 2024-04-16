using Dapper;
using PublicApi.Models.Customer;
using PublicApi.Models.Payment;
using PublicApi.Repositories.Interface;
using System.Data;
public class PaymentRepository : IPaymentRepository
{
    private readonly IDbConnection _db;

    public PaymentRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<PaymentRequest> GetPaymentRequestByIdAsync(int id)
    {
        var query = "SELECT * FROM PaymentRequests WHERE Id = @Id";
        return await _db.QueryFirstOrDefaultAsync<PaymentRequest>(query, new { Id = id });
    }
    public async Task<IEnumerable<ApprovedPayment>> GetAllAuthorizedPaymentsAsync()
    {
        var query = "SELECT * FROM fn_GetAuthorizedPayments();";
        return await _db.QueryAsync<ApprovedPayment>(query);
    }

    public async Task<int> AddPaymentRequest(PaymentRequest paymentRequest)
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

    public async Task UpdatePaymentStatus(int paymentRequestId, int statusId)
    {
        var query = @"SELECT fn_UpdatePaymentStatus(@PaymentRequestId, @StatusId);";

        var parameters = new DynamicParameters();
        parameters.Add("PaymentRequestId", paymentRequestId, DbType.Int32);
        parameters.Add("StatusId", statusId, DbType.Int32);

        await _db.ExecuteAsync(query, parameters);
    }

    public async Task ApprovePayment(int paymentRequestId) 
    {
        var query = @"SELECT fn_UpdatePaymentStatus(@PaymentRequestId, 1);";

        var parameters = new DynamicParameters();
        parameters.Add("PaymentRequestId", paymentRequestId, DbType.Int32);

        await _db.ExecuteAsync(query, parameters);
    }

    public async Task<Customer> GetClientById(int clientId)
    {
        var query = @"SELECT * FROM fn_GetClientById(@customer_id);";

        var parameters = new DynamicParameters();
        parameters.Add("customer_id", clientId, DbType.Int32);

        Customer customer = await _db.QueryFirstOrDefaultAsync<Customer>(query, parameters);

        return customer;
    }

    public async Task UpdatePaymentRequest(int paymentRequestId, int newStatusId, bool isConfirmed)
    {
        var query = "SELECT fn_UpdatePaymentRequest(@PaymentRequestId, @NewStatusId, @IsConfirmed);";
        var parameters = new DynamicParameters(new
        {
            PaymentRequestId = paymentRequestId,
            NewStatusId = newStatusId,
            IsConfirmed = isConfirmed
        });

        using (var connection = _db)
        {
            await connection.ExecuteAsync(query, parameters);
        }
    }

    public async Task ReversePayment(int paymentRequestId)
    {
        var query = "SELECT fn_ReversePayment(@PaymentRequestId);";
        var parameters = new DynamicParameters(new { PaymentRequestId = paymentRequestId });

        using (var connection = _db)
        {
            await connection.ExecuteAsync(query, parameters);
        }
    }

}