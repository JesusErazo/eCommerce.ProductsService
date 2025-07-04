﻿namespace eCommerce.ProductsService.API.Middlewares;

public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;

  public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task Invoke(HttpContext httpContext)
  {
    try
    {
      await _next(httpContext);
    }
    catch (Exception ex)
    {
      _logger.LogError($"{ex.GetType().ToString()}: {ex.Message}");

      if (ex.InnerException is not null)
      {
        _logger.LogError($"{ex.InnerException.GetType().ToString()}: {ex.InnerException.Message}");
      }

      //Internal server error
      httpContext.Response.StatusCode = 500;

      await httpContext.Response.WriteAsJsonAsync(new {
        ex.Message,
        Type= ex.GetType().ToString()
      });
    }
  }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class ExceptionHandlingMiddlewareExtensions
{
  public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<ExceptionHandlingMiddleware>();
  }
}
