﻿namespace WebApplication2.Models;

public class ProductWarehouse
{
    public int IdProductWarehouse { get; set; }
    public int IdWarehouse { get; set; }
    public int IdProduct { get; set; }
    public int IdOrder { get; set; }
    public int Amount { get; set; }
    public float Price { get; set; }
    public DateTime CreatedAt { get; set; }
}