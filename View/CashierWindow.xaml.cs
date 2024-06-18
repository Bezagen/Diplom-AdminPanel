using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using AdminPanel.View.Pages;
using AdminPanel.View.Pages.Admin;

namespace AdminPanel.View
{
    public partial class CashierWindow : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        public List<Page> pages;

        public CashierWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            pages = new List<Page>();
            
            pages.Add(new CashierMainPage());
            pages.Add(new SchedulePage());
            FrameField.Content = pages[0];
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimazeButton_Click(object sender, RoutedEventArgs e)
        { WindowState = WindowState.Minimized; }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        { Application.Current.Shutdown(); }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (MenuGrid.Visibility == Visibility.Visible)
                MenuGrid.Visibility = Visibility.Collapsed;
            else
                MenuGrid.Visibility = Visibility.Visible;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuGrid.Visibility = Visibility.Collapsed;
        }

        private void CheckConnectiobButton_Click(object sender, RoutedEventArgs e)
        {
            dataBaseAdapter.Open();
            if (dataBaseAdapter.State == System.Data.ConnectionState.Closed)
                MessageBox.Show($"Сервер не доступен", "Результат");
            else if (dataBaseAdapter.State == System.Data.ConnectionState.Open)
                MessageBox.Show($"Сервер доступен", "Результат");
            dataBaseAdapter.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            dataBaseAdapter.Close();
            Application.Current.Shutdown();
        }

        private void ReturnToMainPage_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = pages[0];
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void ShowSeatsButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new AdminSeatPage();
        }
    }
}
