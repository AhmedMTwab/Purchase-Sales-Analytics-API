using Microsoft.AspNetCore.Mvc;
using Purchase_Sales_Core;
using Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions;

namespace Purchase_Sales_API.Controllers
{
    [Route("api/upload/[controller]")]
    [ApiController]
    public class PurchaseController(
        IUploadPurchaseAnalysisFromExcel _uploadPurchaseAnalysisFromExcel,
        IUploadPurchaseAnalysisFromCsv _uploadPurchaseAnalysisFromCsv) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadPurchases([FromForm] PurchaseFileMetadataDTO purchaseFileDTO)
        {
            var ext = Path.GetExtension(purchaseFileDTO.purchaseFile?.FileName ?? string.Empty)
                          .ToLowerInvariant();

            Result<int> result = ext switch
            {
                ".csv"  => await _uploadPurchaseAnalysisFromCsv.UploadPurchaseData(purchaseFileDTO),
                ".xlsx" => await _uploadPurchaseAnalysisFromExcel.UploadPurchaseData(purchaseFileDTO),
                _       => Result<int>.Fail(ErrorType.Invalid,
                               "Unsupported file format. File must be .csv or .xlsx.")
            };

            if (result.IsSuccess)
                return Ok($"{result.Value} Products added");

            return result.ToActionResult(this);
        }
    }
}
