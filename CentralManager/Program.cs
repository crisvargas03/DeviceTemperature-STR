using StackExchange.Redis;

class Program
{
    private static List<double> temperatures = [];
    static void Main()
    {
        Console.Write("Ingrese el nombre del equipo que desea monitorear: ");
        string deviceName = Console.ReadLine();

        using var connection = ConnectionMultiplexer.Connect("localhost");
        var subscriber = connection.GetSubscriber();
        subscriber.Subscribe($"ch:{deviceName}", (RedisChannel, value) =>
        {
            double temperatura = double.Parse(value);
            Console.WriteLine($"Temperatura recibida para {deviceName}: {temperatura}");

            temperatures.Add(temperatura);
            ShowAnalytics();
        });

        Console.WriteLine($"Sistema central esperando lecturas de temperatura para {deviceName}...");
        Console.ReadLine();
    }

    private static void ShowAnalytics()
    {
        Console.WriteLine($"Datos de Temperatura:");
        Console.WriteLine($"Temperatura Minima: {temperatures.Min()}");
        Console.WriteLine($"Temperatura Maxima: {temperatures.Max()}");
        Console.WriteLine($"Temperatura Promedio: {Math.Round(temperatures.Average(), 2)}");
    }
}