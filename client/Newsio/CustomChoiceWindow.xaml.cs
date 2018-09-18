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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for CustomChoiceWindow.xaml
    /// </summary>
    public partial class CustomChoiceWindow : Window
    {
        public CustomChoiceWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //once an option has been chosen, return to main window and send chosen selection
            (Owner as MainWindow).customMenuResult = listView.SelectedIndex;
            (Owner as MainWindow).SetSingleplayerCustom();
            Close();
        }
    }
}
