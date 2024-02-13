using StackExchange.Redis;

class Program
{
    private const string RedisConnectionString = "localhost:6379";
    private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
    private static bool exitRequested = false;

    [Obsolete]
    static void Main()
    {
        List<string> devices = [];

        Console.WriteLine("Registro de Equipos (Ingrese 'exit' para finalizar el registro):");

        while (true)
        {
            Console.Write("Ingrese el nombre del equipo (o 'exit' para salir): ");
            string newDevice = Console.ReadLine();

            if (string.IsNullOrEmpty(newDevice) || newDevice == "exit") break;

            devices.Add(newDevice);
        }

        if (devices.Count > 0)
        {
            var random = new Random();
            var publisher = connection.GetSubscriber();
            var startTime = DateTime.Now;

            while (true)
            {
                foreach (var device in devices)
                {
                    if (exitRequested)
                        return;

                    double elapsedTime = (DateTime.Now - startTime).TotalSeconds;

                    // Simular fluctuación realista utilizando una función senoidal
                    double fluctuation = Math.Sin(elapsedTime * 0.1) * 5;
                    double baseTemperature = random.Next(40, 70) + random.NextDouble();
                    double deviceTemp = Math.Round(baseTemperature + fluctuation, 2);

                    string channelName = $"ch:{device}";

                    Thread.Sleep(1000);
                    publisher.PublishAsync(channelName, deviceTemp.ToString(), CommandFlags.FireAndForget);
                    Console.WriteLine($"Lectura de temperatura para {device}: {deviceTemp}");
                }
            }
        }

        connection.Close();
    }
}
