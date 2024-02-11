using StackExchange.Redis;

class Program
{
    private const string RedisConnectionString = "localhost:6379";
    private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
    static void Main()
    {
        List<string> devices = [];
        Console.WriteLine("Registro de Equipos (Ingrese 'exit' para finalizar el registro):");

        while (true)
        {
            Console.Write("Ingrese el nombre del equipo (o 'exit' para salir): ");
            string newDevice = Console.ReadLine();

            if (string.IsNullOrEmpty(newDevice)) break;
            if (newDevice == "exit") break;

            devices.Add(newDevice);
        }

        if (devices.Count > 0)
        {
            var random = new Random();
            var publisher = connection.GetSubscriber();
            while (true)
            {
                foreach(var device in devices)
                {
                    double deviceTemp = random.Next(40, 70) + random.NextDouble();
                    deviceTemp = Math.Round(deviceTemp, 2);
                    string channelName = $"ch:{device}";

                    publisher.PublishAsync(channelName, deviceTemp.ToString(), CommandFlags.FireAndForget);
                    Console.WriteLine($"Lectura de temperatura para {device}: {deviceTemp}");
                    Thread.Sleep(500);
                }
            }
        }
        return;      
    }
}
