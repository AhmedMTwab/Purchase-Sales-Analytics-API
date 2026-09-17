using FluentValidation;

namespace Purchase_Sales_Core.Validators
{
    public class PurchaseFileDTOValidator : AbstractValidator<PurchaseFileMetadataDTO>
    {
        public PurchaseFileDTOValidator()
        {
            RuleFor(x => x.purchaseFile)
                .NotNull().WithMessage("Purchase file is required.")
                .Must(f => f != null && f.Length > 0).WithMessage("Purchase file must not be empty.");

            RuleFor(x => x.productNameHeader)
                .NotEmpty().WithMessage("Product name header column name is required.");

            RuleFor(x => x.priceHeader)
                .NotEmpty().WithMessage("Price header column name is required.");

            RuleFor(x => x.headerRow)
                .GreaterThan(0).WithMessage("Header row must be greater than 0.");
        }
    }
}

