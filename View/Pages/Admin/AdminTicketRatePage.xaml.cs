using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using AdminPanel.View.EditingWindows;
using System.Text.RegularExpressions;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminTicketRatePage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        DataTable ticketRateTable;

        public AdminTicketRatePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetTicketRates();
        }

        private async Task GetTicketRates()
        {
            ticketRateTable = await dataBaseAdapter.GetTableAsync("SELECT [Name] as [Название], [Price] as [Цена] FROM Ticket_Rate");
            TicketRateDataGrid.ItemsSource = ticketRateTable.DefaultView;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketRateNameTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести название", "Внимание");
                return;
            }

            if (TicketRatePriceTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести цену", "Внимание");
                return;
            }

            if (!Regex.IsMatch($"{TicketRatePriceTextBox.Text}", @"(\d+.\d+)|(\d+)"))
            {
                MessageBox.Show("Введенная цена не является допустимой", "Внимание");
                return;
            }

            foreach (DataRow row in ticketRateTable.Rows)
            {
                if (row["Название"].ToString().Equals($"{TicketRateNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такой тип билета уже есть", "Внимание");
                    return;
                }
            }

            int maxID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync("SELECT MAX(ID) AS MaxID FROM [Ticket_Rate]")) + 1;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Ticket_Rate] (ID, Name, Price) VALUES ({maxID}, '{TicketRateNameTextBox.Text}', {TicketRateNameTextBox.Text})");

            await GetTicketRates();
        }

        private void TicketRatePriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && (!TicketRatePriceTextBox.Text.Contains(".") && TicketRatePriceTextBox.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketRateDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно удалить", "Внимние");
                return;
            }

            var dataRow = TicketRateDataGrid.SelectedItem as DataRowView;

            string typeName = "";
            if (dataRow != null)
                typeName = dataRow["Название"].ToString();
            else
                MessageBox.Show("Не удалось получить тип билета для удаления", "Внимание");

            int ticketRateID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT FROM [Ticket_Rate] WHERE Name '{typeName}'"));

            if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Ticket WHERE ID_Ticket_Rate = {ticketRateID}") != null)
            {
                MessageBox.Show("Есть билеты с данным тарифом", "Внимание");
                return;
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Ticket_Rate] WHERE Name = '{typeName}'");

            await GetTicketRates();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketRateDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую хотите изменить", "Внимание");
                return;
            }

            var dataRow = TicketRateDataGrid.SelectedItem as DataRowView;

            if (dataRow != null)
            {
                EditTicketRate editTicketRate = new EditTicketRate(ticketRateTable, dataRow);
                editTicketRate.ShowDialog();
            }

            await GetTicketRates();
        }
    }
}
