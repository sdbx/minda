using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class NetworkManager : MonoBehaviour {

	public string ipAddress = "127.0.0.1";
	public int port = 0;

	private Socket _socket;

	public bool Connect () 
	{
		_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		try
        {
            IPAddress ipAddr = IPAddress.Parse(ipAddress);
            IPEndPoint ipendPoint = new IPEndPoint(ipAddr, port);
            _socket.Connect(ipendPoint);
			return true;

        }
        catch(SocketException SocketException)
        {
            Debug.Log("소켓 연결 에러 ! : " + SocketException.ToString());
            return false;
        }
	}
	public void StartReceive()
	{
		Thread t = new Thread(new ThreadStart(Receive));
		t.Start();
	}
	private void Receive()
	{
		byte[] buffer = new byte[1024];
		_socket.Receive(buffer);
		string result = System.Text.Encoding.UTF8.GetString(buffer);
		Debug.Log(result);

	}
	// Update is called once per frame
    void Update () 
	{
		
	}
}
