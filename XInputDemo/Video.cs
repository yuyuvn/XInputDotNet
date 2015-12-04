using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XInputDemo
{
    class Video
    {
        private volatile Process Process;
        public void Start(int port)
        {
            Process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ffplay.exe");
            startInfo.Arguments = String.Format("-rtsp_flags listen rtsp://0.0.0.0:{0}/live.sdp?tcp -analyzeduration 500", port);
            Process.StartInfo = startInfo;
            Process.Start();
        }

        public void Stop()
        {
            if (Process != null)
            {
                try
                {
                    Process.Kill();
                }
                catch { }
            }
        }

        ~Video()
        {
            Stop();
        }
    }
}
