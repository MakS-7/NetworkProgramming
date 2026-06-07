using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Core;

namespace TcpBookServer;

public class TcpBookServer
{
    private TcpListener _listener;
    private readonly int _port;
    private bool _isRunning;
    private readonly List<Book> _books;
    private int _nextId;

    public TcpBookServer(int port)
    {
        _port = port;
        // Инициализируем тестовыми данными
        _books = new List<Book>();
        _nextId = 1;
        // Добавим пару книг для примера
        _books.Add(new Book { Id = _nextId++, Name = "1984", Author = "Джордж Оруэлл", Description = "Антиутопия" });
        _books.Add(new Book { Id = _nextId++, Name = "Мастер и Маргарита", Author = "Михаил Булгаков", Description = "Мистический роман" });
    }

    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"[Server] Запущен на порту {_port}");

        while (_isRunning)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine("[Server] Новый клиент подключился!");
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Ошибка: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return;

                string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"[Server] Получен запрос: {requestJson}");

                
                ResponceDto response = ProcessRequest(requestJson);
                string responseJson = JsonSerializer.Serialize(response);
                byte[] responseData = Encoding.UTF8.GetBytes(responseJson);

                await stream.WriteAsync(responseData, 0, responseData.Length);
                Console.WriteLine($"[Server] Отправлен ответ: {responseJson}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Ошибка при обработке клиента: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("[Server] Клиент отключился.");
        }
    }

    private ResponceDto ProcessRequest(string requestJson)
    {
        
        RequestDto? requestDto;
        try
        {
            requestDto = JsonSerializer.Deserialize<RequestDto>(requestJson);
        }
        catch (JsonException)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = "Invalid JSON format."
            };
        }

        if (requestDto == null)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = "Request cannot be empty."
            };
        }

        
        if (!Enum.IsDefined(typeof(DictionaryOperation), requestDto.Operation))
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = $"Invalid operation: {requestDto.Operation}."
            };
        }

        
        try
        {
            switch (requestDto.Operation)
            {
                case DictionaryOperation.Create:
                    return CreateBook(requestDto.Body);
                case DictionaryOperation.Read:
                    return ReadAllBooks();
                case DictionaryOperation.Update:
                    return UpdateBook(requestDto.Body);
                case DictionaryOperation.Delete:
                    return DeleteBook(requestDto.Body);
                default:
                    return new ResponceDto
                    {
                        Responce = DictionaryResponce.BadRequest,
                        ResponceMessage = $"Unsupported operation: {requestDto.Operation}."
                    };
            }
        }
        catch (Exception ex)
        {
            
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = $"Internal server error: {ex.Message}."
            };
        }
    }

    

    private ResponceDto CreateBook(string body)
    {
        Book? newBook;
        try
        {
            newBook = JsonSerializer.Deserialize<Book>(body);
        }
        catch (JsonException)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = "Invalid book JSON format."
            };
        }

        if (newBook == null || string.IsNullOrWhiteSpace(newBook.Name))
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = "Book must have a Name."
            };
        }

        newBook.Id = _nextId++;
        _books.Add(newBook);
        return new ResponceDto
        {
            Responce = DictionaryResponce.OK,
            ResponceMessage = $"Book '{newBook.Name}' created with ID {newBook.Id}."
        };
    }

    private ResponceDto ReadAllBooks()
    {
        if (_books.Count == 0)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.NotFoundResource,
                ResponceMessage = "No books found."
            };
        }

        string booksJson = JsonSerializer.Serialize(_books);
        return new ResponceDto
        {
            Responce = DictionaryResponce.OK,
            ResponceMessage = booksJson
        };
    }

    private ResponceDto UpdateBook(string body)
    {
        Book? updatedBook;
        try
        {
            updatedBook = JsonSerializer.Deserialize<Book>(body);
        }
        catch (JsonException)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = "Invalid book JSON format."
            };
        }

        if (updatedBook == null || updatedBook.Id <= 0)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = "Invalid book ID."
            };
        }

        var existingBook = _books.FirstOrDefault(b => b.Id == updatedBook.Id);
        if (existingBook == null)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.NotFoundResource,
                ResponceMessage = $"Book with ID {updatedBook.Id} not found."
            };
        }

        existingBook.Name = updatedBook.Name;
        existingBook.Author = updatedBook.Author;
        existingBook.Description = updatedBook.Description;

        return new ResponceDto
        {
            Responce = DictionaryResponce.OK,
            ResponceMessage = $"Book with ID {updatedBook.Id} updated."
        };
    }

    private ResponceDto DeleteBook(string body)
    {
        if (!int.TryParse(body, out int id) || id <= 0)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.BadRequest,
                ResponceMessage = "Invalid ID format. Expected a number."
            };
        }

        var bookToRemove = _books.FirstOrDefault(b => b.Id == id);
        if (bookToRemove == null)
        {
            return new ResponceDto
            {
                Responce = DictionaryResponce.NotFoundResource,
                ResponceMessage = $"Book with ID {id} not found."
            };
        }

        _books.Remove(bookToRemove);
        return new ResponceDto
        {
            Responce = DictionaryResponce.OK,
            ResponceMessage = $"Book with ID {id} deleted."
        };
    }

    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
        Console.WriteLine("[Server] Остановлен.");
    }
}


public class Program
{
    public static async Task Main(string[] args)
    {
        int port = 8888;
        var server = new TcpBookServer(port);
        var serverTask = server.StartAsync();

        Console.WriteLine($"Сервер запущен. Для выхода нажмите 'q'.");
        while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

        server.Stop();
        await serverTask;
    }
}