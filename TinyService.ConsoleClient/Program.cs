using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TinyService.ConsoleClient
{
    class Program
    {
        private static TcpClient _tcpClient;

        static void Main(string[] args)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 5060);

            Console.WriteLine("Console client was launched!");

            EnterUserId();

            IdentifyTargetUser();

            SendListeningServer();

            SendMessage();



            
     

   

            var str = getMesage.Select(x => x.ToString()).Aggregate((s, s1) => s += s1);


            Console.ReadKey(true);
        }

        private static void SendListeningServer()
        {
            Task.Factory.StartNew(() =>
            {
                var builder = new StringBuilder();

                while (true)
                {
                    var buffer = new byte[64];

                    _tcpClient.GetStream().Read(buffer, 0, buffer.Length);

                    var data = Encoding.UTF8.GetString(buffer);

                    builder.Append(data);

                    var builderString = builder.ToString();

                    if (IsCompletedMessage(builderString))
                    {
                        HandlerServerMessage(builderString);

                        builder.Clear();
                    }
                }
            }, TaskCreationOptions.LongRunning).Wait();
        }

        private static void HandlerServerMessage(string builderString)
        {
            //Removing <START> 
            var tempString = builderString.Remove(0, 7);
            // <END>
            builderString = tempString.Remove(tempString.Length - 5, 5);

            var dataDictionary = JsonSerializer.Deserialize<Dictionary<string,string>>(builderString);

            var dataMessage = dataDictionary["message"];
            var dataMessageType = dataDictionary["messageType"];

            if (dataMessageType == MessageType.)
            {
                
            }
        }

        private static bool IsCompletedMessage(string message)
        {
            return message.StartsWith("<START>") && message.EndsWith("<END>");
        }

        private static void SendMessage()
        {
            Console.WriteLine("Send a message:");

            while (true)
            {
                var message = Console.ReadLine();

                SendMessage(message);
            }
        }

        private static void SendMessage(string message)
        {
            var messageToSend = BuildMessage(message, 1);

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

            var dtoObject = new {message = message, messageType = messageType};

            var str = JsonSerializer.Serialize(dtoObject);

            builder.Append(str);

            builder.Append("<END>");

            return builder.ToString();
        }

        private static void IdentifyTargetUser()
        {
            Console.Write("Whom do you want start chat:");
            var targetUserId = Console.ReadLine();

            Console.Clear();
        }

        private static void EnterUserId()
        {
            Console.Write("Enter your id:");
            var userId = Console.ReadLine();

            Console.Clear();
        }
    }
}
