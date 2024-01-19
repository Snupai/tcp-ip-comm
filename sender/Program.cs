using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using CommandLine;
using System.Text;
using static System.Console;

namespace sender
{
    internal class Program
    {
        class Options
        {
            [Option('p', "port", Required = false, Default = 50000, HelpText = "Port to listen on. If not defined will use standard port \"50000\".")]
            public int Port { get; set; }
            [Option('i', "ip", Required = false, Default = "127.0.0.1", HelpText = "IP to listen on. If not defined will use the standard IP \"127.0.0.1\".")]
            public string Ip { get; set; }
            [Option('m', "message", Required = false, HelpText = "Message to send.")]
            public string Message { get; set; }
            [Option('k', "keep-alive", Required = false, Default = false, HelpText = "Keep the connection alive. If not defined will close the connection after the message was sent.")]
            public bool KeepAlive { get; set; }
        }

        private static int port = 50000;
        private static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private static IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

        static async Task Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed<Options>(o => options = o)
                  .WithNotParsed<Options>(e => Environment.Exit(42));

            WriteLine($"{options.KeepAlive}");
            if (options.Message == null)
            {
                if (options.KeepAlive != true)
                {
                    WriteLine("Error: No message defined or keep-alive not toggled.");
                    Environment.Exit(42);
                }
            }


            if (options.Ip != null)
            {
                try
                {
                    ipAddress = IPAddress.Parse(options.Ip);
                    ipEndPoint = new IPEndPoint(ipAddress, options.Port);
                }
                catch (Exception e)
                {
                    WriteLine($"Error: {e.Message}");
                    Environment.Exit(42);
                }
            }
            if (options.KeepAlive)
            {
                while (true)
                {
                    Write("Message: ");
                    var message = ReadLine();
                    if (message == null)
                    {
                        WriteLine("Message can't be empty!");
                    }
                    else if (message == "/exit")
                    {
                        Environment.Exit(69);
                    }
                    else if (message == "/help")
                    {
                        WriteLine("Commands:");
                        WriteLine("/exit - Exit the program");
                        WriteLine("/help - Show this help message");
                    }
                    else
                    {
                       await client.SendenAsync(ipEndPoint, message);
                    }
                }
            }

            await client.SendenAsync(ipEndPoint, options.Message);
        }

        static public class client
        {
            static public async Task SendenAsync(IPEndPoint ipEndPoint, string message)
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(ipEndPoint);
                await using NetworkStream stream = client.GetStream();

                // Nachricht senden
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBytes);

                // Nachricht empfangen
                byte[] buffer = new byte[1024];
                int received = await stream.ReadAsync(buffer);

                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"Received: \"{receivedMessage}\"");
            }
        }
    }
}
