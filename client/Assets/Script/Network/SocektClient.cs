using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Network
{
    public enum ClientState
    {
        DISCONNECTED,
        CONNECTING,
        CONNECTED
    }

    public class SocketClient
    {
        public Queue<string> dataQueue = new Queue<string>();
        public Queue<string> logQueue = new Queue<string>();
        public Queue<Action> callbackQuene = new Queue<Action>(); 
        private Action connectedCallback;
        public Action closeSocketCallback;
        public ClientState state = ClientState.DISCONNECTED;

        private Socket client;
        private byte[] receiveByte;

        public void Connect(string ip, int port, Action callback)
        {
            try
            {
                state = ClientState.CONNECTING;
                IPAddress ipAddress = IPAddress.Parse(ip);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectedCallback = callback;
            }
            catch (Exception e)
            {
                state = ClientState.DISCONNECTED;
                logQueue.Enqueue(e.Message);
            }
        }

        void ConnectCallback(IAsyncResult iar)
        {
            try
            {
                client.EndConnect(iar);
                logQueue.Enqueue("[Connected]" + client.RemoteEndPoint.ToString());
                receiveByte = new byte[client.ReceiveBufferSize];
                client.BeginReceive(receiveByte, 0, receiveByte.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
                state = ClientState.CONNECTED;
                if (connectedCallback != null)
                    connectedCallback();
            }
            catch (Exception e)
            {
                state = ClientState.DISCONNECTED;
                logQueue.Enqueue(e.Message);
            }
        }

        void ReceiveCallback(IAsyncResult iar)
        {
            try
            {
                if (client.Available == 0 && client.Poll(1, SelectMode.SelectRead))
                {
                    logQueue.Enqueue("Disconnect from Poll");
                    Close();
                    return;
                }
                int recv = client.EndReceive(iar);
                string data = Encoding.UTF8.GetString(receiveByte, 0, recv);
                data = data.Trim('\0');
                dataQueue.Enqueue(data);
                logQueue.Enqueue("[Receive]" + data);
                client.BeginReceive(receiveByte, 0, receiveByte.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            catch (Exception e)
            {
                logQueue.Enqueue(e.Message);
                Close();
            }
        }

        public void SendData(string data)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                client.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
                logQueue.Enqueue("[Send data]" + data);
            }
            catch (Exception e)
            {
                logQueue.Enqueue(e.Message);
                Close();
            }
        }

        void SendCallback(IAsyncResult ias)
        {
            Socket handler = (Socket)ias.AsyncState;
            int bytesSent = handler.EndSend(ias);
            logQueue.Enqueue("[Sent Bytes]" + bytesSent);
        }

        public void Close()
        {
            logQueue.Enqueue("[Disconnected]");
            state = ClientState.DISCONNECTED;
            callbackQuene.Enqueue(closeSocketCallback);
            if (client != null)
            {
                try { client.Shutdown(SocketShutdown.Both); }
                catch (Exception e) { }
                client.Close();
            }
        }
    }
}