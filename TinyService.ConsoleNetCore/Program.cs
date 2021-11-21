using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TinyService.Common;

namespace TinyService.ConsoleNetCore
{
    class Program
    {
        private static TcpClient _tcpClient;
        private static string _userId;
        private static string _targetUserId;

        static void Main(string[] args)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect("localhost", 5060);

            Console.WriteLine("Console client was launched!");

            EnterUserId();

            IdentifyTargetUser();

            SendListeningServer();

            SendMessage();

            Console.ReadKey(true);
        }

        private static void SendListeningServer()
        {
            Task.Factory.StartNew(() =>
            {
                var messageHanlder = new MessageHandler();

                while (true)
                {
                    var buffer = new byte[128];

                    _tcpClient.GetStream().Read(buffer, 0, buffer.Length);

                    messageHanlder.HandleRawMessage(buffer);

                    if (messageHanlder.IsCompletedMessage())
                    {
                        HandlerServerMessage(messageHanlder.GetMessage());
                        messageHanlder.ClearCache();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private static void HandlerServerMessage(string builderString)
        {
            //Removing <START> 
            var tempString = builderString.Remove(0, 7);
            // <END>
            builderString = tempString.Remove(tempString.Length - 5, 5);

            var dataDTo = JsonSerializer.Deserialize<MessageDto>(builderString);

            if (dataDTo.MessageType == (int) MessageType.ServerMessage)
            {
                PrintMessage(dataDTo.Message);
            }
        }

        private static void PrintMessage(string dataMessage)
        {
            Console.WriteLine("You receive message:");
            Console.WriteLine(dataMessage);
        }

        private static void SendMessage()
        {
            Console.WriteLine("Send a message:");

            while (true)
            {
                var message = Console.ReadLine();

                SendMessage(message, (int) MessageType.SendMessage);
            }
        }

        private static void SendMessage(string message, int sendMessage)
        {
            var messageToSend = BuildMessage(message, sendMessage);

            SendMessageOverTcp(messageToSend);
        }

        private static void SendMessageOverTcp(string messageToSend)
        {
            try
            {
                var strBytes = Encoding.UTF8.GetBytes(messageToSend);

                _tcpClient.GetStream().Write(strBytes, 0, strBytes.Length);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private static string BuildMessage(string message, int messageType)
        {
            var builder = new StringBuilder();

            builder.Append("<START>");

            var dtoObject = new MessageDto
            {
                Message = message, 
                MessageType = messageType,
                User = _userId,
                Target = _targetUserId
            };

            var str = JsonSerializer.Serialize(dtoObject);

            builder.Append(str);

            builder.Append("<END>");

            return builder.ToString();
        }

        private static void IdentifyTargetUser()
        {
            Console.Write("Who do you want to start chat:");
            _targetUserId = Console.ReadLine();

            SendMessage(_targetUserId, (int) MessageType.SetTarget);

            Console.Clear();
        }

        private static void EnterUserId()
        {
            Console.Write("Enter your id:");
            _userId = Console.ReadLine();

            SendMessage(_userId, (int) MessageType.Greetings);

            Console.Clear();
        }
    }
}
