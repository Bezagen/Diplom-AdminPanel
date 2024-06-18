using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using AdminPanel.View.Pages.Admin;

namespace AdminPanel.View
{
    public partial class AdminWindow : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        public AdminWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        private void AdminWindow1_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
            dataBaseAdapter.Close();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void minimazeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (MenuGrid.Visibility == Visibility.Hidden)
                MenuGrid.Visibility = Visibility.Visible;
            else
            {
                MenuGrid.Visibility = Visibility.Hidden;
                if (TableSubMenu.Visibility == Visibility.Visible)
                    TableSubMenu.Visibility = Visibility.Hidden;
            }
        }

        private void ShowTablesButton_Click(object sender, RoutedEventArgs e)
        {
            if (TableSubMenu.Visibility == Visibility.Hidden)
                TableSubMenu.Visibility = Visibility.Visible;
            else
                TableSubMenu.Visibility = Visibility.Hidden;
        }

        private void ShowUserTypeButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminUserTypePage();
        }

        private void ShowUserButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminUserPage();
        }

        private void ShowRouteButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminRoutesPage();
        }

        private void ShowScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminSchedulePage();
        }

        private void ShowTrainButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminTrainPage();
        }

        private void ShowTrainCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminTrainTypePage();
        }

        private void ShowTicketRateButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminTicketRatePage();
        }

        private void ShowCarriageButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminCarriagePage();
        }

        private void ShowTypeCarriageButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminCarriageTypePage();
        }

        private void ShowSeatButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminSeatPage();
        }

        private void ShowPassengerButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminPassengerPage();
        }

        private void ShowCityButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminCityPage();
        }

        private void AdminWindow1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuGrid.Visibility = Visibility.Hidden;
            TableSubMenu.Visibility = Visibility.Hidden;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ShowAdditionalServices_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminAdditionalServicesPage();
        }

        private void CheckConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            dataBaseAdapter.Open();
            if (dataBaseAdapter.State == System.Data.ConnectionState.Closed)
                MessageBox.Show($"Сервер не доступен", "Результат");
            else if (dataBaseAdapter.State == System.Data.ConnectionState.Open)
                MessageBox.Show($"Сервер доступен", "Результат");
            dataBaseAdapter.Close();
        }
    }
}
