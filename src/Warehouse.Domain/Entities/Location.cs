namespace Warehouse.Domain.Entities;

public class Location
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public CWarehouse Warehouse { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}