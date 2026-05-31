using ClientProgram;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ServerProgram
{
    public class Server : IDisposable
    {
        private readonly Socket _serverSocket;
        private Socket? _clientSocket;

        private const int BufferSize = 1024;

        public Server(EndPoint endPoint, int backlog)
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _serverSocket.Bind(endPoint);
            _serverSocket.Listen(backlog);
        }

        public async Task<string> ReceiveAsync()
        {
            _clientSocket = await _serverSocket.AcceptAsync();

            int readBytes;
            var buffer = new byte[BufferSize];
            var requestBuilder = new StringBuilder();
            do
            {
                readBytes = await _clientSocket.ReceiveAsync(buffer);
                var request = Encoding.UTF8.GetString(buffer, 0, readBytes);
                requestBuilder.Append(request);
            }
            while (readBytes > 0);

            return requestBuilder.ToString();
        }

        public async Task SendAsync(string responce)
        {
            if (_clientSocket != null)
            {
                var responceBytes = Encoding.UTF8.GetBytes(responce);
                await _clientSocket.SendAsync(responceBytes);

                _clientSocket.Dispose();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
