using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using AdminPanel.Model.Structures;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditSchedule : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<Route> routesCollection = new ObservableCollection<Route>();
        private ObservableCollection<int> trainsCollection = new ObservableCollection<int>();
        private CertainRoute route = new CertainRoute();

        public EditSchedule(CertainRoute certainRoute)
        {
            InitializeComponent();

            route = certainRoute;
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (RouteComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберете маршрут", "Внимание");
                return;
            }

            if (TrainComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберете поезд", "Внимание");
                return;
            }

            if (DepartureDatePicker.Text == "")
            {
                MessageBox.Show("Выберете дату отправления", "Внимание");
                return;
            }

            if (DepartureTimePicker.Text == "")
            {
                MessageBox.Show("Выберете время отправления", "Внимание");
                return;
            }

            if (ArrivalDatePicker.Text == "")
            {
                MessageBox.Show("Выберете дату прибытия", "Внимание");
                return;
            }

            if (DepartureTimePicker.Text == "")
            {
                MessageBox.Show("Выберете время прибытия", "Внимание");
                return;
            }

            try
            {
                Route selectedRoute = RouteComboBox.SelectedItem as Route;
                CertainRoute selectedTrainID = TrainComboBox.SelectedItem as CertainRoute;

                string DateTimeOfDeparture = $"{DepartureDatePicker.Text} {DepartureTimePicker.Text}";
                string DateTimeOfArrival = $"{ArrivalDatePicker.Text} {ArrivalTimePicker.Text}";

                _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Schedule SET ID_Route = {selectedRoute.ID}, ID_Train = {TrainComboBox.SelectedValue}, DateAndTimeOfDeparture = '{DateTimeOfDeparture}', DateAndTimeOfArrival = '{DateTimeOfArrival}' WHERE ID_Train = {route.TrainID}");

                this.Close();
            }
            catch
            {
                MessageBox.Show("Не удалоась сохранить изменения", "Ошибка");
            }
        }

        private void DepartureDatePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!DateTime.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }

        private void ArrivalDateTimePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!DateTime.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }

        private void DepartureTimePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Match match = Regex.Match(e.Text, @"^\d{2}:\d{2}$");

            if (!match.Success)
            {
                e.Handled = true;
            }
        }

        private void ArrivalTimePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Match match = Regex.Match(e.Text, @"^\d{2}:\d{2}$");

            if (!match.Success)
            {
                e.Handled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnterDateAndTime();

            DataTable tempTableRoutes = await dataBaseAdapter.GetTableAsync("SELECT Route.ID, Depart.Name as DeparturePoint, Destin.Name as DestinationPoint FROM Route JOIN City as Depart ON Depart.ID = Route.ID_City_Of_Departure JOIN City AS Destin ON Destin.ID = Route.ID_Destination_City");
            DataRow[] rows = tempTableRoutes.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                Route route = new Route();
                route.ID = Convert.ToInt32(rows[i]["ID"]);
                route.DeparturePoint = rows[i]["DeparturePoint"].ToString();
                route.DestinationPoint = rows[i]["DestinationPoint"].ToString();
                routesCollection.Add(route);
            }

            RouteComboBox.ItemsSource = routesCollection;

            SelectRouteInComboBox();

            await GetTrains();

            TrainComboBox.ItemsSource = trainsCollection;

            int trainIndex = TrainComboBox.Items.IndexOf(route.TrainID);

            if (trainIndex != -1)
                TrainComboBox.SelectedIndex = trainIndex;
        }

        private void EnterDateAndTime()
        {
            DepartureDatePicker.DisplayDateStart = DateTime.Now;
            DepartureTimePicker.TimeInterval = TimeSpan.FromMinutes(15);
            ArrivalDatePicker.DisplayDateStart = DateTime.Now;
            ArrivalTimePicker.TimeInterval = TimeSpan.FromMinutes(15);

            string[] departureParts = route.DateTimeDeparture.Split(' ');
            DepartureDatePicker.Text = departureParts[0];
            DepartureTimePicker.Text = departureParts[1];

            string[] arrivalParts = route.DateTimeArrival.Split(' ');
            ArrivalDatePicker.Text = arrivalParts[0];
            ArrivalTimePicker.Text = arrivalParts[1];
        }

        private void SelectRouteInComboBox()
        {
            for (int i = 0; i < RouteComboBox.Items.Count; i++)
            {
                Route currentRoute = RouteComboBox.Items[i] as Route;
                if (currentRoute.DeparturePoint == route.DeparturePoint && currentRoute.DestinationPoint == route.DestinationPoint)
                {
                    RouteComboBox.SelectedIndex = i;
                }
            }
        }

        private async Task GetTrains()
        {
            try
            {
                trainsCollection.Clear();

                DataTable tempTableTrains = await dataBaseAdapter.GetTableAsync("SELECT Train.ID FROM Train WHERE NOT EXISTS( SELECT 1 FROM Schedule WHERE Schedule.ID_Train = Train.ID ); ");
                DataRow[] rows = null;
                rows = tempTableTrains.Select();

                for (int i = 0; i < rows.Length; i++)
                {
                    trainsCollection.Add(Convert.ToInt32(rows[i]["ID"]));
                }

                trainsCollection.Add(route.TrainID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалость получить поезда", "Ошибка");
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
