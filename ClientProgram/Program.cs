using ClientProgram;
using System.Text.Json;

public class Program
{
    private static async Task Main(string[] args)
    {
        using var client = new Client("127.0.0.1", 8888);
        await client.OpenConnectionAsync();

        Console.WriteLine("Введите данные для отправки объектов Person.");
        Console.WriteLine("Формат: Id,FirstName,LastName (например: 1,Ivan,Pupkin,25,male)");
        Console.WriteLine("Для выхода введите 'exit'.");

        while (true)
        {
            Console.Write("\n> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
                break;

            string[] parts = input.Split(',');
            if (parts.Length != 5)
            {
                Console.WriteLine("Ошибка: нужно ввести 5 значений через запятую (Id,FirstName,LastName,Age,Gender).");
                continue;
            }

            if (!int.TryParse(parts[0], out int id) || !int.TryParse(parts[3], out int age))
            {
                Console.WriteLine("Ошибка: Id и Age должны быть целыми числами.");
                continue;
            }

            string firstName = parts[1].Trim();
            string lastName = parts[2].Trim();
            string gender = parts[4].Trim();

            var person = new Person
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Age = age,
                Gender = gender
            };

            string json = JsonSerializer.Serialize(person);
            Console.WriteLine($"Отправка: {json}");

            try
            {
                await client.SendAsync(json);
                var response = await client.ReciveAsync();
                Console.WriteLine($"Ответ сервера: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке: {ex.Message}");
            }
        }

        Console.WriteLine("Работа клиента завершена.");
        Console.ReadLine();
    }
}