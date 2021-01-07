using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weather;

namespace GrpcImpl.Service.Serices
{
    class WeatherService : WeatherForecasts.WeatherForecastsBase
    {
        private readonly ILogger _logger;

        public WeatherService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WeatherService>();
        }
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public override Task<GetWeatherForecastsResponse> GetWeatherForecasts(GetWeatherForecastsRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Getting {request.ReturnCount} Weather Forecasts");
            if (request.ReturnCount > 999999)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Return count is too large."));
            }

            var rng = new Random();
            var results = Enumerable.Range(1, request.ReturnCount).Select(index => new WeatherForecast
            {
                Date = DateTime.UtcNow.AddDays(index).ToTimestamp(),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();

            var response = new GetWeatherForecastsResponse();
            response.Forecasts.AddRange(results);

            return Task.FromResult(response);
        }
    }
}
