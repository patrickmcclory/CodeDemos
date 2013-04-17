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
        private static HC_SR04 sensor;

        public static void Main()
        {
            // write your code here

            NetduinoGo.Button button = new NetduinoGo.Button(SecretLabs.NETMF.Hardware.NetduinoGo.GoSockets.Socket1);
            ShieldBase sb = new ShieldBase(SecretLabs.NETMF.Hardware.NetduinoGo.GoSockets.Socket2);

            sensor = new HC_SR04(Cpu.Pin.GPIO_Pin1, Cpu.Pin.GPIO_Pin0);

            button.ButtonPressed += button_ButtonPressed;
            
        }

        static void button_ButtonPressed(object sender, bool isPressed)
        {
            long ticks = sensor.Ping();
            if (ticks > 0L)
            {
                double inches = sensor.TicksToInches(ticks);
            }

        }

    }
}
