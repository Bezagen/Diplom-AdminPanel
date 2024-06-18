using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditRoute : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<string> citiesCollection = new ObservableCollection<string>();
        private DataRowView selectedRow;

        public EditRoute(ObservableCollection<string> cities, DataRowView dataRow)
        {
            InitializeComponent();

            citiesCollection = cities;
            selectedRow = dataRow;
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DeparturePointComboBox.ItemsSource = citiesCollection;
            DestinationPointComboBox.ItemsSource = citiesCollection;

            int departureCityIndex = DeparturePointComboBox.Items.IndexOf(selectedRow["ГородОтправления"].ToString());
            int destinationCityIndex = DestinationPointComboBox.Items.IndexOf(selectedRow["ГородНазначения"].ToString());

            DeparturePointComboBox.SelectedIndex = departureCityIndex;
            DestinationPointComboBox.SelectedIndex = destinationCityIndex;

            string price = selectedRow["Цена"].ToString();

            if (price.Contains(","))
                RoutePriceTextBox.Text = price.Replace(",", ".");
            else
                RoutePriceTextBox.Text = price;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
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

            int selectedDepartureCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{DeparturePointComboBox.SelectedValue.ToString()}'"));
            int selectedDestinationCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{DestinationPointComboBox.SelectedValue.ToString()}'"));
            
            if (selectedRow["ГородОтправления"].ToString() != DeparturePointComboBox.SelectedValue.ToString())
            if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Route WHERE ID_City_Of_Departure = {selectedDepartureCityID} AND ID_Destination_City = {selectedDestinationCityID}") != null)
            {
                MessageBox.Show("Такой маршрут уже существует", "Внимание");
                return;
            }

            int currentDepartureCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{selectedRow["ГородОтправления"].ToString()}'"));
            int currentDestinationCityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{selectedRow["ГородНазначения"].ToString()}'"));

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Route SET Price = {RoutePriceTextBox.Text}, ID_City_Of_Departure = '{selectedDepartureCityID}', ID_Destination_City = '{selectedDestinationCityID}' WHERE ID_City_Of_Departure = {currentDepartureCityID} AND ID_Destination_City = {currentDestinationCityID}");

            this.Close();
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
