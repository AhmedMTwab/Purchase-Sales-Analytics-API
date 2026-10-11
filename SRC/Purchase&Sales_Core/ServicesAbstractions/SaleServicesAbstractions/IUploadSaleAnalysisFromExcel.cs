using Purchase_Sales_Core.DTOs.SaleDTO;

namespace Purchase_Sales_Core.ServicesAbstractions.SaleServicesAbstractions
{
    public interface IUploadSaleAnalysisFromExcel
    {
        Task<Result<int>> UploadSaleData(SalesFileJobDTO saleFileDTO);
    }
}
