using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        using var client = new BookClient("http://localhost:5000");

        while (true)
        {
            Console.WriteLine("\n=== HTTP Клиент для библиотеки ===");
            Console.WriteLine("1. Получить все книги");
            Console.WriteLine("2. Получить книгу по ID");
            Console.WriteLine("3. Добавить новую книгу");
            Console.WriteLine("4. Обновить книгу");
            Console.WriteLine("5. Удалить книгу");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1":
                        await GetAllBooks(client);
                        break;
                    case "2":
                        await GetBookById(client);
                        break;
                    case "3":
                        await AddBook(client);
                        break;
                    case "4":
                        await UpdateBook(client);
                        break;
                    case "5":
                        await DeleteBook(client);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный ввод.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    static async Task GetAllBooks(BookClient client)
    {
        var books = await client.GetAllBooksAsync();
        if (books == null || books.Count == 0)
        {
            Console.WriteLine("Книг не найдено.");
            return;
        }
        Console.WriteLine("Список книг:");
        foreach (var b in books)
            Console.WriteLine($"[{b.Id}] {b.Title} – {b.Author}");
    }

    static async Task GetBookById(BookClient client)
    {
        Console.Write("Введите ID книги: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некорректный ID.");
            return;
        }
        var book = await client.GetBookByIdAsync(id);
        if (book != null)
            Console.WriteLine($"Книга: {book.Title} – {book.Author}");
    }

    static async Task AddBook(BookClient client)
    {
        Console.Write("Название: ");
        string title = Console.ReadLine() ?? "";
        Console.Write("Автор: ");
        string author = Console.ReadLine() ?? "";
        var newBook = new Book { Title = title, Author = author };
        var created = await client.AddBookAsync(newBook);
        if (created != null)
            Console.WriteLine($"Книга добавлена с ID = {created.Id}");
    }

    static async Task UpdateBook(BookClient client)
    {
        Console.Write("ID книги для обновления: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некорректный ID.");
            return;
        }
        Console.Write("Новое название: ");
        string title = Console.ReadLine() ?? "";
        Console.Write("Новый автор: ");
        string author = Console.ReadLine() ?? "";
        var updatedBook = new Book { Title = title, Author = author };
        bool success = await client.UpdateBookAsync(id, updatedBook);
        if (success)
            Console.WriteLine("Книга обновлена.");
    }

    static async Task DeleteBook(BookClient client)
    {
        Console.Write("ID книги для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некорректный ID.");
            return;
        }
        bool success = await client.DeleteBookAsync(id);
        if (success)
            Console.WriteLine("Книга удалена.");
    }
}