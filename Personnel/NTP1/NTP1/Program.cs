using System;
using System.Net;
using System.Net.Sockets;

namespace NTP1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ntpServer = "0.ch.pool.ntp.org";
            byte[] ntpData = new byte[48];
            ntpData[0] = 0X1b; // NTP request header

            IPEndPoint ntpEndpoint = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);
            UdpClient ntpClient = new UdpClient();
            ntpClient.Connect(ntpEndpoint);

            ntpClient.Send(ntpData, ntpData.Length);
            ntpData = ntpClient.Receive(ref ntpEndpoint);
            DateTime utcTime = ToDateTime(ref ntpData); // UTC time from NTP
            Console.WriteLine("Heure UTC (NTP) : " + utcTime.ToString("dd/MM/yyyy HH:mm:ss"));

            DateTime localTime = utcTime.ToLocalTime(); // Convert to local time
            Console.WriteLine("Heure locale (à partir de l'heure UTC) : " + localTime.ToString("dd/MM/yyyy HH:mm:ss"));

            DateTime gmtTime = localTime.ToUniversalTime().AddHours(-1); // Adjust by -1 hour to get GMT from CET
            Console.WriteLine("Heure GMT : " + gmtTime.ToString("dd/MM/yyyy HH:mm:ss"));

            localTime = gmtTime.AddHours(1); // Adjust by +1 hour for Switzerland time zone
            Console.WriteLine("Heure locale (à partir de l'heure GMT) : " + localTime.ToString("dd/MM/yyyy HH:mm:ss"));

            ntpClient.Close();
        }

        public static DateTime ToDateTime(ref byte[] ntpData)
        {
            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
            return networkDateTime;
        }
    }
}
