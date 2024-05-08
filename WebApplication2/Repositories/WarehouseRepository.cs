using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebApplication2.Exceptions;

namespace WebApplication2.Repositories;

public interface IWarehouseRepository
{
    public Task<int?> RegisterProductInWarehouseAsync(int idProduct, int idWarehouse,  int amount, DateTime createdAt);
    public Task<int?> RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct,  int amount ,DateTime createdAt);
}

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;
    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<int?> RegisterProductInWarehouseAsync(int idProduct, int idWarehouse, int amount, DateTime createdAt)
    {

        using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        using var command = new SqlCommand($"SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount", connection);

        await connection.OpenAsync();

        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@Amount", amount);

        var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows) throw new NotFoundException("No order for this product id and amount");

        await reader.ReadAsync();
        int idOrder = (int)reader["IdOrder"];
        await reader.CloseAsync();



        command.Parameters.Clear();

        command.CommandText = @"SELECT Price FROM [Product] WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", idProduct);

        reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows) throw new NotFoundException("Provided Product Id does not exists");

        await reader.ReadAsync();
        double price = double.Parse(reader["Price"].ToString());
        await reader.CloseAsync();



        command.Parameters.Clear();

        command.CommandText = @"SELECT CreatedAt FROM [Order] WHERE IdOrder = @IdOrder";
        command.Parameters.AddWithValue("@IdOrder", idOrder);

        reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows) throw new NotFoundException("Provided Order does not exists");

        await reader.ReadAsync();
        var creationOrderDate = DateTime.Parse(reader["CreatedAt"].ToString());
        await reader.CloseAsync();

        if (createdAt < creationOrderDate) throw new ConflictException("Creation order date cannot be later than fullfillment date");


        command.Parameters.Clear();

        command.CommandText = @"SELECT IdWarehouse FROM [Warehouse] WHERE IdWarehouse = @IdWarehouse";
        command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);

        reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows) throw new NotFoundException("Provided Warehouse does not exists");

        await reader.ReadAsync();
        await reader.CloseAsync();

        command.Parameters.Clear();

        command.CommandText = @"SELECT * FROM [Product_Warehouse] WHERE IdOrder = @IdOrder";
        command.Parameters.AddWithValue("@IdOrder", idOrder);

        reader = await command.ExecuteReaderAsync();

        if (reader.HasRows) throw new ConflictException("Provided Order has been already fullfilled");

        await reader.ReadAsync();
        await reader.CloseAsync();

        command.Parameters.Clear();


        await using var transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;

        try
        {

            
            command.CommandText = @"UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder AND FulfilledAt is null";
            
            
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.UtcNow);
            var updated = (int)await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();

            if (updated != 1) throw new ConflictException("There is no order/product with this id or the order has been fulfilled");

            command.CommandText = @"
                      INSERT INTO [Product_Warehouse] (IdWarehouse, IdProduct, IdOrder, CreatedAt, Amount, Price)
                      OUTPUT Inserted.IdProductWarehouse
                      VALUES (@IdWarehouse, @IdProduct, @IdOrder, @CreatedAt, @Amount, @Price);";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@Price", (price*amount));
            var idProductWarehouse = (int)await command.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return idProductWarehouse;
        }
        catch (Exception ex) 
        {
            await transaction.RollbackAsync();
            throw ex;
        }
        
    }

    public async Task<int?> RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct, int amount, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        await using var command = new SqlCommand("AddProductToWarehouse", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("IdProduct", idProduct);
        command.Parameters.AddWithValue("IdWarehouse", idWarehouse);
        command.Parameters.AddWithValue("Amount", amount);
        command.Parameters.AddWithValue("CreatedAt", createdAt);

        var idProductWarehouse = await command.ExecuteNonQueryAsync();
        return idProductWarehouse;

        

        

        
    }
}