using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AdminPanel.Model.Structures;


namespace AdminPanel.View.Pages
{
    public partial class SchedulePage : Page
    {
        List<CertainRoute> certainRoutes = new List<CertainRoute>();

        public SchedulePage(List<CertainRoute> getRoutes)
        {
            InitializeComponent();

            certainRoutes = getRoutes;
            
            RoutesListBox.ItemsSource = certainRoutes;
        }

        public SchedulePage()
        {
            InitializeComponent();
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            object tag = (sender as FrameworkElement).Tag;
            int index = RoutesListBox.Items.IndexOf(tag);

            SelectedRouteWindow routeWindow = new SelectedRouteWindow(certainRoutes[index]);
            routeWindow.ShowDialog();

            NavigationService.GoBack();
        }
    }
}
