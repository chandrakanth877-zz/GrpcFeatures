using CodeFirst.Shared;
using CodeFirst.Shared.Serivces;
using Grpc.Core;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeFirst.Service.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        public Task<List<WeatherForecasrResponse>> GetWeatherForecasts(WeatherForecastRequest request, CallContext context = default)
        {
            if (request.ReturnCount > 999999)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Return count is too large."));
            }

            var rng = new Random();
            var results = Enumerable.Range(1, request.ReturnCount).Select(index => new WeatherForecasrResponse
            {
                Date = DateTime.UtcNow.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToList();
            return Task.FromResult(results);
        }
    }
}
