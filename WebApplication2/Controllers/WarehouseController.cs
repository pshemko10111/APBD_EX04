using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using WebApplication2.Dto;
using WebApplication2.Exceptions;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterProductInWarehouseAsync([FromBody] RegisterProductInWarehouseRequestDTO dto)
    {
        try
        {
            var idProductWarehouse = await _warehouseService.RegisterProductInWarehouseAsync(dto);
            return Ok(idProductWarehouse);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
    }


    //W Procedurze RAISE ERROR powinno zostać zmienione na THROW co da lepszy efekt obsługi błędów
    [HttpPost("Procedure")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterProductInWarehouseByProcedureAsync([FromBody] RegisterProductInWarehouseRequestDTO dto)
    {
        try
        {
            var idProductWarehouse = await _warehouseService.RegisterProductInWarehouseByProcedureAsync(dto);
            return Ok(idProductWarehouse);
        }
        catch (SqlException e)
        {
            switch (e.Number)
            {
                case 5000:
                    return NotFound(e.Message);
                case 50002:
                    return Conflict(e.Message);
                case 50005:
                    return NotFound(e.Message);
                default:
                    return Conflict(e.Message);
            }
        }
        
    }
}