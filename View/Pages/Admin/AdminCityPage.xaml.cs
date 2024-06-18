using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Data;
using AdminPanel.Logic;
using AdminPanel.View.EditingWindows;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminCityPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable cityTable;
        public AdminCityPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                cityTable = await dataBaseAdapter.GetTableAsync("SELECT Name as Название FROM [City]");

                CitiesDataGrid.ItemsSource = cityTable.DefaultView;
            }
            catch 
            {
                MessageBox.Show("Не удалось загрузить данные", "Ошибка");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CitiesDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберете строку которую надо удалить", "Внимание");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить данную строку?", "Внимание");

                if (result == MessageBoxResult.No)
                    return;

                var dataRow = CitiesDataGrid.SelectedItem as DataRowView;
                string selectedCity = dataRow["Название"].ToString();

                int cityID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [City] WHERE Name = '{selectedCity}'"));

                if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Route WHERE ID_City_Of_Departure = '{cityID}' OR ID_Destination_City = '{cityID}'") != null)
                {
                    MessageBox.Show("Данный город уже используется", "Внимание");
                    return;
                }

                _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [City] WHERE Name = '{selectedCity}'");

                await UpdateTable();
            }
            catch
            {
                MessageBox.Show("Не удалось удалить строку", "Ошибка");
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (CityNameTextBox.Text == "")
            {
                MessageBox.Show("Заполните название города", "Внимание");
                return;
            }

            foreach (DataRow row in cityTable.Rows)
            {
                if (row["Название"].ToString().Equals($"{CityNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такой город уже есть", "Внимание");
                    return;
                }
            }

            try
            {
                _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO City (Name) VALUES('{CityNameTextBox.Text}')");

                await UpdateTable();
            }
            catch
            {
                MessageBox.Show("Не удалось внести в запись в базу данных", "Ошибка");
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (CitiesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберете строку которую хотите изменить", "Внимание");
                return;
            }

            var dataRow = CitiesDataGrid.SelectedItem as DataRowView;
            
            string columnValue = "";
            
            if (dataRow != null)
                columnValue = dataRow["Название"].ToString();

            EditCity editCity = new EditCity(columnValue, cityTable);
            editCity.ShowDialog();

            try
            {
                await UpdateTable();
            }
            catch
            {
                MessageBox.Show("Не получилось обновить данные", "Внимание");
            }
        }

        private async Task UpdateTable()
        {
            cityTable = await dataBaseAdapter.GetTableAsync("SELECT Name as Название FROM [City]");
            CitiesDataGrid.ItemsSource = null;
            CitiesDataGrid.ItemsSource = cityTable.DefaultView;
        } 
    }
}
