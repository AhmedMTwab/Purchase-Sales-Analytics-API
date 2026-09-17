using OfficeOpenXml;
using Purchase_Sales_Core.DTOs.ProductDTO;
using Purchase_Sales_Core.DTOs.SaleDTO;
using Purchase_Sales_Core.Services.ProductServices;
using Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions;
using Purchase_Sales_Core.ServicesAbstractions.SaleServicesAbstractions;
using Purchase_Sales_Domain.Models;

namespace Purchase_Sales_Core.Services.SaleServices
{
    public class UploadSaleAnalysisFromExcel(
        ISaleAdder _saleAdder,
        IGetAllProducts _getAllProducts,
        IProductAdder _productAdder) : IUploadSaleAnalysisFromExcel
    {
        const int batchSize = constants.BatchSize;

        public async Task<Result<int>> UploadSaleData(SalesFileMetadataDTO saleFileDTO)
        {
            // --- Validation ---
            if (saleFileDTO.salesFile == null || saleFileDTO.salesFile.Length == 0)
                return Result<int>.Fail(ErrorType.Invalid, "Sales file is missing or empty.");

            int insertedSales = 0;
            int totalSalesAdded = 0;

            List<string> allProducts = await _getAllProducts.GetProductsNamesAsync();
            HashSet<string> allProductsNames = new HashSet<string>(allProducts, StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ProductAddDTO> addedProducts = new Dictionary<string, ProductAddDTO>();

            var readResult = await ReadSales(saleFileDTO);
            if (!readResult.IsSuccess)
                return Result<int>.Fail(readResult.Error.ErrorType, readResult.Error.Error);

            var batchOfSales = new List<SaleAddDTO>();
            foreach (var sale in readResult.Value)
            {
                AddNewProductToAddList(sale.productName, allProductsNames, addedProducts);
                batchOfSales.Add(sale);
                totalSalesAdded++;
                insertedSales++;

                if (insertedSales >= batchSize)
                {
                    await AddProductsToDB(addedProducts);
                    await AddSalesToDB(batchOfSales);
                    insertedSales = 0;
                }
            }

            await AddProductsToDB(addedProducts);
            await AddSalesToDB(batchOfSales);

            return Result<int>.Ok(totalSalesAdded);
        }

        public Dictionary<string, int> ReadWorksheetHeader(ExcelWorksheet worksheet, int headerRowIndex)
        {
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var header = worksheet.Cells[headerRowIndex, col].Text.Trim();
                if (!string.IsNullOrEmpty(header))
                    headers[header] = col;
            }
            return headers;
        }

        private async Task<Result<List<SaleAddDTO>>> ReadSales(SalesFileMetadataDTO saleFileDTO)
        {
            var salesToAdd = new List<SaleAddDTO>();

            MemoryStream stream = new MemoryStream();
            await saleFileDTO.salesFile.CopyToAsync(stream);
            ExcelPackage.License.SetNonCommercialPersonal("Eltwab");

            using ExcelPackage excelpackage = new ExcelPackage(stream);
            ExcelWorksheet worksheet = excelpackage.Workbook.Worksheets[0];

            // --- Validate headerRow is in range ---
            int totalRows = worksheet.Dimension.Rows;
            if (saleFileDTO.headerRow < 1 || saleFileDTO.headerRow > totalRows)
                return Result<List<SaleAddDTO>>.Fail(ErrorType.Invalid,
                    $"Header row index {saleFileDTO.headerRow} is out of range (file has {totalRows} rows).");

            var headers = ReadWorksheetHeader(worksheet, saleFileDTO.headerRow);

            // --- Validate required columns exist ---
            if (!headers.ContainsKey(saleFileDTO.productNameHeader))
                return Result<List<SaleAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{saleFileDTO.productNameHeader}' not found in the Excel file.");

            if (!headers.ContainsKey(saleFileDTO.quantityHeader))
                return Result<List<SaleAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{saleFileDTO.quantityHeader}' not found in the Excel file.");

            if (!headers.ContainsKey(saleFileDTO.priceHeader))
                return Result<List<SaleAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{saleFileDTO.priceHeader}' not found in the Excel file.");

            for (int row = saleFileDTO.headerRow + 1; row <= totalRows; row++)
            {
                string? cellValue = worksheet.GetValue(row, headers[saleFileDTO.productNameHeader])?.ToString();
                if (string.IsNullOrEmpty(cellValue))
                    continue;

                string productName = cellValue.Trim();

                var quantityCell = worksheet.Cells[row, headers[saleFileDTO.quantityHeader]].Value;
                if (!decimal.TryParse(quantityCell?.ToString(), out decimal quantity))
                    continue;

                var priceCell = worksheet.Cells[row, headers[saleFileDTO.priceHeader]].Value;
                if (!decimal.TryParse(priceCell?.ToString(), out decimal price))
                    continue;

                salesToAdd.Add(new SaleAddDTO
                {
                    productName = productName,
                    quantity = (int)quantity,
                    price = price
                });
            }

            return Result<List<SaleAddDTO>>.Ok(salesToAdd);
        }

        private void AddNewProductToAddList(
            string trimedName,
            HashSet<string> allProductsNames,
            Dictionary<string, ProductAddDTO> addedProducts)
        {
            bool isNewProduct = !allProductsNames.Contains(trimedName) && !addedProducts.ContainsKey(trimedName);
            if (isNewProduct)
            {
                addedProducts.Add(trimedName, new ProductAddDTO
                {
                    name = trimedName,
                    purchasePrice = 0,
                    updatedAt = DateTime.Now
                });
                allProductsNames.Add(trimedName);
            }
        }

        private async Task AddProductsToDB(Dictionary<string, ProductAddDTO> addedProducts)
        {
            if (addedProducts.Any())
            {
                await _productAdder.AddPulkOfProducts(addedProducts.Values.ToList());
                addedProducts.Clear();
            }
        }

        private async Task AddSalesToDB(List<SaleAddDTO> salesToAdd)
        {
            if (salesToAdd.Any())
            {
                await _saleAdder.AddPulkOfSales(salesToAdd);
                salesToAdd.Clear();
            }
        }
    }
}
