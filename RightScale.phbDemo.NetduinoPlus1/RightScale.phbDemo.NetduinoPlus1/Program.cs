using System;
using System.Net;
using System.Collections;
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
    public enum phbVector
    {
        TowardYou,
        AwayFromYou, 
        NotSure
    }

    public class Program
    {
        static HC_SR04 distanceSensor;
        static HC_SR501 motionSensor;
        static GpioLcdTransferProvider lcdProvider;
        static Lcd lcd;
        static bool lcdInitialized = false;
        static int measurementSetSize = 10;
        static int huntTimeDelay = 250;
        public static double maxDistance = 240d;
        public static double minDistance = 12d;
        public static int measurementTolerance = 24;
        private static int motionSensorThreshold = 50;
        private static bool isDebug { get; set; }

        public static void Main()
        {
            //set debug = true;
            isDebug = true;

            InitializeLCDDisplay();

            InitializeNetwork();

            InitializeTime();

            InitializeMotionSensor();

            WriteLCD("initializing...", "distance sensor");
            distanceSensor = new HC_SR04(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);
            WriteLCD("distance sensor", "initialized");

            Thread.Sleep(2000);

            while (true)
            {
                WriteLCD("PHB Search", DateTime.Now.ToString("HH:mm:ss"));
                while(!MeasureMotion())
                {
                    Thread.Sleep(500);
                    WriteLCD("PHB Search", "safe at: " + DateTime.Now.ToString("HH:mm:ss"));
                }

                WriteLCD("PHB sited: ", DateTime.Now.ToString("HH:mm:ss"));

                bool alarmed = false;

                switch (MeasureVector())
                {
                    case phbVector.TowardYou:
                        SendMessageToEndpoint("phbalarm");
                        alarmed = true;
                        WriteLCD("Message Sent", "phbalarm!");
                        Thread.Sleep(5000);
                        break;
                    case phbVector.AwayFromYou:
                        SendMessageToEndpoint("You're off the hook this time");
                        WriteLCD("Message Sent", "phb leaving area");
                        Thread.Sleep(5000);
                        break;
                    case phbVector.NotSure:
                        SendMessageToEndpoint("We're really not sure what you should do here...");
                        WriteLCD("Message Sent", "not sure...");
                        Thread.Sleep(5000);
                        break;
                    default:
                        break;
                }

                if (isDebug && !alarmed)
                {
                    WriteLCD("System in", "debug mode");
                    Thread.Sleep(1000);
                    SendMessageToEndpoint("phbalarm");

                }
            }
        }
        
        private static bool MeasureMotion()
        {
            bool retVal = false;
            if (motionSensor.sense(motionSensorThreshold))
            {
                retVal = true;
            }
            return retVal;
        }

        private static phbVector MeasureVector()
        {
            double[] measurements = new double[measurementSetSize];

            for (int i = 0; i < measurementSetSize; i++)
            {
                long ticks = distanceSensor.Ping();
                if (ticks > 0L)
                {
                    Thread.Sleep(huntTimeDelay);
                    double inches = distanceSensor.TicksToInches(ticks);
                    measurements[i] = inches;

                    string printString = Properties.Resources.GetString(Properties.Resources.StringResources.defaultDisplayString);

                    if (inches.ToString().Length > 12)
                    {
                        printString = inches.ToString().Substring(0, 12);
                    }
                    else
                    {
                        printString = inches.ToString();
                    }

                    WriteLCD("PHB Approaching", printString);
                }
            }
            return AnalyzeVectorData(measurements);
        }
        
        private static phbVector AnalyzeVectorData(double[] measurements)
        {
            double currentValue;
            double previousValue;
            int dataSetLength = measurements.Length;

            if (dataSetLength > 1)
            {
                int score = 0;
                ArrayList validData = new ArrayList();

                for (int i = 0; i < dataSetLength; i++)
                {
                    if (measurements[i] > maxDistance || measurements[i] < minDistance)
                    {
                        //not a valid measurement
                    }
                    else
                    {
                        validData.Add(measurements[i]);
                    }
                }

                int arrayListLength = validData.Count;
                if (arrayListLength > 1)
                {
                    double totalDistance = (double)validData[0] - (double)validData[arrayListLength - 1];

                    score += (int)totalDistance;

                    for (int i = 0; i < arrayListLength; i++)
                    {
                        if (i > 0)
                        {
                            currentValue = (double)validData[i];
                            previousValue = (double)validData[i - 1];
                            score += (int)(previousValue - currentValue);
                        }
                    }

                    if (score > measurementTolerance)
                    {
                        return phbVector.AwayFromYou;
                    }
                    else if(score < (-1 * measurementTolerance))
                    {
                        return phbVector.TowardYou;
                    }
                    else
                    {
                        return phbVector.NotSure;;
                    }
                }
                else
                {
                    return phbVector.NotSure;
                }
            }
            else
            {
                return phbVector.NotSure;
            }
        }

        #region Initialization methods

        private static void InitializeTime()
        {
            try
            {
                var currentTime = NtpClient.GetNetworkTime();
                Microsoft.SPOT.Hardware.Utility.SetLocalTime(currentTime);
                WriteLCD("NTP Time set:", DateTime.Now.ToString("HH:mm:ss"));
            }
            catch (Exception)
            {
                WriteLCD("Couldn't set", "network time");
                Thread.Sleep(2000);
            }
        }

        private static void InitializeLCDDisplay()
        {
            lcdProvider = new GpioLcdTransferProvider(Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D7);
            lcd = new Lcd(lcdProvider);
            lcd.Begin(16, 2);
            lcdInitialized = true;

            WriteLCD("PHB Detector", "...now loading");
            Thread.Sleep(5000);//show loading dialog for at least 5 sec..
        }

        private static void InitializeMotionSensor()
        {
            WriteLCD("initializing...", "motion sensor");
            motionSensor = new HC_SR501(SecretLabs.NETMF.Hardware.NetduinoPlus.AnalogChannels.ANALOG_PIN_A0);
            WriteLCD("calibrating...", "motion sensor");
            motionSensor.calibrateSensor();
            WriteLCD("motion sensor", "calibrated");
        }

        public static void InitializeNetwork()
        {
            WriteLCD("initializing...", "networking");
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
                            try
                            {
                                System.Net.WebRequest request = HttpWebRequest.Create(new Uri("http://www.google.com"));
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    networkValid = true;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                lcd.Write("exception", ex.Message.Substring(0, 16));
                                Thread.Sleep(5000);
                            }
                        }
                        else
                        {
                            if (!networkInterface.IsDhcpEnabled)
                            {
                                networkInterface.EnableDhcp();
                                //Thread.Sleep(2000);
                                //networkInterface.RenewDhcpLease();
                            }
                            WriteLCD("initializing...", "networking-dhcp");
                            Thread.Sleep(2000);
                            break;
                        }
                    }
                }
            }
            WriteLCD("networking", "initialized");
        }
        #endregion 

        private static void WriteLCD(string line1, string line2)
        {
            if (!lcdInitialized)
            {
                InitializeLCDDisplay();
            }
            lcd.Clear();
            lcd.Write(line1, line2);
        }
        
        private static bool SendMessageToEndpoint(string message)
        {
            bool retVal = false;
           
            using (System.Net.WebRequest request = HttpWebRequest.Create(new Uri("http://netduinoendpoint.cloudlord.com:10282/?message=" + Tools.RawUrlEncode(message))))
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        retVal = true;
                    }
                }
            }
            return retVal;
        }
    }
}
