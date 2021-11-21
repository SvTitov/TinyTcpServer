using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using TinyService.Common;

namespace TinyService.WebApi
{
    class Program
    {
        private static TcpListener _listener;
        private static ConcurrentDictionary<string, TcpClient> _concurrentDictionary = new ConcurrentDictionary<string, TcpClient>();

        static void Main(string[] args)
        {
            var address = IPAddress.Parse("127.0.0.1");
            _listener = new TcpListener(address, 5060);
            _listener.Start();

            Console.WriteLine("Server was launched");

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(state => HandleTcpClient(client), null);
            }
        }

        private static void HandleTcpClient(TcpClient client)
        {
            Console.WriteLine("New Client was connected!");

            var messageHandler = new MessageHandler();

            while (true)
            {
                var byteArray = new byte[128];

                while (client.Connected)
                {

                    client.GetStream().Read(byteArray, 0, byteArray.Length);
                    messageHandler.HandleRawMessage(byteArray);

                    if (messageHandler.IsCompletedMessage())
                    {
                        HandleMessage(messageHandler.GetMessage(), client);
                        messageHandler.ClearCache();
                    }
                }
            }
        }

        private static void HandleMessage(string builderString, TcpClient tcpClient)
        {
            //Removing <START> 
            var tempString = builderString.Remove(0, 7);
            // <END>
            var hasErrorEnd = tempString.Contains("<END>>");
            builderString = tempString.Remove(tempString.Length - (hasErrorEnd ? 6 : 5), tempString.Contains("<END>>") ? 6 : 5);

            var dataDto = JsonSerializer.Deserialize<MessageDto>(builderString);

            if (dataDto.MessageType == (int) MessageType.Greetings)
            {
                SaveUser(dataDto.User, tcpClient);
            }
            if (dataDto.MessageType == (int) MessageType.SendMessage)
            {
                SendMessageTo(dataDto.Target, dataDto.Message);
            }
        }

        private static void SaveUser(string dataUserId, TcpClient tcpClient)
        {
            if (!_concurrentDictionary.ContainsKey(dataUserId))
            {
                _concurrentDictionary.TryAdd(dataUserId, tcpClient);
            }
        }

        private static void SendMessageTo(string destination, string message)
        {
            if (_concurrentDictionary.ContainsKey(destination))
            {
                var value = _concurrentDictionary[destination];
                if (value.Connected)
                {
                    var dto = new MessageDto
                    {
                        Message = message,
                        MessageType = (int) MessageType.ServerMessage
                    };

                    value.GetStream().Write(Encoding.ASCII.GetBytes(WrapMessage(JsonSerializer.Serialize(dto))));
                }
            }
        }

        private static string WrapMessage(string data)
        {
            var builder = new StringBuilder();

            builder.Append("<START>");

            builder.Append(data);

            builder.Append("<END>");

            return builder.ToString();
        }
    }
}
