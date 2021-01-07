using ConsoleTables;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;
using Weather;

namespace GrpcImpl.Clinet
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var clinet = new WeatherForecasts.WeatherForecastsClient(channel);
            var results = await clinet.GetWeatherForecastsAsync(new GetWeatherForecastsRequest { ReturnCount = 100 });

            var table = new ConsoleTable("Date", "Temperature in C", "Summary");
            foreach (var bench in results.Forecasts)
            {
                table.AddRow(bench.Date, bench.TemperatureC, bench.Summary);
            }
            table.Write(Format.Default);
            Console.ReadLine();
        }
    }
}
