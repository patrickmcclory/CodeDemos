using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using MicroLiquidCrystal;


namespace RightScale.phbDemo.NetduinoPlus1
{
    public class Program
    {
        static HC_SR04 sensor;
        

        public static void Main()
        {
            var lcdProvider = new GpioLcdTransferProvider(Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D7);
            var lcd = new Lcd(lcdProvider);

            lcd.Begin(16, 2);
            lcd.Write("PHB Detector", "...now loading");

            InitializeNetwork();
            HC_SR501 motionTrigger = new HC_SR501(SecretLabs.NETMF.Hardware.NetduinoPlus.AnalogChannels.ANALOG_PIN_A0);
            sensor = new HC_SR04(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);

            try
            {
                var currentTime = NtpClient.GetNetworkTime();
                Microsoft.SPOT.Hardware.Utility.SetLocalTime(currentTime);
                lcd.Write("NTP Time set:", DateTime.Now.ToString("HH:mm:ss"));
            }
            catch (Exception ex)
            {
                lcd.Clear();
                lcd.Write("Couldn't set", "network time");
                Thread.Sleep(2000);
            }

            
            while (true)
            {
                lcd.Clear();
                lcd.Write("PHB Search", DateTime.Now.ToString("HH:mm:ss"));
                while(motionTrigger.sense(50) == false)
                {
                    Thread.Sleep(500);
                    lcd.Clear();
                    lcd.Write("PHB Search" , "safe at: " + DateTime.Now.ToString("HH:mm:ss"));
                }

                lcd.Clear();
                lcd.Write("PHB sited: ", DateTime.Now.ToString("HH:mm:ss"));

                long ticks = sensor.Ping();
                if (ticks > 0L)
                {
                    Thread.Sleep(250);
                    double inches = sensor.TicksToInches(ticks);
                    lcd.Clear();

                    string printString = Properties.Resources.GetString(Properties.Resources.StringResources.defaultDisplayString) ;
                    
                    if (inches.ToString().Length > 12)
                    {
                        printString = inches.ToString().Substring(0, 12);
                    }
                    else
                    {
                        printString = inches.ToString();
                    }
                    lcd.Clear();
                    lcd.Write("PHB Approaching", printString);
                }
            }
            
        }

        public static string ipAddress;
        public static string netmask;
        public static string gateway;

        public static void InitializeNetwork()
        {
            bool networkValid = false;

            while (!networkValid)
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        Debug.Print("IP Address: " + networkInterface.IPAddress);
                        Debug.Print("Subnet mask " + networkInterface.SubnetMask);

                        if (networkInterface.IPAddress != "0.0.0.0")
                        {
                            System.Net.WebRequest request = HttpWebRequest.Create(new Uri("http://www.google.com"));
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                networkValid = true;
                                break;
                            }
                        }
                        else
                        {

                            if (!networkInterface.IsDhcpEnabled)
                            {
                                networkInterface.EnableDhcp();
                            }
                            Thread.Sleep(20000);
                            break;
                        }
                    }
                }
            }

        }

    }
}
