namespace Purchase_Sales_Core.DTOs.SaleDTO;

public class SalesFileJobDTO
{
    public string filePath { get; set; } = string.Empty;
    public int headerRow { get; set; }
    public string productNameHeader { get; set; } = string.Empty;
    public string quantityHeader { get; set; } = string.Empty;
    public string priceHeader { get; set; } = string.Empty;
}
