using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Purchase_Sales_Core.DTOs.ProductDTO;
using Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions;
using Purchase_Sales_Domain.Models;

namespace Purchase_Sales_Core.Services.ProductServices
{
    public class UploadPurchaseAnalysisFromCsv(
        IProductAdder _productAdder,
        IGetAllProducts _getAllProducts,
        IGetExistingProductByName _getProductByName,
        IProductUpdater _productUpdater) : IUploadPurchaseAnalysisFromCsv
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

        private async Task<Result<List<ProductAddDTO>>> ReadPurchase(PurchaseFileMetadataDTO purchaseFileDTO)
        {
            var purchaseList = new List<ProductAddDTO>();

            using var stream = purchaseFileDTO.purchaseFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter = true,
                BadDataFound = null,
                MissingFieldFound = null,
            });

            int headerRow = purchaseFileDTO.headerRow;
            for (int i = 1; i < headerRow; i++)
                await csv.ReadAsync();

            await csv.ReadAsync();
            csv.ReadHeader();

            // --- Validate required columns exist ---
            if (!csv.HeaderRecord!.Contains(purchaseFileDTO.productNameHeader, StringComparer.OrdinalIgnoreCase))
                return Result<List<ProductAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{purchaseFileDTO.productNameHeader}' not found in the CSV file.");

            if (!csv.HeaderRecord!.Contains(purchaseFileDTO.priceHeader, StringComparer.OrdinalIgnoreCase))
                return Result<List<ProductAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{purchaseFileDTO.priceHeader}' not found in the CSV file.");

            while (await csv.ReadAsync())
            {
                var productName = csv.GetField<string>(purchaseFileDTO.productNameHeader)?.Trim();
                if (string.IsNullOrEmpty(productName))
                    continue;

                decimal purchasePrice = 0;
                var purchasePriceField = csv.GetField(purchaseFileDTO.priceHeader);
                if (!string.IsNullOrEmpty(purchasePriceField))
                    decimal.TryParse(purchasePriceField, out purchasePrice);

                purchaseList.Add(new ProductAddDTO
                {
                    name = productName,
                    purchasePrice = purchasePrice,
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
