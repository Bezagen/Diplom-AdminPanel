using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using System.Collections.ObjectModel;
using AdminPanel.View.EditingWindows;
using System.Text.RegularExpressions;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminRoutesPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<string> citiesCollection = new ObservableCollection<string>();
        public AdminRoutesPage()
        {
            InitializeComponent();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeparturePointComboBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать город отправления", "Внимание");
                return;
            }

            if (DestinationPointComboBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать город назначения", "Внимание");
                return;
            }

            if (DeparturePointComboBox.SelectedValue.ToString() == DestinationPointComboBox.SelectedValue.ToString())
            {
                MessageBox.Show("Нельзя выбрать одинаковый город\nв отправлении и прибытии", "Внимание");
                return;
            }

            if (RoutePriceTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести цену", "Внимание");
                return;
            }

            if (!Regex.IsMatch($"{RoutePriceTextBox.Text}", @"(\d+.\d+)|(\d+)"))
            {
                MessageBox.Show("Введенная цена не является допустимой", "Внимание");
                return;
            }

            int maxID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync("SELECT MAX(ID) AS MaxID FROM Route"));

            int selectedDepartureCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{DeparturePointComboBox.SelectedValue.ToString()}'"));
            int selectedDestinationCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{DestinationPointComboBox.SelectedValue.ToString()}'"));

            if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Route WHERE ID_City_Of_Departure = {selectedDepartureCityID} AND ID_Destination_City = {selectedDestinationCityID}") != null)
            {
                MessageBox.Show("Такой маршрут уже существует", "Внимание");
                return;
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO Route (ID, ID_City_Of_Departure, ID_Destination_City, Price) VALUES({maxID + 1}, {selectedDepartureCityID}, {selectedDestinationCityID}, {RoutePriceTextBox.Text})");

            await GetRoutes();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoutesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую надо удалить", "Внимание");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить эту строку?", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            var dataRow = RoutesDataGrid.SelectedItem as DataRowView;

            string departureCity = "",
                   destinationCity = "";
            if (dataRow != null)
            {
                departureCity = dataRow["ГородОтправления"].ToString();
                destinationCity = dataRow["ГородНазначения"].ToString();
            }
            else
            {
                MessageBox.Show("Не получилось получить города для удаления", "Ошибка");
                return;
            }

            int selectedDepartureCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{departureCity}'"));
            int selectedDestinationCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{destinationCity}'"));

            int routeID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM Route WHERE ID_City_Of_Departure = {selectedDepartureCityID} AND ID_Destination_City = {selectedDestinationCityID}"));

            if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Schedule WHERE ID_Route = {routeID}") != null)
            {
                MessageBox.Show("Данный маршрут уже используется", "Внимание");
                return;
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM Route WHERE ID_City_Of_Departure = {selectedDepartureCityID} AND ID_Destination_City = {selectedDestinationCityID}");

            await GetRoutes();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoutesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую хотите изменить", "Внимание");
                return;
            }

            var dataRow = RoutesDataGrid.SelectedItem as DataRowView;

            if (dataRow != null)
            {
                EditRoute editRoute = new EditRoute(citiesCollection, dataRow);
                editRoute.ShowDialog();
            }

            await GetRoutes();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetRoutes();

            await GetCities();

            DeparturePointComboBox.ItemsSource = citiesCollection;
            DestinationPointComboBox.ItemsSource = citiesCollection;
        }

        private async Task GetCities()
        {
            DataTable citiesTable = await dataBaseAdapter.GetTableAsync("SELECT Name FROM [City]");
            DataRow[] rows = citiesTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                citiesCollection.Add(rows[i]["Name"].ToString());
            }
        }

        private async Task GetRoutes()
        {
            DataTable routesTable = await dataBaseAdapter.GetTableAsync("SELECT Depart.Name as ГородОтправления, Destin.Name as ГородНазначения, Route.Price as Цена FROM Route JOIN City as Depart ON Depart.ID = Route.ID_City_Of_Departure JOIN City AS Destin ON Destin.ID = Route.ID_Destination_City");
            RoutesDataGrid.ItemsSource = routesTable.DefaultView;
        }

        private void RoutePriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && (!RoutePriceTextBox.Text.Contains(".") && RoutePriceTextBox.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }
    }
}
