using Purchase_Sales_Core.DTOs.PurchaseDTO;

namespace Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions
{
    public interface IUploadPurchaseAnalysisFromCsv
    {
        Task<Result<int>> UploadPurchaseData(PurchaseFileJobDTO purchaseFileDTO);
    }
}
