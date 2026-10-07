using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Purchase_Sales_Core;
using Purchase_Sales_Core.DTOs.SaleDTO;
using Purchase_Sales_Core.Helpers;
using Purchase_Sales_Core.ServicesAbstractions.SaleServicesAbstractions;

namespace Purchase_Sales_API.Controllers
{
    [Route("api/upload/[controller]")]
    [ApiController]
    public class SaleController(
        IConfiguration configuration,
        IWebHostEnvironment environment) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadSales([FromForm] SalesFileMetadataDTO salesFileDTO)
        {
            var uploadedFile = salesFileDTO.salesFile;
            if (uploadedFile is null || uploadedFile.Length == 0)
                return BadRequest("Sales file is required and must not be empty.");

            var extension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            if (extension is not (".csv" or ".xlsx"))
                return BadRequest("Unsupported file format. File must be .csv or .xlsx.");

            var uploadDirectory = GetUploadDirectory();
            Directory.CreateDirectory(uploadDirectory);
            var filePath = Path.Combine(uploadDirectory, $"{Guid.NewGuid():N}{extension}");

            await AttachmentsUploader.SaveJobFile(uploadedFile, filePath);

            var job = new SalesFileJobDTO
            {
                filePath = filePath,
                headerRow = salesFileDTO.headerRow,
                productNameHeader = salesFileDTO.productNameHeader,
                quantityHeader = salesFileDTO.quantityHeader,
                priceHeader = salesFileDTO.priceHeader
            };

            try
            {
                var jobId = extension == ".csv"
                    ? BackgroundJob.Enqueue<IUploadSaleAnalysisFromCsv>(
                        service => service.UploadSaleData(job))
                    : BackgroundJob.Enqueue<IUploadSaleAnalysisFromExcel>(
                        service => service.UploadSaleData(job));

                return Accepted(new { jobId, message = "Sales upload queued for background processing." });
            }
            catch
            {
                System.IO.File.Delete(filePath);
                throw;
            }
        }

        private string GetUploadDirectory()
        {
            var configuredDirectory = configuration["Hangfire:UploadDirectory"]
                ?? Path.Combine("App_Data", "HangfireUploads");
            return Path.IsPathRooted(configuredDirectory)
                ? configuredDirectory
                : Path.Combine(environment.ContentRootPath, configuredDirectory);
        }
    }
}
