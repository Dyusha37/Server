using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        static string[] args;
        public Form1(string[] s)
        {
            args = s;
            InitializeComponent();
        }
        Thread th;
        Semaphore sem;

        private void Form1_Load(object sender, EventArgs e)
        {


            th = new Thread(run);
            th.Start();


        }
        void run()
        {

            try
            {
                sem = Semaphore.OpenExisting("semof");
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Invoke(new Action(() => this.Text = "Memory-mapped file does not exist. Run Process A first."));
            }
            using (PipeStream pipeClient = new AnonymousPipeClientStream(PipeDirection.In, args[0]))
            {
                using (StreamReader sr = new StreamReader(pipeClient))
                {
                    string temp;

                    do
                    {
                        temp = sr.ReadLine();
                    }
                    while (!temp.StartsWith("SYNC"));
                    while (true)
                    {
                        sem.WaitOne();
                        while ((temp = sr.ReadLine()) != null)
                        {
                            int x = Convert.ToInt32(temp);
                            int y = Convert.ToInt32(sr.ReadLine());
                            Invoke(new Action(() => this.Text = (x.ToString() + ";" + y.ToString())));
                            Invoke(new Action(() => this.Paint(x, y)));
                        }
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            th.Abort();
        }
        void Paint(int x, int y)
        {
            Graphics grfx = CreateGraphics();
            grfx.DrawRectangle(p(), rect1(x, y));
        }
        Rectangle rect1(int x, int y)
        {
            Random rand2 = new Random();
            return new Rectangle(x, y, rand2.Next(0, this.Width / 2), rand2.Next(0, this.Height / 2)); ;
        }
        Pen p()
        {
            Random rand1 = new Random();
            return new Pen(Color.FromArgb(rand1.Next(0, 255), rand1.Next(0, 255), rand1.Next(0, 255), rand1.Next(0, 255)), 3f);
        }
    }
}
