using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Threading;



namespace Sociedade_Serial
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    /// 
    enum state
    {
        disconnected,
        connected,
        sendingBuffer,
        runningScript
    }
        
    public partial class MainWindow : Window
    {
        SerialPort s0 = new SerialPort();

        string receive = "";
               
        DateTime lastRead = DateTime.Now;

        System.Timers.Timer buffer_timer;

        bool scriptRunning = false;

        DateTime buffer_timestamp;

        utils.script script = new utils.script();

        List<string> tags = new List<string>();

        List<string> replacer = new List<string>();

        List<string> htmlTextOutput = new List<string>();

        bool total_evaluation = true;
        commandWindow commandWindow;
        public MainWindow()
        {
            utils.constants c = new utils.constants();

            this.WindowState = WindowState.Maximized;
            InitializeComponent();

            this.screenText.Document.Blocks.Clear();

            this.baudrate.ItemsSource = c.baudRates;
            this.dataBits.ItemsSource = c.dataBits;
            this.stopBits.ItemsSource = c.stopBits;
            this.parity.ItemsSource = c.parity;
            this.flowControl.ItemsSource = c.flowControl;
            this.port.GotMouseCapture += Port_GotMouseCapture;

            this.baudrate.SelectedIndex = 5;
            this.dataBits.SelectedIndex = 3;
            this.stopBits.SelectedIndex = 0;
            this.parity.SelectedIndex = 0;
            this.flowControl.SelectedIndex = 0;
            this.timeout.Text = 2000.ToString();
            updateScreenButtons(state.disconnected);

            buffer_timer = new System.Timers.Timer() { Interval = 1};
            buffer_timer.Elapsed += Buffer_timer_Elapsed;
            script.commands = new List<utils.command>();
            

        }

        // Configure buttons
        private void disconect_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (buffer_timer.Enabled) buffer_timer.Stop();
                s0.DiscardOutBuffer();
                s0.Close();
                s0 = null;
                updateScreenButtons(state.disconnected);

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                updateScreenButtons(state.disconnected);
            }

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
                s0.DataReceived += S0_DataReceived;
                s0.RtsEnable = this.dtr.IsChecked == true;
                s0.DtrEnable = this.rts.IsChecked == true;
                s0.Open();
                updateScreenButtons(state.connected);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (s0.IsOpen) s0.Close();
                updateScreenButtons(state.disconnected);

            }


        }

        // Port Event
        private void S0_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (scriptRunning)
            {
                string str = "";
                while (s0.BytesToRead > 0)
                {
                    str += $"{s0.ReadByte().ToString("x2").ToUpper()} ";

                }
                receive += str;
            }
            else
            {
                string str = "";
                if (DateTime.Now.Subtract(lastRead) > new TimeSpan(0,0,1))
                {
                    str += $"[{DateTime.Now}] - Leitura: ";
                }
                lastRead = DateTime.Now;
                string pstr = "";
                while (s0.BytesToRead > 0)
                {
                    pstr += $"{s0.ReadByte().ToString("x2").ToUpper()} ";

                }
                str += pstr;

                this.screenText.Dispatcher.Invoke((Action)(() =>
                {
                        if (!str.Contains("["))
                        {
                            screenText.AppendText(" " + str);
                            if (!this.buffer.IsEnabled) htmlTextOutput.Add($"<font color=\"#FF0000\">{str}</font>");
                        }
                        else
                        {
                            Paragraph p = new Paragraph();
                            p.Inlines.Add(str);
                            p.Foreground = Brushes.DarkRed;
                            this.screenText.Document.Blocks.Add(p);
                            if (!this.buffer.IsEnabled) htmlTextOutput.Add($"<br><br><font color=\"#FF0000\">{str}</font>");
                        }

                   


                }));
            }
            
          
        }

        //Communication Buttons
        private void scriptAddress_Click(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog newScript = new Microsoft.Win32.OpenFileDialog();
            newScript.Filter = "Script|*.json";
            
            if (Directory.Exists(utils.constants.script_default_address))
                newScript.InitialDirectory = utils.constants.script_default_address;
            
            if (newScript.ShowDialog() == true)
            {
                try
                {
                    script = new utils.script();
                    script = utils.loadScript(newScript.FileName);
                    this.commands.Items.Clear();
     
                    
                    tags.Clear();
                    replacer.Clear();

                    foreach (utils.command c in script.commands)
                    {
                        this.commands.Items.Add(c.name);
                        
                    }                   

                                        
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }
            

        }

        private void buffer_Click(object sender, RoutedEventArgs e)
        {
            
            buffer_timer.Start();
            buffer_timestamp = DateTime.Now;
            updateScreenButtons(state.sendingBuffer);

        }

        private void Buffer_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            buffer_timer.Stop();
            try 
            {
                    int n = s0.BaudRate/10;
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
                            updateScreenButtons(state.connected);
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
            this.screenText.Document.Blocks.Clear();

            htmlTextOutput.Clear();

            lastRead = DateTime.Now.Subtract(new TimeSpan(1,0,0));
        }

        private void finish_Click(object sender, RoutedEventArgs e)
        {
            scriptRunning = false;
            try
            {
                s0.DiscardOutBuffer();
                s0.DiscardInBuffer();
                buffer_timer.Stop();
            }
            catch
            {
                buffer_timer.Stop();
            }
            buffer_timer.Enabled = false;
            updateScreenButtons(state.connected);
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            if (this.commands.SelectedItems.Count == 0) return;

            List<utils.command> c_list = new List<utils.command>();
            List<string> ActiveTags = new List<string>();

            for (int i =0; i < this.commands.SelectedItems.Count; i++)
            {
                // Import command information 
                utils.command c = script.commands[this.commands.Items.IndexOf(this.commands.SelectedItems[i])];

                for(int id =0;id<c.send.Length;id++)
                {
                    if (c.send[id].Equals('#'))
                    {
                        string flag = c.send.Substring(id+1).ToUpper();
                        int new_id = flag.IndexOf('#');
                        flag = flag.Substring(0, new_id);
                        id += new_id+1;

                        if (flag!= "CS")
                        {
                            if (!tags.Contains(flag))
                            {
                                ActiveTags.Add(flag);
                                tags.Add(flag);
                            }else
                            {
                                ActiveTags.Add(flag);
                            }
                        }

                    }
                    else if (c.send[id].Equals(' ')) c.send.Remove(id, 1);
                    
                }
                for (int id = 0; id < c.receive.Length; id++)
                {
                    if (c.receive[id].Equals('#'))
                    {
                        string flag = c.receive.Substring(id + 1).ToUpper();
                        int new_id = flag.IndexOf('#');
                        flag = flag.Substring(0, new_id);
                        id += new_id+1;
                        if (flag != "CS")
                        {
                            if (!tags.Contains(flag))
                            {
                                ActiveTags.Add(flag);
                                tags.Add(flag);
                            }
                            else
                            {
                                ActiveTags.Add(flag);
                            }
                        }
                    }
                    else if (c.receive[id].Equals(' ')) c.receive.Remove(id, 1);
                }
                c_list.Add(c);
            }

            if (ActiveTags.Count > 0)
            {
                int i = 0;
                this.IsEnabled = false;
                ReplaceStrings replace = new ReplaceStrings();

                foreach (string str in tags)
                {
                    if (ActiveTags.Contains(str)) 
                    { 
                         string TAG = "";
                        int maxSize = 256;
                        if (str.IndexOf(",") != -1)
                        {
                            int len = str.IndexOf(",");
                            Tag = str.Substring(0, len).Replace("#", "");
                            maxSize = int.Parse(str.Substring(len + 1).Replace("#", "").Trim()) * 2;
                        }
                        else
                        {
                            Tag = str.Replace("#", "");
                        }
                        replace.stack.Children.Add(new Label { Content = Tag, Margin = new Thickness(10, 0, 10, 0), FontWeight = FontWeights.Normal });
                        if (i < replacer.Count)
                            replace.stack.Children.Add(new TextBox
                            {
                                Margin = new Thickness(10, 0, 10, 10),
                                FontWeight = FontWeights.Normal,
                                TextAlignment = TextAlignment.Left,
                                Text = replacer[i],
                                MaxLength = maxSize
                            });
                        else
                            replace.stack.Children.Add(new TextBox
                            {
                                Margin = new Thickness(10, 0, 10, 10),
                                FontWeight = FontWeights.Normal,
                                TextAlignment = TextAlignment.Left,
                                MaxLength = maxSize
                            });

                        i++;
                    }
                }

                replace.ShowDialog();
                this.IsEnabled = true;
                if (replace.flag)
                    replacer = replace.textValues;
                else
                {
                    return;
                }
                
                replace.Close();
            }
            
            scriptRunning = true;
            Thread.Sleep(100);
            clean_Click(sender, e);
            int timeOut = int.Parse(this.timeout.Text);
            total_evaluation = true;
            Thread t = new Thread(new ThreadStart(() => this.RunScript(c_list, timeOut)));
            updateScreenButtons(state.runningScript);
            t.Start();
        }
        private void RunScript(List<utils.command> command_list,int timeOut)
        {
            List<utils.test_results> _test = new List<utils.test_results>();
            
            utils.test_results commandOnTest = new utils.test_results();
            for (int i = 0; i < command_list.Count; i++)
            {                
                if (!scriptRunning) return;

                receive = "";

                utils.command c = command_list[i];
                htmlTextOutput.Add($"<p><b>[{i}] - {c.name}</b></p>");

                for (int j = 0; j < replacer.Count; j++)
                {
                    c.send = c.send.ToUpper().Replace($"#{tags[j]}#", utils.getFrameFormat(replacer[j], false));
                    c.receive = c.receive.ToUpper().Replace($"#{tags[j]}#", utils.getFrameFormat(replacer[j], false));  
                }
                commandOnTest.sent.raw = c.send;
                commandOnTest.expectedAnswer = c.receive;
                if (c.send_script == null) c.send_script = "";
                
                if (c.send_script.ToLower().IndexOf("js") < 0)
                {
                    if (commandOnTest.sent.raw.IndexOf("#CS#") > 0)
                    {
                        commandOnTest.sent.raw = commandOnTest.sent.raw
                            .Replace("#CS#",
                            c.checksum.calcCRC(commandOnTest.sent.raw.Substring
                            (c.checksum.from, commandOnTest.sent.raw.IndexOf("#CS#") - c.checksum.from)));
                    }
                    commandOnTest.sent.processed = commandOnTest.sent.raw;
                    byte[] b = utils.str2Hex(commandOnTest.sent.processed);
                    receive = "";
                    s0.Write(b, 0, b.Length);
                    this.screenText.Dispatcher.Invoke((Action)(() => write2Screen(commandOnTest.sent.processed)));
                    Thread.Sleep(timeOut);
                    commandOnTest.realAnswer.raw = receive;
                    if (c.receive_script == null) c.receive_script = "";
                    if (Receive2Processed(c.receive_script, ref commandOnTest.realAnswer)) return;
                    if (!String.IsNullOrWhiteSpace(commandOnTest.realAnswer.processed))
                        this.Dispatcher.Invoke((Action)(() => read2Screen(commandOnTest.realAnswer.processed)));
                }
                else
                {
                    utils.ScriptAnswer sa = new utils.ScriptAnswer();
                    sa.newRound = true;
                    int round = 0;
                    commandOnTest.realAnswer.raw = "";
                    while (sa.newRound)
                    {
                        if (!scriptRunning) return;

                        sa = utils.callScript(c.send_script, commandOnTest.sent.raw, commandOnTest.realAnswer.raw, round);

                        if (!String.IsNullOrWhiteSpace(sa.error))
                        {
                            MessageBox.Show($"{sa.error}","Error ao executar o script",MessageBoxButton.OK,MessageBoxImage.Error);
                            scriptRunning = false;
                            this.screenText.Dispatcher.Invoke((Action)(() => updateScreenButtons(state.connected)));
                            return;
                        }
                        else
                        {
                            commandOnTest.sent.processed = sa.processed_frame;
                            if (!String.IsNullOrWhiteSpace(commandOnTest.sent.processed))
                            {
                                if (commandOnTest.sent.processed.IndexOf("#CS#") > 0)
                                {
                                    commandOnTest.sent.processed = commandOnTest.sent.processed
                                        .Replace("#CS#",
                                        c.checksum.calcCRC(commandOnTest.sent.processed.Substring
                                        (c.checksum.from, commandOnTest.sent.processed.IndexOf("#CS#") - c.checksum.from)));
                                }
                                byte[] b = utils.str2Hex(commandOnTest.sent.processed);
                                receive = "";
                                s0.Write(b, 0, b.Length);
                                this.screenText.Dispatcher.Invoke((Action)(() => write2Screen(commandOnTest.sent.processed)));
                                Thread.Sleep(timeOut);
                                commandOnTest.realAnswer.raw = receive;
                                if (c.receive_script == null) c.receive_script = "";
                                if (Receive2Processed(c.receive_script, ref commandOnTest.realAnswer)) return;
                                if (!String.IsNullOrWhiteSpace(commandOnTest.realAnswer.processed))
                                    this.Dispatcher.Invoke((Action)(() => read2Screen(commandOnTest.realAnswer.processed)));
                                round++;
                            }
                        }
                    }
                }
                try
                {
                    commandOnTest.expectedAnswer = commandOnTest.expectedAnswer.Replace(" ", "");
                    string ans = "";
                    if (commandOnTest.realAnswer.processed != null)
                    {
                        ans = commandOnTest.realAnswer.processed.Replace(" ", "");
                    }
                    for (int ch = 0; ch < commandOnTest.expectedAnswer.Length; ch++)
                    {
                        if (commandOnTest.expectedAnswer[ch] == 'X')
                        {
                            char[] array = commandOnTest.expectedAnswer.ToCharArray();
                            array[ch] = ans[ch];
                            commandOnTest.expectedAnswer = new string(array);
                        }
                    }
                    if (commandOnTest.expectedAnswer.IndexOf("#CS#") > 0)
                    {
                        commandOnTest.expectedAnswer = commandOnTest.expectedAnswer
                        .Replace("#CS#",
                                 c.checksum.calcCRC(
                                 commandOnTest.expectedAnswer.Substring(c.checksum.from,
                                 commandOnTest.expectedAnswer.IndexOf("#CS#") - c.checksum.from))).ToUpper();
                        Console.WriteLine(commandOnTest.expectedAnswer.Length);

                    }
                    commandOnTest.evaluation = commandOnTest.expectedAnswer.Replace(" ","").Equals(ans);
                }
                catch
                {
                    commandOnTest.evaluation = false;
                }
                
                if (!String.IsNullOrWhiteSpace(commandOnTest.expectedAnswer)) total_evaluation &= commandOnTest.evaluation;
                
                _test.Add(commandOnTest);

                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (total_evaluation)
                        this.partial_result.Background = Brushes.Green;
                    else
                        this.partial_result.Background = Brushes.Red;
                }));
            }

            scriptRunning = false;
            htmlTextOutput.Add("<br><br><h3>Análise automática dos resultados:</h3>");
            htmlTextOutput.Add("<table  class=\"table table-striped\">");
            htmlTextOutput.Add("<tr>");
            htmlTextOutput.Add($"<th>ID</th>");
            htmlTextOutput.Add($"<th>Resultado</th>");
            htmlTextOutput.Add("</tr>");
            int id = 0;
            foreach (utils.test_results r in _test)
            {
                string eval = "";
                if (r.expectedAnswer == "") eval = "Análise manual"; else eval = r.evaluation ? "Aprovado" : "Reprovado";
                
                htmlTextOutput.Add("<tr>");
                htmlTextOutput.Add($"<td>{id}</td>");
                htmlTextOutput.Add($"<td>{eval}</td>");
                htmlTextOutput.Add("</tr>");
                id++;
            }
            htmlTextOutput.Add("</table>");
            this.screenText.Dispatcher.Invoke((Action)(() => updateScreenButtons(state.connected)));
        }
        
        private void write2Screen(string str2Screen)
        {
            try
            {
                string str = $"[{DateTime.Now}] - Escrita: {str2Screen.ToUpper()}";

                Paragraph p = new Paragraph();
                p.Foreground = Brushes.Blue;
                
                p.Inlines.Add (str);
                
                htmlTextOutput.Add($"<p style=\"color:#0000FF\">{str}</style>");
                this.screenText.Document.Blocks.Add(p);

            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
            this.UpdateLayout();
        }
        private void read2Screen(string read2Screen)
        {
            string str = $"[{DateTime.Now.ToString()}] Leitura - {read2Screen}";
            Paragraph p = new Paragraph();
            p.Inlines.Add(str);
            p.Foreground = Brushes.DarkRed;
            this.screenText.Document.Blocks.Add(p);
            htmlTextOutput.Add($"<p style=\"color:#FF0000\">{str}</style><br><br>");

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
            saveFile.AddExtension = true;
            saveFile.DefaultExt = "html";

            if (Directory.Exists(utils.constants.register_default_address))
                saveFile.InitialDirectory = utils.constants.register_default_address;

            if (saveFile.ShowDialog() == true)
            {
                string fileName = saveFile.FileName;
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter($@"{fileName}"))
                {
                    sw.WriteLine("<html>");
                    sw.WriteLine("<head>");
                    sw.WriteLine("<title>Log do ensaio</title>");
                    sw.WriteLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css\">");
                    sw.WriteLine("<style>*{font-family:'Arial' !important}</style>");
                    sw.WriteLine("</head>");
                    sw.WriteLine("<body>");
                    sw.WriteLine("<div class=\"container\">");
                    sw.WriteLine("<h1>Sociedade Serial</h1>");
                    sw.WriteLine($"<b>Versão: </b>{App.ResourceAssembly.GetName().Version}<br>");
                    sw.WriteLine($"<b>Executor: </b>{Environment.UserName}<br>");
                    sw.WriteLine($"<b>Data de emissão do relatório: </b>{DateTime.Now.ToString("dd/MM/yyyy")}<br>");
                    if (!this.tag.Text.Equals("")) sw.WriteLine("<b>TAG da banca de energia: </b>" + this.tag.Text + "<br>");
                    sw.WriteLine("<br>");
                    htmlTextOutput.ForEach(delegate (string element)
                    {
                        sw.WriteLine(element);
                    });                    
                    sw.WriteLine("</div>");
                    sw.WriteLine("</body>");
                    sw.WriteLine("</html>");
                }
            }




        }

        private void screenText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            while (this.screenText.Document.Blocks.Count > 500)
            {
                this.screenText.Document.Blocks.Remove(this.screenText.Document.Blocks.FirstBlock);
            }
        }

        private void updateScreenButtons(state state)
        {
            switch (state)
            {
                case state.disconnected:
                    this.disconect.IsEnabled = false;
                    this.send.IsEnabled = false;
                    this.buffer.IsEnabled = false;
                    this.finish.IsEnabled = false;
                    this.baudrate.IsEnabled = true;
                    this.port.IsEnabled = true;
                    this.stopBits.IsEnabled = true;
                    this.parity.IsEnabled = true;
                    this.flowControl.IsEnabled = true;
                    this.tag.IsEnabled = true;
                    this.dataBits.IsEnabled = true;
                    this.conect.IsEnabled = true;
                    this.test_time.IsEnabled = false;
                    this.scriptAddress.IsEnabled = true;
                    this.add.IsEnabled = true;
                    this.edit.IsEnabled = true;
                    this.remove.IsEnabled = true;
                    this.Menu.IsEnabled = true;
                    this.rts.IsEnabled = true;
                    this.dtr.IsEnabled = true;
                    break;
                case state.connected:
                    this.disconect.IsEnabled = true;
                    this.conect.IsEnabled = false;
                    this.send.IsEnabled = true;
                    this.buffer.IsEnabled = true;
                    this.finish.IsEnabled = false;
                    this.baudrate.IsEnabled = false;
                    this.port.IsEnabled = false;
                    this.stopBits.IsEnabled = false;
                    this.parity.IsEnabled = false;
                    this.flowControl.IsEnabled = false;
                    this.dataBits.IsEnabled = false;
                    this.test_time.IsEnabled = true;
                    this.scriptAddress.IsEnabled = true;
                    this.add.IsEnabled = true;
                    this.edit.IsEnabled = true;
                    this.remove.IsEnabled = true;
                    this.Menu.IsEnabled = true;
                    this.rts.IsEnabled = false;
                    this.dtr.IsEnabled = false;
                    break;
                case state.sendingBuffer:
                    this.finish.IsEnabled = true;
                    this.send.IsEnabled = false;
                    this.buffer.IsEnabled = false;
                    this.test_time.IsEnabled = false;
                    this.scriptAddress.IsEnabled = false;
                    this.add.IsEnabled = false;
                    this.edit.IsEnabled = false;
                    this.remove.IsEnabled = false;
                    this.Menu.IsEnabled = false;
                    break;
                case state.runningScript:
                    this.finish.IsEnabled = true;
                    this.send.IsEnabled = false;
                    this.buffer.IsEnabled = false;
                    this.test_time.IsEnabled = false;
                    this.scriptAddress.IsEnabled = false;
                    this.add.IsEnabled = false;
                    this.edit.IsEnabled = false;
                    this.remove.IsEnabled = false;
                    this.Menu.IsEnabled = false;
                    break;

            }
        }

        private void Port_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.port.ItemsSource = SerialPort.GetPortNames();
        }

        private void GridViewColumn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MessageBox.Show("OK");
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            if (this.commands.SelectedItems.Count != this.commands.Items.Count)
                this.commands.SelectAll();
            else
                this.commands.UnselectAll();
            
        }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            
            int index = this.commands.SelectedIndex;

            if (index < 0) return;

            this.IsEnabled = false;

            commandWindow = new commandWindow(script.commands[index]);
            
            commandWindow.ShowDialog();


            if (commandWindow.addCommand)
            {
                try
                {
                    utils.command c = createCommand();
                    script.commands[index] = c;
                    this.commands.Items[index] = c.name;
                }
                catch
                {
                    MessageBox.Show(utils.ErrorMessages.unexpected_error, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    edit_Click(sender, e);
                }
                
            }
            this.IsEnabled = true;
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            
            this.IsEnabled = false;
            if (commandWindow is null) commandWindow = new commandWindow(); else commandWindow = new commandWindow(commandWindow);
            commandWindow.ShowDialog();


            if (commandWindow.addCommand)
            {

                try
                {
                    utils.command c = createCommand();
                    script.commands.Add(c);
                    this.commands.Items.Add(c.name);                    
                }
                catch
                {
                    MessageBox.Show(utils.ErrorMessages.unexpected_error, "Erro",MessageBoxButton.OK,MessageBoxImage.Error);
                    add_Click(sender, e);
                }
                
            }
            this.IsEnabled = true;

        }

        private void script_save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFile = new Microsoft.Win32.SaveFileDialog();
            saveFile.Filter = "Arquivo json|*.json";
            saveFile.AddExtension = true;
            saveFile.DefaultExt = "json";

            if (Directory.Exists(utils.constants.script_default_address))
                saveFile.InitialDirectory = utils.constants.script_default_address;

            if (saveFile.ShowDialog() == true)
            {
                try
                {
                    utils.saveScript(saveFile.FileName, script);
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }



        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            while (this.commands.SelectedIndex>=0)
            {
                script.commands.RemoveAt(this.commands.SelectedIndex);
                this.commands.Items.RemoveAt(this.commands.SelectedIndex);
            }
        }

        private utils.command createCommand()
        {
            utils.command c = new utils.command();
            c.name = commandWindow.name.Text;
            c.send = commandWindow.send.Text;
            c.receive = commandWindow.receive.Text;
            if (commandWindow.receive_external_script.Content.ToString() != "Nenhum Arquivo Selecionado")
                c.receive_script = commandWindow.receive_external_script.Content.ToString();
            else
                c.receive_script = "";
            if (commandWindow.send_external_script.Content.ToString() != "Nenhum Arquivo Selecionado")
                c.send_script = commandWindow.send_external_script.Content.ToString();
            else
                c.send_script = "";

            c.checksum = new crc();
            c.checksum.algorithm = commandWindow.algorithm.SelectedItem.ToString();
            if (c.checksum.algorithm.Equals("CRC"))
            {
                c.checksum.initialValue = UInt64.Parse(commandWindow.initialValue.Text, System.Globalization.NumberStyles.HexNumber);
                c.checksum.finalXorVal = UInt64.Parse(commandWindow.finalXorVal.Text, System.Globalization.NumberStyles.HexNumber);
                c.checksum.polynominal = UInt64.Parse(commandWindow.polynominal.Text, System.Globalization.NumberStyles.HexNumber);
                c.checksum.inputReflected = commandWindow.inputReflected.IsChecked == true;
                c.checksum.resultReflected = commandWindow.outputReflected.IsChecked == true;
                c.checksum.from = int.Parse(commandWindow.from.Text, System.Globalization.NumberStyles.HexNumber);
                c.checksum.lowBitFirst = commandWindow.lowBitFirst.IsChecked == true;
                if (commandWindow.w8.IsChecked == true) c.checksum.width = 8;
                else if (commandWindow.w16.IsChecked == true) c.checksum.width = 16;
                else if (commandWindow.w32.IsChecked == true) c.checksum.width = 32;
                else if (commandWindow.w64.IsChecked == true) c.checksum.width = 64;
            }
            else if (c.checksum.algorithm.Equals("Checksum"))
            {
                c.checksum.initialValue = 0;
                c.checksum.finalXorVal = 0;
                c.checksum.polynominal = 0;
                c.checksum.inputReflected = false;
                c.checksum.resultReflected = false;
                c.checksum.from = int.Parse(commandWindow.from.Text, System.Globalization.NumberStyles.HexNumber);
                c.checksum.lowBitFirst = commandWindow.lowBitFirst.IsChecked == true;
                if (commandWindow.w8.IsChecked == true) c.checksum.width = 8;
                else if (commandWindow.w16.IsChecked == true) c.checksum.width = 16;
                else if (commandWindow.w32.IsChecked == true) c.checksum.width = 32;
                else if (commandWindow.w64.IsChecked == true) c.checksum.width = 64;
            }
            return c;
        }
    
        private bool Receive2Processed(string scriptName, ref utils.testFrames tf)
        {
            
            if (scriptName.IndexOf("js") < 0)
            {
                tf.processed = tf.raw;

            }
            else
            {
                utils.ScriptAnswer sa = new utils.ScriptAnswer();


                try
                {
                    sa = utils.callScript(scriptName, "", tf.raw, 0);

                    if (!String.IsNullOrWhiteSpace(sa.error))
                    {
                        MessageBox.Show($"O script retornou o seguinte erro: {sa.error}");
                        scriptRunning = false;
                        this.screenText.Dispatcher.Invoke((Action)(() => updateScreenButtons(state.connected)));
                        return true;
                    }
                    else
                    {
                        tf.processed = sa.processed_frame;
                    }
                }
                catch (Exception exce)
                {
                    MessageBox.Show(exce.Message, "Error");
                    return true;
                }

            }
            return false;
        }

        private void save_log_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFile = new Microsoft.Win32.SaveFileDialog();
            TextRange tr = new TextRange(this.screenText.Document.ContentStart,
                                         this.screenText.Document.ContentEnd);
            saveFile.Filter = "Arquivo html|*.html";
            saveFile.AddExtension = true;
            saveFile.DefaultExt = "html";

            if (Directory.Exists(utils.constants.register_default_address))
                saveFile.InitialDirectory = utils.constants.register_default_address;

            if (saveFile.ShowDialog() == true)
            {
                string fileName = saveFile.FileName;
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter($@"{fileName}"))
                {
                    sw.WriteLine("<html>");
                    sw.WriteLine("<head>");
                    sw.WriteLine("<title>Log do ensaio</title>");
                    sw.WriteLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css\">");
                    sw.WriteLine("<style>*{font-family:'Arial' !important}</style>");
                    sw.WriteLine("</head>");
                    sw.WriteLine("<body>");
                    sw.WriteLine("<div class=\"container\">");
                    sw.WriteLine("<h1>Sociedade Serial</h1>");
                    sw.WriteLine($"<b>Versão: </b>{App.ResourceAssembly.GetName().Version}<br>");
                    sw.WriteLine($"<b>Executor: </b>{Environment.UserName}<br>");
                    sw.WriteLine($"<b>Data de emissão do relatório: </b>{DateTime.Now.ToString("dd/MM/yyyy")}<br>");
                    if (!this.tag.Text.Equals("")) sw.WriteLine("<b>TAG da banca de energia: </b>" + this.tag.Text + "<br>");
                    sw.WriteLine("<br>");
                    sw.WriteLine(tr.Text.Replace("[","<br><br>["));
                    sw.WriteLine("</div>");
                    sw.WriteLine("</body>");
                    sw.WriteLine("</html>");
                }
            }
        }
    }

}
