using CodeFirst.Shared.Serivces;
using ConsoleTables;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using System;
using System.Threading.Tasks;

namespace CodeFirst.Clinet
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = channel.CreateGrpcService<IWeatherForecastService>();

            var reply = await client.GetWeatherForecasts(new Shared.WeatherForecastRequest { ReturnCount = 100 });
            var table = new ConsoleTable("Date", "Temperature in C", "Temperature in F", "Summary");
            foreach (var bench in reply)
            {
                table.AddRow(bench.Date, bench.TemperatureC, bench.TemperatureF, bench.Summary);
            }
            table.Write(Format.Default);
            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
