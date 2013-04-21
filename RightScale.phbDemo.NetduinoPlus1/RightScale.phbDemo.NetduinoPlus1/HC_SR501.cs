using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace RightScale.phbDemo.NetduinoPlus1
{
    class HC_SR501
    {
        public int maxLoops { get; set; }
        public int loopWaitTime { get; set; }

        private bool isCalibrated = false;

        private AnalogInput triggerPort;

        public HC_SR501(Cpu.AnalogChannel outPort)
        {
            this.triggerPort = new AnalogInput(outPort);
            this.maxLoops = -1; //loop indefinitely
            this.loopWaitTime = 200;
        }

        public void calibrateSensor()
        {
            if (!this.isCalibrated)
            {
                Thread.Sleep(60000);
                this.isCalibrated = true;
            }
        }

        public bool sense(int thresholdPct)
        {
            calibrateSensor();

            bool retVal = false;

            int loopCount = 0;

            while (!retVal)
            {
                if (this.maxLoops < 0 || loopCount < this.maxLoops)
                {
                    double val = this.triggerPort.Read() * 100;
                    if (val > (double)thresholdPct)
                    {
                        retVal = true;
                        break;
                    }
                    if (this.maxLoops >= 0)
                    {
                        loopCount++;
                    }
                    Thread.Sleep(this.loopWaitTime);
                }
            }

            return retVal;
        }
    }
}
