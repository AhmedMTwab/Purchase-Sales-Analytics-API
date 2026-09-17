using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Purchase_Sales_Core.DTOs.ProductDTO;
using Purchase_Sales_Core.DTOs.SaleDTO;
using Purchase_Sales_Core.ServicesAbstractions.ProductServicesAbstractions;
using Purchase_Sales_Core.ServicesAbstractions.SaleServicesAbstractions;
using Purchase_Sales_Domain.Models;

namespace Purchase_Sales_Core.Services.SaleServices
{
    public class UploadSaleAnalysisFromCsv(
        ISaleAdder _saleAdder,
        IGetAllProducts _getAllProducts,
        IProductAdder _productAdder) : IUploadSaleAnalysisFromCsv
    {
        const int batchSize = constants.BatchSize;

        public async Task<Result<int>> UploadSaleData(SalesFileMetadataDTO saleFileDTO)
        {
            // --- Validation ---
            if (saleFileDTO.salesFile == null || saleFileDTO.salesFile.Length == 0)
                return Result<int>.Fail(ErrorType.Invalid, "Sales file is missing or empty.");

            int insertedSales = 0;
            int totalSalesAdded = 0;

            var allProducts = await _getAllProducts.GetProductsNamesAsync();
            var allProductNames = new HashSet<string>(allProducts, StringComparer.OrdinalIgnoreCase);
            var addedProducts = new Dictionary<string, ProductAddDTO>();

            var readResult = await ReadSales(saleFileDTO);
            if (!readResult.IsSuccess)
                return Result<int>.Fail(readResult.Error.ErrorType, readResult.Error.Error);

            var batchOfSales = new List<SaleAddDTO>();
            foreach (var sale in readResult.Value)
            {
                AddNewProductToAddList(sale.productName, allProductNames, addedProducts);
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

        private async Task<Result<List<SaleAddDTO>>> ReadSales(SalesFileMetadataDTO saleFileDTO)
        {
            var salesToAdd = new List<SaleAddDTO>();

            using var stream = saleFileDTO.salesFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter = true,
                BadDataFound = null,
                MissingFieldFound = null,
            });

            int headerRow = saleFileDTO.headerRow;
            for (int i = 1; i < headerRow; i++)
                await csv.ReadAsync();

            await csv.ReadAsync();
            csv.ReadHeader();

            // --- Validate required columns exist ---
            if (!csv.HeaderRecord!.Contains(saleFileDTO.productNameHeader, StringComparer.OrdinalIgnoreCase))
                return Result<List<SaleAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{saleFileDTO.productNameHeader}' not found in the CSV file.");

            if (!csv.HeaderRecord!.Contains(saleFileDTO.quantityHeader, StringComparer.OrdinalIgnoreCase))
                return Result<List<SaleAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{saleFileDTO.quantityHeader}' not found in the CSV file.");

            if (!csv.HeaderRecord!.Contains(saleFileDTO.priceHeader, StringComparer.OrdinalIgnoreCase))
                return Result<List<SaleAddDTO>>.Fail(ErrorType.Invalid,
                    $"Column '{saleFileDTO.priceHeader}' not found in the CSV file.");

            while (await csv.ReadAsync())
            {
                var productName = csv.GetField<string>(saleFileDTO.productNameHeader)?.Trim();
                if (string.IsNullOrEmpty(productName))
                    continue;

                var quantity = csv.TryGetField<decimal>(saleFileDTO.quantityHeader, out var q) ? q : 0;
                var price = csv.TryGetField<decimal>(saleFileDTO.priceHeader, out var p) ? p : 0;

                salesToAdd.Add(new SaleAddDTO
                {
                    productName = productName,
                    quantity = (int)quantity,
                    price = price
                });
            }

            return Result<List<SaleAddDTO>>.Ok(salesToAdd);
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

        private void AddNewProductToAddList(
            string productName,
            HashSet<string> allProductNames,
            Dictionary<string, ProductAddDTO> addedProducts)
        {
            if (!allProductNames.Contains(productName) && !addedProducts.ContainsKey(productName))
            {
                var newProduct = new ProductAddDTO
                {
                    name = productName,
                    purchasePrice = 0,
                    updatedAt = DateTime.Now
                };
                addedProducts.Add(newProduct.name, newProduct);
                allProductNames.Add(productName);
            }
        }
    }
}
