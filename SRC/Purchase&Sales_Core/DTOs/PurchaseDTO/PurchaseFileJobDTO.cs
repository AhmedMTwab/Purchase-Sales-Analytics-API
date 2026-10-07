namespace Purchase_Sales_Core.DTOs.PurchaseDTO;

public class PurchaseFileJobDTO
{
    public string filePath { get; set; } = string.Empty;
    public string productNameHeader { get; set; } = string.Empty;
    public string priceHeader { get; set; } = string.Empty;
    public int headerRow { get; set; }
}
