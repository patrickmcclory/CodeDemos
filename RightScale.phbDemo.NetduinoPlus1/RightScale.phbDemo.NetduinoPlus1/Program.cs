using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
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
            // write your code here
            sensor = new HC_SR04(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);

            var lcdProvider = new GpioLcdTransferProvider(Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D7);
            var lcd = new Lcd(lcdProvider);
            lcd.Begin(16, 2);
            lcd.Write("hello world");
            
            while (true)
            {
                long ticks = sensor.Ping();
                if (ticks > 0L)
                {
                    Thread.Sleep(1000);
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
                    lcd.Write(printString);
                }
            }
        }

        public static void sendAlert()
        {

        }

    }
}
