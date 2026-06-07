using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class BookClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BookClient(string baseAddress = "http://localhost:5000")
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
    }

    // GET /book – получить все книги
    public async Task<List<Book>?> GetAllBooksAsync()
    {
        var response = await _httpClient.GetAsync("/book");
        await EnsureSuccessWithMessage(response);
        return await response.Content.ReadFromJsonAsync<List<Book>>(_jsonOptions);
    }

    // GET /book/{id} – получить книгу по ID
    public async Task<Book?> GetBookByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/book/{id}");
        await EnsureSuccessWithMessage(response);
        return await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
    }

    // POST /book – добавить книгу
    public async Task<Book?> AddBookAsync(Book book)
    {
        var response = await _httpClient.PostAsJsonAsync("/book", book);
        await EnsureSuccessWithMessage(response);
        return await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
    }

    // PUT /book/{id} – обновить книгу
    public async Task<bool> UpdateBookAsync(int id, Book book)
    {
        var response = await _httpClient.PutAsJsonAsync($"/book/{id}", book);
        await EnsureSuccessWithMessage(response);
        return response.IsSuccessStatusCode;
    }

    // DELETE /book/{id} – удалить книгу
    public async Task<bool> DeleteBookAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/book/{id}");
        await EnsureSuccessWithMessage(response);
        return response.IsSuccessStatusCode;
    }

    // Обработка ошибок: если статус не успешный, читаем тело ответа и выбрасываем исключение
    private static async Task EnsureSuccessWithMessage(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"Ошибка {(int)response.StatusCode} ({response.ReasonPhrase}): {errorMessage}"
        );
    }

    public void Dispose() => _httpClient.Dispose();
}