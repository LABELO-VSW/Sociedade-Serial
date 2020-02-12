using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Timers;
namespace Sociedade_Serial
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort s0 = new SerialPort();
               
        DateTime lastRead = DateTime.Now;

        Timer buffer_timer;

        DateTime buffer_timestamp;


        List<string> htmlTextOutput = new List<string>();
        public MainWindow()
        {
            constants c = new constants();
            
            InitializeComponent();          
            
            this.baudrate.ItemsSource = c.baudRates;
            this.dataBits.ItemsSource = c.dataBits;
            this.stopBits.ItemsSource = c.stopBits;
            this.parity.ItemsSource = c.parity;
            this.flowControl.ItemsSource = c.flowControl;
            this.port.ItemsSource = SerialPort.GetPortNames();
            this.disconect.IsEnabled = false;
            this.send.IsEnabled = false;
            this.execute.IsEnabled = false;
            this.buffer.IsEnabled = false;
            this.finish.IsEnabled = false;
            
            buffer_timer = new Timer() { Interval = 1};
            buffer_timer.Elapsed += Buffer_timer_Elapsed; ;

        }

        private void UpdateScreen_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void execute_Click(object sender, RoutedEventArgs e)
        {

        }

        private void conect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s0 = new SerialPort();
                s0.BaudRate = int.Parse(this.baudrate.Text);
                s0.Parity = (Parity)this.parity.SelectedIndex;
                s0.Handshake = (Handshake)this.flowControl.SelectedIndex;
                s0.StopBits = (StopBits)this.stopBits.SelectedIndex + 1;
                s0.ReadTimeout = 2000;
                s0.WriteTimeout = 2000;
                s0.PortName = this.port.Text;
                this.conect.IsEnabled = false;
                this.disconect.IsEnabled = true;
                this.send.IsEnabled = true;
                this.execute.IsEnabled = true;
                this.buffer.IsEnabled = true;
                
                s0.DataReceived += S0_DataReceived;
                s0.Open();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (s0.IsOpen) s0.Close();
               
            }
                    
                
        }

        private void S0_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string str = "";

            if (DateTime.Now.Subtract(lastRead) > new TimeSpan(2000000))
            {
                if (htmlTextOutput.Count>0)
                {
                    str += $"\n\n";
                    htmlTextOutput.Add($"<br><br>");
                }
                str += $"[{DateTime.Now}] - Leitura: ";
            }
            lastRead = DateTime.Now;

            while (s0.BytesToRead > 0)
            {
                str += $"{s0.ReadByte().ToString("x2").ToUpper()} ";
            }
            
            

            this.screenText.Dispatcher.Invoke((Action)(() =>
            {

                Run r = new Run(str);

                htmlTextOutput.Add("<font color=red>" + r.Text + "</font>");


                r.Foreground = Brushes.DarkRed;

                this.screenText.Inlines.Add(r);
                
            }));
          
        }

        private void disconect_Click(object sender, RoutedEventArgs e)
        {
          
            try
            {
                if(buffer_timer.Enabled) buffer_timer.Stop();
                s0.DiscardOutBuffer();
                s0.Close();
                s0 = null;
                this.conect.IsEnabled = true;
                this.disconect.IsEnabled = false;
                this.send.IsEnabled = false;
                this.execute.IsEnabled = false;
                this.buffer.IsEnabled = false;
                this.finish.IsEnabled = false;
                this.test_time.IsEnabled = true;

            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void scriptAddress_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buffer_Click(object sender, RoutedEventArgs e)
        {
            
            buffer_timer.Start();
            this.buffer.IsEnabled = false;
            this.finish.IsEnabled = true;
            this.test_time.IsEnabled = false;
            buffer_timestamp = DateTime.Now;

        }

        private void Buffer_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            buffer_timer.Stop();
            try 
            {
                    int n = s0.BaudRate/8;
                    Random r = new Random();
                    byte[] buff = new byte[n];
                    string str = "";

                    r.NextBytes(buff);
                    s0.Write(buff, 0, n);

                    for (int j = 0; j < n; j++)
                        str += buff[j].ToString("x").ToUpper() + " ";


                    this.screenText.Dispatcher.Invoke((Action)(() =>
                    {
                        TimeSpan t = TimeSpan.Parse(this.test_time.Text).Subtract(DateTime.Now.Subtract(buffer_timestamp));
                        
                        buffer_timestamp = DateTime.Now;

                        lastRead = new DateTime();

                        write2Screen(str);
                        
                        if (t <= new TimeSpan(0))
                        {
                            s0.DiscardOutBuffer();
                            s0.DiscardInBuffer();
                            this.test_time.Text = "00:00:00";
                            this.buffer.IsEnabled = true;
                            this.test_time.IsEnabled = true;
                            this.finish.IsEnabled = false;
                        }
                        else
                        {
                            this.test_time.Text = t.ToString();
                            buffer_timer.Start();
                        }

                    }));
                }
                catch (Exception exception)
                {
                    if (exception.HResult != -2147023901) MessageBox.Show(exception.Message);
                }

        }

        private void clean_Click(object sender, RoutedEventArgs e)
        {
            this.screenText.Text = "";
            htmlTextOutput.Clear();
        }

        private void finish_Click(object sender, RoutedEventArgs e)
        {
            s0.DiscardOutBuffer();
            s0.DiscardInBuffer();
            buffer_timer.Stop();
            buffer_timer.Enabled = false;
            this.buffer.IsEnabled = true;
            this.finish.IsEnabled = false;
            this.test_time.IsEnabled = true;
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            if (this.ManualFrame.Text != "")
            {
                string str = this.ManualFrame.Text.ToUpper().Replace(" ", "");
                byte[] frame = new byte[str.Length];
                string str2Screen = "";
                for (int i = 0; i < str.Length; i++)
                {
                    frame[i] = str[i] > 64 ? (byte)(str[i] - 55) : (byte)(str[i] - 48);
                    str2Screen += frame[i].ToString("x");
                    if (i % 2 > 0)
                    {
                        str2Screen += " ";
                    }
                }
                s0.Write(frame, 0, frame.Length);
                write2Screen(str2Screen.ToUpper());

            }
      }

        private void write2Screen(string str2Screen)
        {
            Run r = new Run($"\n\n[{DateTime.Now}] - Escrita: {str2Screen}");
            lastRead = new DateTime();
            r.Foreground = Brushes.Blue;
            this.screenText.Inlines.Add(r);
            htmlTextOutput.Add($"<br><br><font color=blue>{r.Text}</font>");


        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Nome: {App.ResourceAssembly.GetName().Name}\n" +
                $"Versão: {App.ResourceAssembly.GetName().Version}\n" +
                $"Desenvolvido por: Jonathan Culau",
                "Sobre o software",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFile = new Microsoft.Win32.SaveFileDialog();
            saveFile.Filter = "Arquivo html|*.html";
            if (saveFile.ShowDialog() == true)
            {
                string fileName = saveFile.FileName;
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter($@"{fileName}"))
                {
                    sw.WriteLine("<html>");
                    sw.WriteLine("<head>");
                    sw.WriteLine("<title>Log do ensaio</title>");
                    sw.WriteLine("</head>");
                    sw.WriteLine("<body>");
                    sw.WriteLine("<h1>Sociedade Serial</h1>");
                    sw.WriteLine($"<b>Versão: </b>{App.ResourceAssembly.GetName().Version}<br>");
                    sw.WriteLine($"<b>Executor: </b>{Environment.UserName}<br><br>");
                    if (!this.tag.Text.Equals("")) sw.WriteLine("<b>TAG da banca de energia: </b>" + this.tag.Text + "<br>");
                    sw.WriteLine("<br>");
                    sw.WriteLine("<font-size=32px>");
                    htmlTextOutput.ForEach(delegate (string element)
                    {
                        sw.WriteLine(element);
                    });                    
                    sw.WriteLine("</font-size>");
                    sw.WriteLine("</body>");
                    sw.WriteLine("</html>");
                }
            }




        }


        private void screenText_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           while(this.screenText.Text.Length > 1E3)
            {
                this.screenText.Inlines.Remove(this.screenText.Inlines.FirstInline);
            }
        }

        private void screenText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            while (this.screenText.Text.Length > 5e3)
            {
                this.screenText.Inlines.Remove(this.screenText.Inlines.FirstInline);
            }
        }
    }
    
}
