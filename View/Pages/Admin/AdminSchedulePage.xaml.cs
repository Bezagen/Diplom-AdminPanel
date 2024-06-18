using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Data;
using AdminPanel.Logic;
using AdminPanel.Model.Structures;
using AdminPanel.View.EditingWindows;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminSchedulePage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<CertainRoute> scheduleCollection = new ObservableCollection<CertainRoute>();
        private ObservableCollection<CertainRoute> visibleRoutesCollection = new ObservableCollection<CertainRoute>();
        private ObservableCollection<Route> routesCollection = new ObservableCollection<Route>();
        private ObservableCollection<int> trainsCollection = new ObservableCollection<int>();

        public AdminSchedulePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
                DepartureDatePicker.DisplayDateStart = DateTime.Now;
                DepartureTimePicker.TimeInterval = TimeSpan.FromMinutes(15);
                ArrivalDatePicker.DisplayDateStart = DateTime.Now;
                ArrivalTimePicker.TimeInterval = TimeSpan.FromMinutes(15);

                await GetSchedule();

                visibleRoutesCollection = scheduleCollection;

                RoutesListBox.ItemsSource = visibleRoutesCollection;

                DataTable tempTableRoutes = await dataBaseAdapter.GetTableAsync("SELECT Route.ID, Depart.Name as DeparturePoint, Destin.Name as DestinationPoint FROM Route JOIN City as Depart ON Depart.ID = Route.ID_City_Of_Departure JOIN City AS Destin ON Destin.ID = Route.ID_Destination_City");
                DataRow[] rows = null;
                rows = tempTableRoutes.Select();

                for (int i = 0; i < rows.Length; i++)
                {
                    Route route = new Route();
                    route.ID = Convert.ToInt32(rows[i]["ID"]);
                    route.DeparturePoint = rows[i]["DeparturePoint"].ToString();
                    route.DestinationPoint = rows[i]["DestinationPoint"].ToString();
                    routesCollection.Add(route);
                }

                RouteComboBox.ItemsSource = routesCollection;

                await GetTrains();

                TrainComboBox.ItemsSource = trainsCollection;
            }
            catch
            {
                MessageBox.Show($"Ошибка загрузки", "Ошибка");
            }
        }

        private async Task GetSchedule()
        {
            try
            {
                scheduleCollection.Clear();

                DataTable tempTableSchedule = await dataBaseAdapter.GetTableAsync("SELECT DepartPoint.Name AS Depart, DestinPoint.Name AS Destin, Schedule.DateAndTimeOfArrival, Schedule.DateAndTimeOfDeparture, Schedule.ID_Train FROM Schedule JOIN Route ON Route.ID = Schedule.ID_Route JOIN City AS DepartPoint ON DepartPoint.ID = Route.ID_City_Of_Departure JOIN City AS DestinPoint ON DestinPoint.ID = Route.ID_Destination_City");
                DataRow[] rows = tempTableSchedule.Select();

                for (int i = 0; i < rows.Length; i++)
                {
                    CertainRoute certainRoute = new CertainRoute();
                    certainRoute.DeparturePoint = rows[i]["Depart"].ToString();
                    certainRoute.DestinationPoint = rows[i]["Destin"].ToString();
                    certainRoute.DateTimeDeparture = rows[i]["DateAndTimeOfDeparture"].ToString();
                    certainRoute.DateTimeArrival = rows[i]["DateAndTimeOfArrival"].ToString();
                    certainRoute.TrainID = Convert.ToInt32(rows[i]["ID_Train"]);

                    scheduleCollection.Add(certainRoute);
                }
            }
            catch 
            {
                MessageBox.Show($"Не удалось получить расписание", "Ошибка");
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


            }
            catch 
            {
                MessageBox.Show("Не удалость получить поезда", "Ошибка");
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string pattern = SearchTextBox.Text;

            ObservableCollection<CertainRoute> tempCollection = new ObservableCollection<CertainRoute>();

            if (SearchTextBox.Text != "")
            {
                for (int i = 0; i < scheduleCollection.Count; i++)
                {
                    if (Regex.IsMatch(scheduleCollection[i].DeparturePoint, pattern) || Regex.IsMatch(scheduleCollection[i].DestinationPoint, pattern) || Regex.IsMatch(scheduleCollection[i].DateTimeDeparture, pattern) || Regex.IsMatch(scheduleCollection[i].DateTimeArrival, pattern))
                    {
                        tempCollection.Add(scheduleCollection[i]);
                    }
                }
            }
            RoutesListBox.ItemsSource = null;
            RoutesListBox.ItemsSource = tempCollection;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            visibleRoutesCollection = scheduleCollection;
            RoutesListBox.ItemsSource = visibleRoutesCollection;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
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

                if (ArrivalTimePicker.Text == "")
                {
                    MessageBox.Show("Выберете время прибытия", "Внимание");
                    return;
                }

                int newID = Convert.ToInt32( await dataBaseAdapter.GetCellAsync("SELECT MAX(ID) AS MaxID FROM Schedule")) + 1;

                string DateTimeOfDeparture = $"{DepartureDatePicker.Text} {DepartureTimePicker.Text}";
                string DateTimeOfArrival = $"{ArrivalDatePicker.Text} {ArrivalTimePicker.Text}";

                _ = await dataBaseAdapter.ExecuteQueryAsync("INSERT INTO [Schedule] (ID, ID_Route, ID_Train, DateAndTimeOfDeparture, DateAndTimeOfArrival) VALUES" +
                    $@"({newID}, {routesCollection[RouteComboBox.SelectedIndex].ID}, {trainsCollection[TrainComboBox.SelectedIndex]}, '{DateTimeOfDeparture}', '{DateTimeOfArrival}')");

                RoutesListBox.ItemsSource = null;
                
                await GetSchedule();

                visibleRoutesCollection = scheduleCollection;
                RoutesListBox.ItemsSource = visibleRoutesCollection;

                TrainComboBox.ItemsSource = null;

                await GetTrains();

                TrainComboBox.ItemsSource = trainsCollection;
            }
            catch 
            {
                MessageBox.Show($"Не удалось добавить запись", "Ошибка");
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoutesListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись которую хотите отредактировать", "Внимание");
                return;
            }
            CertainRoute selectdRoute = RoutesListBox.SelectedItem as CertainRoute;
            EditSchedule editSchedule = new EditSchedule(selectdRoute);
            editSchedule.ShowDialog();

            try
            {
                RoutesListBox.ItemsSource = null;

                await GetSchedule();

                visibleRoutesCollection = scheduleCollection;
                RoutesListBox.ItemsSource = visibleRoutesCollection;

                TrainComboBox.ItemsSource = null;

                await GetTrains();

                TrainComboBox.ItemsSource = trainsCollection;
            }
            catch
            {
                MessageBox.Show("Не получилось загрузить данные", "Ошибка");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoutesListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись которую хотите удалить", "Внимание");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить эту запись?", "Внимание", MessageBoxButton.YesNo);

            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    CertainRoute selectdRoute = RoutesListBox.SelectedItem as CertainRoute;

                    int scheduleID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [Schedule] WHERE ID_Train = {selectdRoute.TrainID}"));

                    if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM [Ticket] WHERE ID_Schedule = {scheduleID}") != null)
                    {
                        MessageBox.Show("Данное расписание используется", "Внимание");
                        return;
                    }

                    _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Schedule] WHERE ID_Train = {selectdRoute.TrainID}");

                    RoutesListBox.ItemsSource = null;

                    await GetSchedule();

                    visibleRoutesCollection = scheduleCollection;
                    RoutesListBox.ItemsSource = visibleRoutesCollection;

                    TrainComboBox.ItemsSource = null;

                    await GetTrains();

                    TrainComboBox.ItemsSource = trainsCollection;
                }
            }
            catch
            {
                MessageBox.Show($"Не удалость удалить запись", "Внимание");
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
    }
}
