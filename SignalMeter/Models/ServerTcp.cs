using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SignalMeter
{

    public static class ServerTcp
    {
        static public void Start(ref byte[] value)
        {
            const string ip = "127.0.0.1";
            const int port = 8080;

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(5);

            while (true)
            {
                var listener = tcpSocket.Accept();
                var buffer = new byte[256];
                var size = 0;
                var data = new StringBuilder();

                do
                {
                    size = listener.Receive(buffer);
                    data.Append(Encoding.UTF8.GetString(buffer, 0, size));
                } while (listener.Available > 0);

                if (value != null)
                {
                    if (data.ToString() == "mvdata") listener.Send(value);
                    else listener.Send(Encoding.UTF8.GetBytes("command_error"));
                }
                else listener.Send(Encoding.UTF8.GetBytes("no_data"));

                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
        }
    }
}

