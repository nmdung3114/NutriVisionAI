using Microsoft.AspNetCore.Mvc;
using NutriVision.Api.Common;

namespace NutriVision.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected OkObjectResult Success<T>(T data, string message = "Thành công") => Ok(ApiResponse.Ok(data, message));
    protected OkObjectResult Success(string message = "Thành công") => Ok(ApiResponse.Ok(message));
}
