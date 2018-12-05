using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Tello.NET
{
	public interface ITelloClient
	{
		void Connect();
		void Disconnect();
		void Send(string command);
		void ListenForResponses(Action<string> onResponseReceived);
	}

	public class TelloClient : ITelloClient, IDisposable
	{
		private const int LocalPort = 9000;

		private readonly IPAddress ipAddress;
		private readonly int port;
		private readonly IPEndPoint ipEndPoint;

		private UdpClient udpClient;

		public TelloClient(string ipAddress, int port) : this(IPAddress.Parse(ipAddress), port)
		{ }

		public TelloClient(IPAddress ipAddress, int port)
		{
			this.ipAddress = ipAddress;
			this.port = port;
			this.ipEndPoint = new IPEndPoint(ipAddress, port);
		}

		public void Connect()
		{
			udpClient = new UdpClient(LocalPort);
			udpClient.Connect(ipEndPoint);

			//int port = 9617;
			//int port0 = ((int)(port / 1000) % 10) << 4 | ((int)(port / 100) % 10);
			//int port1 = ((int)(port / 10) % 10) << 4 | ((int)(port / 1) % 10);
			//string connectCommand = $"conn_req:{Convert.ToChar(port0)}{Convert.ToChar(port1)}";

			string connectCommand = "conn_req:\x96\x17";

			Console.WriteLine($"Connecting: {connectCommand}");

			Send(connectCommand);
		}

		public void Disconnect()
		{
			udpClient.Close();
		}

		public void Send(string command)
		{
			byte[] connectPacket = Encoding.UTF8.GetBytes(command);

			udpClient.Send(connectPacket, connectPacket.Length);
		}

		public void ListenForResponses(Action<string> onResponseReceived)
		{
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

			byte[] rawData = udpClient.Receive(ref remoteIpEndPoint);
			string response = Encoding.UTF8.GetString(rawData);

			onResponseReceived(response);
		}

		public void Dispose()
		{
			if (udpClient != null)
			{
				udpClient.Close();
				udpClient.Dispose();
			}
		}
	}
}
