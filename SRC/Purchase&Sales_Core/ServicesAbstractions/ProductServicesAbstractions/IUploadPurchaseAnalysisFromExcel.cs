using Microsoft.AspNetCore.Http;

namespace Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions
{
    public interface IUploadPurchaseAnalysisFromExcel
    {
        Task<Result<int>> UploadPurchaseData(PurchaseFileMetadataDTO purchaseFileDTO);
    }
}
