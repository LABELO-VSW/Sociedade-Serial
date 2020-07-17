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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Sociedade_Serial
{
    
    /// <summary>
    /// Lógica interna para commandWindow.xaml
    /// </summary>
    public partial class commandWindow : Window
    {
        KeyEventArgs pkey = null;
        public bool addCommand = true;
        public bool isFlag = false;
        public commandWindow()
        {
            InitializeComponent();
            addCommand = false;
            if (this.algorithm.Text == "")
            {
                this.algorithm.Items.Clear();
                this.algorithm.Items.Add("Nenhum");
                this.algorithm.Items.Add("CRC");
                this.algorithm.Items.Add("Checksum");
            }
            this.algorithm.SelectedIndex = 0;
            this.send.Text = "";
            this.name.Text = "";
            this.receive.Text = "";

        }
        public commandWindow(commandWindow c)
        {
            InitializeComponent();
            addCommand = false;
            if (this.algorithm.Text == "")
            {
                this.algorithm.Items.Clear();
                this.algorithm.Items.Add("Nenhum");
                this.algorithm.Items.Add("CRC");
                this.algorithm.Items.Add("Checksum");
            }
            this.send.Text = "";
            this.name.Text = "";
            this.receive.Text = "";
            
            this.algorithm.SelectedIndex = c.algorithm.SelectedIndex;
            this.from.Text = c.from.Text;
            this.polynominal.Text = c.polynominal.Text;
            this.initialValue.Text = c.initialValue.Text;
            this.finalXorVal.Text = c.finalXorVal.Text;
            this.outputReflected.IsChecked = c.outputReflected.IsChecked;
            this.inputReflected.IsChecked = c.inputReflected.IsChecked;
            this.lowBitFirst.IsChecked = c.lowBitFirst.IsChecked;
            this.send_external_script.Text = c.send_external_script.Text;
            this.receive_external_script.Text = c.receive_external_script.Text;

            this.w8.IsChecked = c.w8.IsChecked;
            this.w16.IsChecked = c.w16.IsChecked;
            this.w32.IsChecked = c.w32.IsChecked; 
            this.w64.IsChecked = c.w64.IsChecked;
        }

        public commandWindow(utils.command c)
        {
            
            InitializeComponent();
            addCommand = false;
            if (this.algorithm.Text == "")
            {
                this.algorithm.Items.Clear();
                this.algorithm.Items.Add("Nenhum");
                this.algorithm.Items.Add("CRC");
                this.algorithm.Items.Add("Checksum");
            }
            this.send.Text = c.send;
            this.name.Text = c.name;
            this.receive.Text = c.receive;

            this.algorithm.Text = c.checksum.algorithm;
            this.from.Text = c.checksum.from.ToString();
            this.polynominal.Text = c.checksum.polynominal.ToString("x");
            this.initialValue.Text = c.checksum.initialValue.ToString("x");
            this.finalXorVal.Text = c.checksum.finalXorVal.ToString("x");
            this.outputReflected.IsChecked = c.checksum.resultReflected;
            this.inputReflected.IsChecked = c.checksum.inputReflected;
            this.lowBitFirst.IsChecked = c.checksum.lowBitFirst;
            this.send_external_script.Text = c.send_script;
            this.receive_external_script.Text = c.receive_script;


            switch (c.checksum.width)
            {
                case 8:
                    this.w8.IsChecked = true;
                    break;
                case 16:
                    this.w16.IsChecked = true;
                    break;
                case 32:
                    this.w32.IsChecked = true;
                    break;
                case 64:
                    this.w64.IsChecked = true;
                    break;

            }
            
        }


        private void algorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.algorithm.SelectedItem.ToString() == "Checksum")
            {
                this.initialValue.IsEnabled = false;
                this.finalXorVal.IsEnabled = false;
                this.outputReflected.IsEnabled = false;
                this.inputReflected.IsEnabled = false;
                this.polynominal.IsEnabled = false;
                this.from.IsEnabled = true;
                this.w16.IsEnabled = true;
                this.w8.IsEnabled = true;
                this.w32.IsEnabled = true;
                this.w64.IsEnabled = true;
                this.lowBitFirst.IsEnabled = true;
            }
            else if (this.algorithm.SelectedItem.ToString().Equals("CRC"))
            {
                this.initialValue.IsEnabled = true;
                this.finalXorVal.IsEnabled = true;
                this.outputReflected.IsEnabled = true;
                this.inputReflected.IsEnabled = true;
                this.polynominal.IsEnabled = true;
                this.from.IsEnabled = true;
                this.w16.IsEnabled = true;
                this.w8.IsEnabled = true;
                this.w32.IsEnabled = true;
                this.w64.IsEnabled = true;
                this.lowBitFirst.IsEnabled = true;
            }
            else
            {
                this.initialValue.IsEnabled = false;
                this.finalXorVal.IsEnabled = false;
                this.outputReflected.IsEnabled = false;
                this.inputReflected.IsEnabled = false;
                this.polynominal.IsEnabled = false;
                this.from.IsEnabled = false;
                this.w16.IsEnabled = false;
                this.w8.IsEnabled = false;
                this.w32.IsEnabled = false;
                this.w64.IsEnabled = false;
                this.lowBitFirst.IsEnabled = false;
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            addCommand = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            addCommand = true;
            this.Close();
        }

        
        private void hex_KeyUp(object sender, KeyEventArgs e)
        {

            TextBox tb = (TextBox)sender;
            string Error_string;
            Label lb;
            int p = tb.SelectionStart;
            tb.Text = tb.Text.Trim();
            tb.SelectionStart = p > tb.Text.Length ? tb.Text.Length : p;

                if (tb.Name == this.send.Name)
                {
                    lb = this.sen_lb;
                    Error_string = lb.Content.ToString();
                    lb.Content = "Frame de envio: ";
                }
                else
                {
                    lb = this.rec_lb;
                    Error_string = lb.Content.ToString();
                    lb.Content = "Resposta esperada: ";
                }
                int b = tb.Text.Length>0?1:0;
                bool isByte = false;
                p = 1;
                bool flag = false;
                try
                {
                    for (int i = 0; i < tb.Text.Length; i++)
                    {
                    if (tb.Text[i] == '#')
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                            b--;
                        }
                    }
                        if (!flag)
                        {
                            if (tb.Text[i] == ' ')
                            {
                                if(isByte) b++;
                                isByte = false;
                            }
                            else
                            {
                                isByte = true;
                            }
                        }
                        else
                        {
                            if (tb.Text[i] == ',')
                            {
                                int j = 1;
                                string nbytes = "";
                                while (tb.Text[i + j] != '#' && i + j < tb.Text.Length)
                                {
                                    if (tb.Text[i + j] != ' ') nbytes += tb.Text[i + j];
                                    j++;
                                }
                                b += int.Parse(nbytes);

                            }
                        }
                        if (i == tb.SelectionStart - 1)
                        {
                            p = b;
                            if (tb.SelectionStart < tb.Text.Length)
                            {
                                if (tb.Text[tb.SelectionStart] == ' ') p++;
                            }
                        }
                    }
                    lb.Content += $"({(p>b?b:p)}/{b})";
                }
                catch
                {
                    lb.Content = Error_string;
                }
            
            
        }

        private void hex_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int p = tb.SelectionStart;

            if (tb.Text.Length > 0 && (e.Key == Key.V) && ModifierKeys.Control.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                return;
            }

            isFlag = false;
            for (int i = 0; i < p; i++)
            {
                if (tb.Text[i] == '#') isFlag = !isFlag;
            }     
            if (pkey==null)
            {
                pkey = e;
            }
            if (!isFlag)
            {
                e.Handled = e.Key == Key.Space;
                if (e.Key == Key.Delete && p<tb.Text.Length && p>1)
                {
                    if (tb.Text[p] == ' ') tb.SelectionStart++;
                }else if (e.Key == Key.Back && p>0)
                {
                    if (tb.Text[p - 1] == ' ') tb.SelectionStart--;
                }
            }


           
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                pkey = e;
            
        }

        private void hex_KeyDown(object sender, KeyEventArgs e)
        {
            
            TextBox tb = (TextBox)sender;
            
            int p = tb.SelectionStart;

            ((TextBox)sender).IsEnabled = false;
           
            if (!isFlag)
            {
                try
                {
                    if (pkey.IsDown && (pkey.Key == Key.LeftShift || pkey.Key == Key.RightShift))
                    {
                        if (!e.Key.Equals(Key.D3) && !e.Key.Equals(Key.Tab))
                        {
                            e.Handled = true;
                        }
                        else if (p>1)
                        {
                            if (tb.Text[p-1]!=' ')
                            {
                                tb.Text = tb.Text.Insert(p, " ");
                                tb.SelectionStart = p + 1;
                            }
                                
                        }

                    }
                    else
                    {
                        var regex = new Regex(@"[^a-fA-F0-9\s]");
                        e.Handled = regex.IsMatch(e.Key.ToString());
                        e.Handled &= (e.Key < Key.NumPad0 || e.Key > Key.NumPad9);
                        e.Handled &= e.Key != Key.X && e.Key!=Key.Tab;
                       
                        if (!e.Handled)
                        {
                            if (p < tb.Text.Length)
                            {
                                if (p > 1)
                                {
                                    if (tb.Text[p - 1] != ' ')
                                    {
                                        if (tb.Text[p - 2] != ' ')
                                        {
                                            tb.Text = tb.Text.Insert(p, " ");
                                            tb.SelectionStart = p + 1;
                                        }
                                        else if (tb.Text[p] != ' ')
                                        {
                                            tb.Text = tb.Text.Insert(p, " ");
                                            tb.SelectionStart = p;
                                        }
                                    } else
                                    {
                                        if (tb.Text.Length - p >= 2)
                                        {
                                            if (tb.Text[p] != ' ' && tb.Text[p + 1] != ' ')
                                            {
                                                tb.Text = tb.Text.Insert(p, " ");
                                                tb.SelectionStart = p--;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (tb.Text.Length >= 2)
                                    {
                                        if (p == 0)
                                        {
                                            if (tb.Text[p] != ' ' && tb.Text[p+1]!=' ')
                                            {
                                                tb.Text = tb.Text.Insert(p, " ");
                                                tb.SelectionStart = p--;
                                            }
                                        }
                                        else
                                        {
                                            if (tb.Text[p] != ' ')
                                            {
                                                tb.Text = tb.Text.Insert(p, " ");
                                                tb.SelectionStart = p--;
                                            }
                                        }
                                    }
                                }
                                
                            }
                            else if (p>1)
                            {
                                if (tb.Text[p - 1] != ' ' && tb.Text[p - 2] != ' ')
                                {
                                    tb.Text = tb.Text.Insert(p, " ");
                                    tb.SelectionStart = p + 2;
                                }                                
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    var regex = new Regex(@"[^a-fA-F0-9\s]");
                    if (regex.IsMatch(e.Key.ToString()) && (e.Key < Key.NumPad0 || e.Key > Key.NumPad9) && e.Key != Key.X)
                    {
                        e.Handled = true;
                    }
                }


            }
            ((TextBox)sender).IsEnabled = true;

        }
    }
}
