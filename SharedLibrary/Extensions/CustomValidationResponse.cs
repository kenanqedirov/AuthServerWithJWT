using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Extensions
{
    public static class CustomValidationResponse
    {
        public static void UseCustomValidationResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values.Where(x=>x.Errors.Count>0).SelectMany(a=>a.Errors).Select(a=>a.ErrorMessage);
                    ErrorDto errorDto = new ErrorDto(errors.ToList(),true);
                    var response = Response<NoContentResult>.Fail(errorDto, 400,true);
                    return new BadRequestObjectResult(response);
                };
            });
        }
    }
}