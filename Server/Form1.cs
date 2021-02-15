using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Form1 : Form
    {
        Semaphore sem = new Semaphore(0, 2, "semof");
        static Process prc;
        Thread th;
        static AnonymousPipeServerStream pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        StreamWriter sw = new StreamWriter(pipeServer);
        AutoResetEvent Evnt = new AutoResetEvent(false);
        int x, y;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            th = new Thread(run);
            th.Start();

        }
        static void StartClient(string clientHandle)
        {
            ProcessStartInfo info = new ProcessStartInfo(@"Client\Client\bin\Debug\Client.exe");
            info.Arguments = clientHandle;
            info.UseShellExecute = false;
            prc = Process.Start(info);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            x = e.X;
            y = e.Y;
            this.Text = (x.ToString() + ";" + y.ToString());
            Evnt.Set();

        }

        void run()
        {
            using (pipeServer)
            {
                StartClient(pipeServer.GetClientHandleAsString());
                pipeServer.DisposeLocalCopyOfClientHandle();

                using (sw)
                {
                    sw.AutoFlush = true;
                    sw.WriteLine("SYNC");
                    pipeServer.WaitForPipeDrain();
                    while (true)
                    {
                        try
                        {
                            Evnt.WaitOne();
                            sw.WriteLine(x);
                            sw.WriteLine(y);
                            sem.Release();
                        }
                        catch (SemaphoreFullException)
                        {
                        }
                    }
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                th.Abort();
            }
            catch { }
        }
    }
}
