using StackExchange.Redis;

class Program
{
    private static List<double> temperatures = new();
    private static SemaphoreSlim semaphore = new(1);
    private static bool exitRequested = false;

    static void Main()
    {
        try
        {
            Console.Write("Ingrese el nombre del equipo que desea monitorear: ");
            string deviceName = Console.ReadLine();

            using var connection = ConnectionMultiplexer.Connect("localhost");
            var subscriber = connection.GetSubscriber();
            subscriber.Subscribe($"ch:{deviceName}", (RedisChannel, value) =>
            {
                try
                {
                    double temperatura = double.Parse(value);
                    Console.WriteLine($"Temperatura recibida para {deviceName}: {temperatura}");

                    semaphore.Wait();
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
        finally
        {
            exitRequested = true;
        }
    }

    private static void ShowAnalytics()
    {
        semaphore.Wait();
        Console.WriteLine($"Datos de Temperatura:");
        Console.WriteLine($"Temperatura Minima: {temperatures.Min()}");
        Console.WriteLine($"Temperatura Maxima: {temperatures.Max()}");
        Console.WriteLine($"Temperatura Promedio: {Math.Round(temperatures.Average(), 2)}");
        semaphore.Release();
    }
}
