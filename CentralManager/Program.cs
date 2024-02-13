using StackExchange.Redis;

class Program
{
    private static List<double> temperatures = new List<double>();
    private static SemaphoreSlim semaphore = new SemaphoreSlim(4); // temperaturas
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

                    semaphore.Wait(2000);

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
        semaphore.Wait();
        Console.WriteLine($"Datos de Temperatura:");
        Console.WriteLine($"Temperatura Minima: {temperatures.Min()}");
        Console.WriteLine($"Temperatura Maxima: {temperatures.Max()}");
        Console.WriteLine($"Temperatura Promedio: {Math.Round(temperatures.Average(), 2)}");
        semaphore.Release();
    }
}
