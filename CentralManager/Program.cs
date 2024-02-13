using StackExchange.Redis;
using System.Runtime.ConstrainedExecution;
using System;

class Program
{
    private static List<double> temperatures = new List<double>();
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1); // temperaturas
    private static bool exitRequested = false;

    [Obsolete]
    static void Main()
    {
        Console.Write("Ingrese el nombre del equipo que desea monitorear: ");
        string deviceName = Console.ReadLine();

        using (var connection = ConnectionMultiplexer.Connect("localhost"))
        {
            var subscriber = connection.GetSubscriber();
            var channel = $"ch:{deviceName}";

            subscriber.Subscribe(channel, (RedisChannel, value) =>
            {
                try
                {
                    double temperatura = double.Parse(value);
                    Console.WriteLine($"Temperatura recibida para {deviceName}: {temperatura}");

                    semaphore.Wait(3000);

                    if (exitRequested)
                        return;

                    temperatures.Add(temperatura);
                    ShowAnalytics();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al procesar la temperatura: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            Console.WriteLine($"Sistema central esperando lecturas de temperatura para {deviceName}...");
            Console.ReadLine();
        }

        exitRequested = true;
    }
    private static void ShowAnalytics()
    {
        semaphore.Wait(3000);
        Console.WriteLine($"Datos de Temperatura:");
        Console.WriteLine($"Temperatura Mínima: {temperatures.Min()}");
        Console.WriteLine($"Temperatura Máxima: {temperatures.Max()}");
        Console.WriteLine($"Temperatura Promedio: {Math.Round(temperatures.Average(), 2)}");
        Console.WriteLine($"Tendencia: {GetTrend()}");

        // DisplayTemperatureHistogram();
        semaphore.Release();
    }

    private static void DisplayTemperatureHistogram()
    {
        Console.WriteLine("Histograma de Frecuencia de Temperaturas:");

        var temperatureGroups = temperatures.GroupBy(temp => Math.Floor(temp));

        foreach (var group in temperatureGroups)
        {
            Console.WriteLine($"{group.Key}°C - {group.Key + 1}°C: {group.Count()} lecturas");
        }
    }

    private static string GetTrend()
    {
        // Determinar la tendencia basada en las últimas dos temperaturas
        if (temperatures.Count >= 2)
        {
            double lastTemperature = temperatures[^1];
            double secondLastTemperature = temperatures[^2];

            if (lastTemperature > secondLastTemperature)
                return "Ascendente";
            else if (lastTemperature < secondLastTemperature)
                return "Descendente";
            else
                return "Estable";
        }
        return "No hay datos suficientes para determinar la tendencia.";
    }
}
