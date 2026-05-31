using System.Text;
using System.Net.Sockets;
using System.Text.Json;

namespace ClientProgram
{
    public class Client : IDisposable
    {
        private readonly Socket _socket;
        private readonly string _address;
        private readonly int _port;

        private const int BufferSize = 1024;

        public Client(string address, int port)
        {
            _port = port;
            _address = address;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task OpenConnectionAsync()
        {
            await _socket.ConnectAsync(_address, _port);
        }

        public async Task SendAsync(string request)
        {
            var requestBytes = Encoding.UTF8.GetBytes(request);
            await _socket.SendAsync(requestBytes);

            _socket.Shutdown(SocketShutdown.Send);
        }

        public async Task<string> ReciveAsync()
        {
            int readBytes;
            var buffer = new byte[BufferSize];
            var requestBuilder = new StringBuilder();
            do
            {
                readBytes = await _socket.ReceiveAsync(buffer);
                var request = Encoding.UTF8.GetString(buffer, 0, readBytes);
                requestBuilder.Append(request);
            }
            while (readBytes > 0);

            return requestBuilder.ToString();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
