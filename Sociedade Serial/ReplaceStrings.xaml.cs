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

namespace Sociedade_Serial
{
    /// <summary>
    /// Lógica interna para ReplaceStrings.xaml
    /// </summary>
    public partial class ReplaceStrings : Window
    {
        public List<string> textValues = new List<string>();
        public bool flag = false;
        public ReplaceStrings()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            foreach(Object obj in this.stack.Children)
            {
                if(obj is TextBox)
                    textValues.Add(((TextBox)obj).Text);
            }
            flag = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
