using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdminPanel.Logic;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminSeatPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable seatsTable;
        public AdminSeatPage()
        {
            InitializeComponent();
        }

        private async void ChangeSingleStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (SeatsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно изменить", "Внимание");
                return;
            }

            var dataRow = SeatsDataGrid.SelectedItem as DataRowView;

            int carriageNumber = 0,
                seatNumber = 0;

            if (dataRow != null)
            {
                carriageNumber = Convert.ToInt32(dataRow["Номер вагона"]);
                seatNumber = Convert.ToInt32(dataRow["Номер места"]);
            }

            string status = Convert.ToString(await dataBaseAdapter.GetCellAsync($"SELECT Status FROM Seat WHERE ID_Carriage = {carriageNumber} AND Seat_Number = {seatNumber}"));

            if (status == "1")
                _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Seat SET Status = '0' WHERE ID_Carriage = {carriageNumber} AND Seat_Number = {seatNumber}");
            else
                _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Seat SET Status = '1' WHERE ID_Carriage = {carriageNumber} AND Seat_Number = {seatNumber}");

            await GetSeats();
        }

        private async void ChangeCarriageSatusToFreeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SeatsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите вагон который нужно изменить", "Внимание");
                return;
            }

            var dataRow = SeatsDataGrid.SelectedItem as DataRowView;

            int carriageNumber = 0;

            if (dataRow != null)
            {
                carriageNumber = Convert.ToInt32(dataRow["Номер вагона"]);
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Seat SET Status = '0' WHERE ID_Carriage = {carriageNumber}");

            await GetSeats();
        }

        private async void ChangeCarriageSatusToOccupiedButton_Click(object sender, RoutedEventArgs e)
        {
            if (SeatsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите вагон который нужно изменить", "Внимание");
                return;
            }

            var dataRow = SeatsDataGrid.SelectedItem as DataRowView;

            int carriageNumber = 0;

            if (dataRow != null)
            {
                carriageNumber = Convert.ToInt32(dataRow["Номер вагона"]);
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Seat SET Status = '1' WHERE ID_Carriage = {carriageNumber}");

            await GetSeats();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetSeats();
        }

        private async Task GetSeats()
        {
            seatsTable = await dataBaseAdapter.GetTableAsync("SELECT ID_Carriage as [Номер вагона], Seat_Number as [Номер места], [Status] as [Статус] FROM Seat");
            SeatsDataGrid.ItemsSource = seatsTable.DefaultView;
        }
    }
}
