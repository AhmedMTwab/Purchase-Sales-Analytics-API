using Microsoft.AspNetCore.Mvc;

namespace Purchase_Sales_API
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            if (result.IsSuccess)
                return controller.Ok(result.Value);

            return result.Error.ErrorType switch
            {
                ErrorType.NotFound       => controller.NotFound(result.Error),
                ErrorType.Invalid        => controller.BadRequest(result.Error),
                ErrorType.Conflict       => controller.Conflict(result.Error),
                ErrorType.Unauthorized   => controller.Unauthorized(result.Error),
                ErrorType.Forbidden      => controller.StatusCode(StatusCodes.Status403Forbidden, result.Error),
                _                        => controller.StatusCode(StatusCodes.Status500InternalServerError, result.Error),
            };
        }
    }
}

