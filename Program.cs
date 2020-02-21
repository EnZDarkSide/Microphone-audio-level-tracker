using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using NAudio.Wave;
using System.Diagnostics;

namespace Volume
{
    //
    class Program
    {
        private static double audioValueMax = 1;
        private static double audioValueLast = 0;
        private static int RATE = 44100;
        private static int BUFFER_SAMPLES = 1024;
        public static int UPDATE_MILLISECONDS = 15;
        public static int SHUTDOWN_PEAK = 90; //% of volume level

        static void Main(string[] args)
        {
            var waveIn = new WaveInEvent();
            Timer timer = new Timer(timer_Tick, 5, 0, UPDATE_MILLISECONDS);
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(RATE, 1);
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.BufferMilliseconds = (int)((double)BUFFER_SAMPLES / (double)RATE * 1000.0);
            waveIn.StartRecording();
            Console.ReadLine();
        }

        private static void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            float max = 0;

            // interpret as 16 bit audio
            for (int index = 0; index < args.BytesRecorded; index += 2)
            {
                short sample = (short)((args.Buffer[index + 1] << 8) |
                                        args.Buffer[index + 0]);
                var sample32 = sample / 32768f; // to floating point
                if (sample32 < 0) sample32 = -sample32; // absolute value 
                if (sample32 > max) max = sample32; // is this the max value?
            }

            // calculate what fraction this peak is of previous peaks
            if (max > audioValueMax)
            {
                audioValueMax = (double)max;
            }
            audioValueLast = max;
        }

        private static void timer_Tick(object sender)
        {
            double frac = audioValueLast / audioValueMax;

            if (frac * 100.0 > SHUTDOWN_PEAK)
            {
                Console.WriteLine("Shut down the computer");
                //Process.Start("shutdown", "-s -t 0");
            }
        }
    }
}
