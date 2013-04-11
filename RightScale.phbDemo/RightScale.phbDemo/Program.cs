using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoGo;
using NetduinoGo;

namespace RightScale.phbDemo
{
    public class Program
    {
        private static int selCol = 0;

        public static void Main()
        {
            // write your code here
            //SecretLabs.NETMF.Hardware.NetduinoGo.GoSockets.Socket1;
            NetduinoGo.Button button1 = new NetduinoGo.Button();
            NetduinoGo.RgbLed rgbLED1 = new RgbLed();
            Potentiometer pot1 = new Potentiometer();

            byte r = 0, g = 0, b = 0;
            double oldPotVal = pot1.GetValue();

            button1.ButtonReleased += new NetduinoGo.Button.ButtonEventHandler(button1_ButtonReleased);

            while (true)
            {
                rgbLED1.SetColor(r, g, b);

                if (pot1.GetValue() != oldPotVal)
                {
                    switch (selCol)
                    {
                        case 0:
                            r = (byte)(pot1.GetValue() * 255);
                            break;
                        case 1:
                            g = (byte)(pot1.GetValue() * 255);
                            break;
                        case 2:
                            b = (byte)(pot1.GetValue() * 255);
                            break;
                    }
                }
            }
        }

        private static void button1_ButtonReleased(object sender, bool buttonState)
        {
            if (selCol < 2)
            {
                selCol++;
            }
            else
            {
                selCol = 0;
            }
        }

    }
}
