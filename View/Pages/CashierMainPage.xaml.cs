using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AdminPanel.Model.Structures;
using AdminPanel.Logic;
using System.Text.RegularExpressions;
using System.Data;

namespace AdminPanel.View.Pages
{
    public partial class CashierMainPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        ObservableCollection<string> citiesCollection = new ObservableCollection<string>();

        public CashierMainPage()
        {
            InitializeComponent();

            DepartureDatePicker.DisplayDateStart = DateTime.Now;

            InfoTextBlock.Text = $"Вы вошли как:\n{User.Surname} {User.Name}\nВремя входа: {User.DateTimeEntry}";
        }

        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((DeparturePointTextBox.Text == "") && (DestinationPointTextBox.Text == ""))
                {
                    MessageBox.Show("Заполните как минимум поле \"Откуда\" ");
                    return;
                }

                string condition = "WHERE";

                if ((DeparturePointTextBox.Text != "") && (DestinationPointTextBox.Text != ""))
                {
                    condition += $" DepartPoint.Name = '{DeparturePointTextBox.Text}' AND DestinPoint.Name = '{DestinationPointTextBox.Text}'";
                }

                if (DeparturePointTextBox.Text != "" && DestinationPointTextBox.Text == "")
                {
                    condition += $" DepartPoint.Name = '{DeparturePointTextBox.Text}'";
                }

                if (DestinationPointTextBox.Text != "" && DeparturePointTextBox.Text == "")
                    condition += $" DestinPoint.Name = '{DestinationPointTextBox.Text}'";

                if (DepartureDatePicker.Text != "")
                    condition += $"AND CAST(Schedule.DateAndTimeOfDeparture as DATE) = CAST('{DepartureDatePicker.Text}' AS DATE)";

                DataTable scheduleTable = await dataBaseAdapter.GetTableAsync($"SELECT DepartPoint.Name as [DepartureCity], DestinPoint.Name as [DestinationCity], Schedule.DateAndTimeOfArrival, Schedule.DateAndTimeOfDeparture, Schedule.ID_Train FROM Schedule " +
                    $"JOIN Route ON Route.ID = Schedule.ID_Route " +
                    $"JOIN City AS DepartPoint ON DepartPoint.ID = Route.ID_City_Of_Departure " +
                    $"JOIN City AS DestinPoint ON DestinPoint.ID = Route.ID_Destination_City " + condition);


                if (scheduleTable.Rows.Count < 1)
                {
                    MessageBox.Show("Ничего не найдено", "Внимание");
                    return;
                }

                DataRow[] scheduleRows = scheduleTable.Select();
                List<CertainRoute> routes = new List<CertainRoute>();

                for (int i = 0; i < scheduleRows.Length; i++)
                {
                    CertainRoute route = new CertainRoute();
                    route.DeparturePoint = scheduleRows[i]["DepartureCity"].ToString();
                    route.DestinationPoint = scheduleRows[i]["DestinationCity"].ToString();
                    route.DateTimeArrival = scheduleRows[i]["DateAndTimeOfDeparture"].ToString();
                    route.DateTimeDeparture = scheduleRows[i]["DateAndTimeOfArrival"].ToString();
                    route.TrainID = Convert.ToInt32(scheduleRows[i]["ID_Train"]);

                    routes.Add(route);
                }

                for (int i = 0; i < routes.Count; i++)
                {
                    routes[i].AvailableSeats = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT COUNT(*) AS FreeSeats FROM [Seat] JOIN Carriage ON Seat.ID_Carriage = Carriage.ID JOIN Train ON Carriage.ID_Train = Train.ID WHERE Train.ID = {routes[i].TrainID} AND [Seat].[Status] = '0'"));
                }

                this.NavigationService.Navigate(new SchedulePage(routes));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}", "Ошибка");
            }
        }

        private void DeparturePointTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowCities(DeparturePointTextBox, departurePopup, DepartureListBox);
        }

        private void ShowCities(TextBox textBox, Popup popup, ListBox listBox)
        {
            if (textBox.Text != "")
                popup.IsOpen = true;
            else
            {
                popup.IsOpen = false;
                return;
            }

            try
            {
                ObservableCollection<string> tempCollection = new ObservableCollection<string>();

                Regex regex = new Regex(textBox.Text + @"\w*");
                foreach (var item in citiesCollection)
                {
                    if (Regex.IsMatch(item, regex.ToString(), RegexOptions.IgnoreCase))
                    {
                        tempCollection.Add(item);
                    }
                }

                listBox.ItemsSource = tempCollection;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}", "Ошибка");
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            citiesCollection = await dataBaseAdapter.GetCollectionAsync("SELECT Name FROM City", 0);
        }

        private void DeparturePointTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(departurePopup.IsOpen == true)
                departurePopup.IsOpen = false;
        }

        private void DepartureListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartureListBox.SelectedItem != null)
                DeparturePointTextBox.Text = DepartureListBox.SelectedItem.ToString();
            if (departurePopup.IsOpen == true)
                departurePopup.IsOpen = false;
        }

        private void DestinationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DestinationListBox.SelectedItem != null)
                DestinationPointTextBox.Text = DestinationListBox.SelectedItem.ToString();
            if (destinationPopup.IsOpen == true)
                destinationPopup.IsOpen = false;
        }

        private void DestinationPointTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowCities(DestinationPointTextBox, destinationPopup, DestinationListBox);
        }

        private void DestinationPointTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (destinationPopup.IsOpen == true)
                destinationPopup.IsOpen = false;
        }

        private void CheckRoutesButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RoutesPage());
        }

        private void DepartureDatePicker_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!DateTime.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }
    }
}
