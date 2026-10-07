using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Purchase_Sales_Core;
using Purchase_Sales_Core.DTOs.PurchaseDTO;
using Purchase_Sales_Core.Helpers;
using Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions;

namespace Purchase_Sales_API.Controllers
{
    [Route("api/upload/[controller]")]
    [ApiController]
    public class PurchaseController(
        IConfiguration configuration,
        IWebHostEnvironment environment) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadPurchases([FromForm] PurchaseFileMetadataDTO purchaseFileDTO)
        {
            var uploadedFile = purchaseFileDTO.purchaseFile;
            if (uploadedFile is null || uploadedFile.Length == 0)
                return BadRequest("Purchase file is required and must not be empty.");

            var extension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            if (extension is not (".csv" or ".xlsx"))
                return BadRequest("Unsupported file format. File must be .csv or .xlsx.");

            var uploadDirectory = GetUploadDirectory();
            Directory.CreateDirectory(uploadDirectory);
            var filePath = Path.Combine(uploadDirectory, $"{Guid.NewGuid():N}{extension}");

            await AttachmentsUploader.SaveJobFile(uploadedFile, filePath);

            var job = new PurchaseFileJobDTO
            {
                filePath = filePath,
                headerRow = purchaseFileDTO.headerRow,
                productNameHeader = purchaseFileDTO.productNameHeader,
                priceHeader = purchaseFileDTO.priceHeader
            };

            try
            {
                var jobId = extension == ".csv"
                    ? BackgroundJob.Enqueue<IUploadPurchaseAnalysisFromCsv>(
                        service => service.UploadPurchaseData(job))
                    : BackgroundJob.Enqueue<IUploadPurchaseAnalysisFromExcel>(
                        service => service.UploadPurchaseData(job));

                return Accepted(new { jobId, message = "Purchase upload queued for background processing." });
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
