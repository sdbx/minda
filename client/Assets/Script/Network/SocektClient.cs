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
        Disconnected,
        Connecting,
        Connected
    }

    public class SocketClient
    {
        public Queue<string> DataQueue = new Queue<string>();
        public Queue<string> LogQueue = new Queue<string>();
        public Queue<Action> CallbackQuene = new Queue<Action>();
        private Action _connectedCallback;
        public Action CloseSocketCallback;
        public ClientState State = ClientState.Disconnected;

        private Socket _client;
        private byte[] _receiveByte;

        public void Connect(string ip, int port, Action callback)
        {
            try
            {
                State = ClientState.Connecting;
                var ipAddress = IPAddress.Parse(ip);
                _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var remoteEp = new IPEndPoint(ipAddress, port);
                _client.BeginConnect(remoteEp, new AsyncCallback(ConnectCallback), _client);
                _connectedCallback = callback;
            }
            catch (Exception e)
            {
                State = ClientState.Disconnected;
                LogQueue.Enqueue(e.Message);
            }
        }

        private void ConnectCallback(IAsyncResult iar)
        {
            try
            {
                _client.EndConnect(iar);
                LogQueue.Enqueue("[Connected]" + _client.RemoteEndPoint.ToString());
                _receiveByte = new byte[_client.ReceiveBufferSize];
                _client.BeginReceive(_receiveByte, 0, _receiveByte.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _client);
                State = ClientState.Connected;
                if (_connectedCallback != null)
                    _connectedCallback();
            }
            catch (Exception e)
            {
                State = ClientState.Disconnected;
                LogQueue.Enqueue(e.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            try
            {
                if (_client.Available == 0 && _client.Poll(1, SelectMode.SelectRead))
                {
                    LogQueue.Enqueue("Disconnect from Poll");
                    Close();
                    return;
                }
                var recv = _client.EndReceive(iar);
                var data = Encoding.UTF8.GetString(_receiveByte, 0, recv);
                data = data.Trim('\0');
                DataQueue.Enqueue(data);
                LogQueue.Enqueue("[Receive]" + data);
                _client.BeginReceive(_receiveByte, 0, _receiveByte.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _client);
            }
            catch (Exception e)
            {
                LogQueue.Enqueue(e.Message);
                Close();
            }
        }

        public void SendData(string data)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                _client.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), _client);
                LogQueue.Enqueue("[Send data]" + data);
            }
            catch (Exception e)
            {
                LogQueue.Enqueue(e.Message);
                Close();
            }
        }

        private void SendCallback(IAsyncResult ias)
        {
            var handler = (Socket)ias.AsyncState;
            var bytesSent = handler.EndSend(ias);
            LogQueue.Enqueue("[Sent Bytes]" + bytesSent);
        }

        public void Close()
        {
            LogQueue.Enqueue("[Disconnected]");
            State = ClientState.Disconnected;
            CallbackQuene.Enqueue(CloseSocketCallback);
            if (_client != null)
            {
                try { _client.Shutdown(SocketShutdown.Both); }
                catch (Exception) { }
                _client.Close();
            }
        }
    }
}
