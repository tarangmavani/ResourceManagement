namespace ResourceManagement.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int ItemCount { get; set; }
}

public class CreateProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}

public class UpdateProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
}
