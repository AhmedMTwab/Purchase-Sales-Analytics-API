using OfficeOpenXml;
using Purchase_Sales_Core.DTOs.ProductDTO;
using Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions;
using Purchase_Sales_Domain.Models;

namespace Purchase_Sales_Core.Services.ProductServices
{
    public class UploadPurchaseAnalysisFromXLSX(
        IProductAdder _productAdder,
        IGetAllProducts _getAllProducts,
        IGetExistingProductByName _getProductByName,
        IProductUpdater _productUpdater) : IUploadPurchaseAnalysisFromExcel
    {
        const int batchSize = constants.BatchSize;

        public async Task<Result<int>> UploadPurchaseData(PurchaseFileMetadataDTO purchaseFileDTO)
        {
            // --- Validation ---
            if (purchaseFileDTO.purchaseFile == null || purchaseFileDTO.purchaseFile.Length == 0)
                return Result<int>.Fail(ErrorType.Invalid, "Purchase file is missing or empty.");

            int insertedProducts = 0;

            List<string> allProducts = await _getAllProducts.GetProductsNamesAsync();
            HashSet<string> allProductsNames = new HashSet<string>(allProducts, StringComparer.OrdinalIgnoreCase);

            Dictionary<string, ProductAddDTO> addedProducts = new Dictionary<string, ProductAddDTO>();
            Dictionary<string, Product> productsToUpdate = new Dictionary<string, Product>();

            var readResult = await ReadPurchase(purchaseFileDTO);
            if (!readResult.IsSuccess)
                return Result<int>.Fail(readResult.Error.ErrorType, readResult.Error.Error);

            foreach (var purchase in readResult.Value)
            {
                await AddUpdatedProductToUpdateList(purchase, allProductsNames, productsToUpdate);
                AddNewProductToAddList(purchase, allProductsNames, addedProducts, ref insertedProducts);

                if (addedProducts.Count >= batchSize)
                {
                    await AddProductsToDB(addedProducts);
                    await UpdateProductsInDB(productsToUpdate);
                }
            }

            await AddProductsToDB(addedProducts);
            await UpdateProductsInDB(productsToUpdate);

            return Result<int>.Ok(insertedProducts);
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

        private async Task<Result<List<ProductAddDTO>>> ReadPurchase(PurchaseFileMetadataDTO purchaseFileDTO)
        {
            var purchaseList = new List<ProductAddDTO>();

            MemoryStream stream = new MemoryStream();
            await purchaseFileDTO.purchaseFile.CopyToAsync(stream);
            ExcelPackage.License.SetNonCommercialPersonal("Eltwab");

            using ExcelPackage excelpackage = new ExcelPackage(stream);
            ExcelWorksheet worksheet = excelpackage.Workbook.Worksheets[0];

            // --- Validate headerRow is in range ---
            int totalRows = worksheet.Dimension.Rows;
            if (purchaseFileDTO.headerRow < 1 || purchaseFileDTO.headerRow > totalRows)
                return Result<List<ProductAddDTO>>.Fail(ErrorType.Invalid,
                    $"Header row index {purchaseFileDTO.headerRow} is out of range (file has {totalRows} rows).");

            var headers = ReadWorksheetHeader(worksheet, purchaseFileDTO.headerRow);

            // --- Validate required columns exist ---
            if (!headers.ContainsKey(purchaseFileDTO.productNameHeader))
                return Result<List<ProductAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{purchaseFileDTO.productNameHeader}' not found in the Excel file.");

            if (!headers.ContainsKey(purchaseFileDTO.priceHeader))
                return Result<List<ProductAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{purchaseFileDTO.priceHeader}' not found in the Excel file.");

            for (int row = purchaseFileDTO.headerRow + 1; row <= totalRows; row++)
            {
                string? cellValue = worksheet.Cells[row, 18].Value?.ToString();
                if (string.IsNullOrEmpty(cellValue))
                    continue;

                var purchasePriceRaw = worksheet.Cells[row, headers[purchaseFileDTO.priceHeader]].Value;
                if (!decimal.TryParse(purchasePriceRaw?.ToString(), out decimal totalPurchase))
                    continue;

                var productName = worksheet.GetValue<string>(row, headers[purchaseFileDTO.productNameHeader])?.Trim();
                if (string.IsNullOrEmpty(productName))
                    continue;

                purchaseList.Add(new ProductAddDTO
                {
                    name = productName,
                    purchasePrice = totalPurchase,
                    updatedAt = DateTime.Now
                });
            }

            return Result<List<ProductAddDTO>>.Ok(purchaseList);
        }

        private void AddNewProductToAddList(
            ProductAddDTO product,
            HashSet<string> allProductNames,
            Dictionary<string, ProductAddDTO> addedProducts,
            ref int insertedProducts)
        {
            if (!allProductNames.Contains(product.name))
            {
                if (!addedProducts.ContainsKey(product.name))
                {
                    addedProducts.Add(product.name, new ProductAddDTO
                    {
                        name = product.name,
                        purchasePrice = product.purchasePrice,
                        updatedAt = DateTime.Now
                    });
                    insertedProducts++;
                }
                else
                {
                    addedProducts[product.name].purchasePrice += product.purchasePrice;
                }
            }
        }

        private async Task AddUpdatedProductToUpdateList(
            ProductAddDTO product,
            HashSet<string> allProductNames,
            Dictionary<string, Product> productsToUpdate)
        {
            if (allProductNames.Contains(product.name))
            {
                if (!productsToUpdate.ContainsKey(product.name))
                {
                    Product existedProduct = await _getProductByName.GetProductByName(product.name);
                    existedProduct.purchasePrice += product.purchasePrice;
                    productsToUpdate.Add(product.name, existedProduct);
                }
                else
                {
                    productsToUpdate[product.name].purchasePrice += product.purchasePrice;
                }
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

        private async Task UpdateProductsInDB(Dictionary<string, Product> productsToUpdate)
        {
            if (productsToUpdate.Any())
            {
                await _productUpdater.UpdatePulkOfProduct(productsToUpdate.Values.ToList());
                productsToUpdate.Clear();
            }
        }
    }
}
