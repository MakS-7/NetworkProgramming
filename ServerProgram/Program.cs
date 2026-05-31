using ClientProgram;
using ServerProgram;
using System.Net;
using System.Text.Json;


internal class Program
{
    private static readonly List<Person> _persons = new();

    private static async Task Main(string[] args)
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
        var server = new Server(ipEndPoint, 1000);

        Console.WriteLine("Сервер запущен");

        while (true)
        {
            var request = await server.ReceiveAsync();
            Console.WriteLine($"Запрос получен: {request}");

            try
            {
                Person? person = JsonSerializer.Deserialize<Person>(request);
                if (person != null)
                {
                    lock (_persons)
                    {
                        _persons.Add(person);
                        Console.WriteLine($"Добавлен: Id={person.Id}, FirstName={person.FirstName}, LastName={person.LastName}, Age={person.Age}, Gender={person.Gender}");
                        Console.WriteLine($"Всего объектов в списке: {_persons.Count}");
                    }
                    await server.SendAsync("OK");
                }
                else
                {
                    await server.SendAsync("Ошибка: неверный формат данных");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка десериализации: {ex.Message}");
                await server.SendAsync("Ошибка: неверный формат JSON");
            }
        }
    }
}