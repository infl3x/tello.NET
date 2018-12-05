using System;
using System.Net;
using System.Threading.Tasks;
using CommandLine = System.Console;

namespace Tello.NET.Console
{
	public class Program
	{
		private static readonly IPAddress DefaultIp = IPAddress.Parse("192.168.10.1");
		private static readonly int DefaultPort = 8889;

		public static void Main(string[] args)
		{
			CommandLine.WriteLine($"Enter Tello IPv4 address ({DefaultIp}): ");
			IPAddress ipAddress = ReadAndParse(l => IPAddress.Parse(l), DefaultIp);

			CommandLine.WriteLine($"Enter Tello port ({DefaultPort}): ");
			int port = ReadAndParse(l => Int32.Parse(l), DefaultPort);

			CommandLine.WriteLine("Attempting to connect...");

			try
			{
				ITelloClient telloClient = Connect(ipAddress, port);

				string command = null;
				do
				{
					CommandLine.WriteLine("Enter command:");
					command = CommandLine.ReadLine();

					telloClient.Send(command);

				} while (!String.IsNullOrEmpty(command));
			}
			catch (Exception e)
			{
				CommandLine.WriteLine($"An error has occurred: ${e}");
			}

			CommandLine.ReadLine();
		}

		private static ITelloClient Connect(IPAddress ipAddress, int port)
		{
			ITelloClient telloClient = new TelloClient(ipAddress, port);
			telloClient.Connect();

			Task.Run(() => telloClient.ListenForResponses(OnResponseReceived));

			CommandLine.WriteLine("Connected successfully!");

			return telloClient;
		}

		private static T ReadAndParse<T>(Func<string, T> parse, T defaultValue)
		{
			string line = CommandLine.ReadLine();

			if (String.IsNullOrEmpty(line))
				return defaultValue;

			while (true)
			{
				try
				{
					return parse(line);
				}
				catch (Exception e)
				{
					CommandLine.WriteLine(e.Message);
					CommandLine.WriteLine("Please try enter data in the correct format");
				}
			}
		}

		private static void OnResponseReceived(string response)
		{
			CommandLine.WriteLine("Response recieved: ");
			CommandLine.WriteLine(response);
		}
	}
}
