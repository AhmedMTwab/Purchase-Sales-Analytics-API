using Microsoft.AspNetCore.Http;

namespace Purchase_Sales_Core.ServicesAbstractions.SaleServicesAbstractions
{
    public interface IUploadSaleAnalysisFromCsv
    {
        Task<Result<int>> UploadSaleData(SalesFileMetadataDTO saleFileDTO);
    }
}
