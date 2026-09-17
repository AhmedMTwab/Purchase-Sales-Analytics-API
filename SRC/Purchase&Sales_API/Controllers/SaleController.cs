using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Purchase_Sales_Core;
using Purchase_Sales_Core.ServicesAbstractions.SaleServicesAbstractions;

namespace Purchase_Sales_API.Controllers
{
    [Route("api/upload/[controller]")]
    [ApiController]
    public class SaleController(
        IUploadSaleAnalysisFromExcel _uploadSaleAnalysisFromExcel,
        IUploadSaleAnalysisFromCsv _uploadSaleAnalysisFromCsv) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadSales([FromForm] SalesFileMetadataDTO salesFileDTO)
        {
            var timer = Stopwatch.StartNew();

            var ext = Path.GetExtension(salesFileDTO.salesFile?.FileName ?? string.Empty)
                          .ToLowerInvariant();

            Result<int> result = ext switch
            {
                ".csv"  => await _uploadSaleAnalysisFromCsv.UploadSaleData(salesFileDTO),
                ".xlsx" => await _uploadSaleAnalysisFromExcel.UploadSaleData(salesFileDTO),
                _       => Result<int>.Fail(ErrorType.Invalid,
                               "Unsupported file format. File must be .csv or .xlsx.")
            };

            timer.Stop();
            Console.Write("timing:" + timer.ElapsedMilliseconds);

            if (result.IsSuccess)
                return Ok($"{result.Value} Sales added");

            return result.ToActionResult(this);
        }
    }
}
