using System;
using System.Net.Sockets;
using System.Threading;

namespace HmiPublisher
{
    public class PublisherServer
    {
        CancellationTokenSource _cancellation;

        public PublisherServer()
        {
        }

        public void Send(string ipAddresses, string command)
        {
            _cancellation = new CancellationTokenSource();
            CancellationToken token = _cancellation.Token;
            ConnectAndSend(ipAddresses, command);
        }

        public bool ConnectAndSend(string server, String message)
        {
            try
            {
                var port = 13700;
                var client = new TcpClient(server, port);
                var data = System.Text.Encoding.ASCII.GetBytes(message);
                var stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);
                data = new Byte[256];
                String responseData = String.Empty;
                Int32 bytes = 0;
                bytes = stream.Read(data, 0, data.Length);

                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);
                stream.Close();
                client.Close();

                return data[0] == 1;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            Console.Read();
            return false;
        }

        void CancelTask(CancellationTokenSource tokenSource)
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }
        }
    }
}
