using AdminPanel.Properties;
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

namespace AdminPanel.View
{

    public partial class OptionWindow : Window
    {
        public OptionWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectionTextBox.Text = Settings.Default.ConnectionString;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectionTextBox.Text != "")
            {
                Settings.Default.ConnectionString = ConnectionTextBox.Text;
                Settings.Default.Save();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ConnectionString = Settings.Default.DefaultConnectionString;
            Settings.Default.Save();
        }
    }
}
