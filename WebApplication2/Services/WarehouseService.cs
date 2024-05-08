using WebApplication2.Dto;
using WebApplication2.Exceptions;
using WebApplication2.Repositories;

namespace WebApplication2.Services;

public interface IWarehouseService
{
    public Task<int> RegisterProductInWarehouseAsync(RegisterProductInWarehouseRequestDTO dto);
    public Task<int> RegisterProductInWarehouseByProcedureAsync(RegisterProductInWarehouseRequestDTO dto);
}

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    // private readonly IProductRepository _productRepository;
    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }
    
    public async Task<int> RegisterProductInWarehouseAsync(RegisterProductInWarehouseRequestDTO dto)
    {
        // Example Flow:
        // check if product exists else throw NotFoundException
        // if (_productRepository.GetProductByIdAsync(dto.IdProduct) is null)
        //    throw new NotFoundException("adsasdasd");
        // check if warehouse exists else throw NotFoundException
        // get order if exists else throw NotFoundException
        // check if product is already in warehouse else throw ConflictException

        var idProductWarehouse = await _warehouseRepository.RegisterProductInWarehouseAsync(
            idProduct: dto.IdProduct!.Value,
            idWarehouse: dto.IdWarehouse!.Value,
            amount: dto.amount!.Value,
            createdAt: DateTime.UtcNow);

        if (!idProductWarehouse.HasValue)
            throw new Exception("Failed to register product in warehouse");

        return idProductWarehouse.Value;
    }

    public async Task<int> RegisterProductInWarehouseByProcedureAsync(RegisterProductInWarehouseRequestDTO dto)
    {
        var idProductWarehouse = await _warehouseRepository.RegisterProductInWarehouseByProcedureAsync(
            idProduct: dto.IdProduct!.Value,
            idWarehouse: dto.IdWarehouse!.Value,
            amount: dto.amount!.Value,
            createdAt: DateTime.UtcNow);

        if (!idProductWarehouse.HasValue)
            throw new Exception("Failed to register product in warehouse");

        return idProductWarehouse.Value;


    }
}