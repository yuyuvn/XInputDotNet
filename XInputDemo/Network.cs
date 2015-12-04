using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;

namespace XInputDemo
{
    class Network
    {
        #region ConnectedHandler property
        private volatile Action<string, NetworkStream> _ConnectedHandler;
        public Action<string, NetworkStream> ConnectedHandler
        {
            get { return _ConnectedHandler; }
            set { _ConnectedHandler = value; }
        }
        #endregion

        public TcpListener Connection;

        private bool SocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }

        public void StartListener(int port)
        {
            if (Connection != null && SocketConnected(Connection.Server)) throw new InvalidOperationException("Connection is not closed");
            try
            {
                IPAddress localAddr = IPAddress.Any;

                // TcpListener server = new TcpListener(port);
                Connection = new TcpListener(localAddr, port);

                // Start listening for client requests.
                Connection.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data;

                // Enter the listening loop.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = Connection.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        Console.WriteLine("Received: {0}", data);

                        if (data.Substring(0, 8) == "HED-Robo")
                        {
                            var conntectedHandler = ConnectedHandler;
                            if (conntectedHandler != null)
                            {
                                conntectedHandler(data, stream);
                            }
                        }
                        else
                        {
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("This is HED-Capcom. You are not Hed-Robo!");
                            stream.Write(msg, 0, msg.Length);
                            client.Close();
                            break;
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                Connection.Stop();
            }
        }

        public void SendBroadcast(string IP, int port, string message)
        {
            Console.WriteLine("Sending broadcast to {0}:{1}...",IP,port);

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.Blocking = false;
            //IPAddress broadcast = IPAddress.Parse("192.168.1.255");
            IPAddress broadcast = IPAddress.Parse(IP);

            byte[] sendbuf = Encoding.ASCII.GetBytes(message);
            IPEndPoint ep = new IPEndPoint(broadcast, port);

            s.SendTo(sendbuf, ep);

            s.Close();
        }

        public void Stop()
        {
            Console.WriteLine("Stoped");
            Connection.Stop();
        }
    }
}
