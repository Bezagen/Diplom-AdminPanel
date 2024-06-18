using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using System.Text.RegularExpressions;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditTicketRate : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable ticketRateTable;
        private DataRowView selectedRow;
        public EditTicketRate(DataTable dataTable, DataRowView dataRow)
        {
            InitializeComponent();
            ticketRateTable = dataTable;
            selectedRow = dataRow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TicketRateNameTextBox.Text = selectedRow["Название"].ToString();

            string price = selectedRow["Цена"].ToString();

            if (price.Contains(","))
                TicketRatePriceTextBox.Text = price.Replace(",", ".");
            else
                TicketRatePriceTextBox.Text = price;
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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

            if (TicketRateNameTextBox.Text != selectedRow["Название"].ToString())
                foreach (DataRow row in ticketRateTable.Rows)
                {
                    if (row["Название"].ToString().Equals($"{TicketRateNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Такой тип вагона уже есть", "Внимание");
                        return;
                    }
                }

            _ = await dataBaseAdapter.GetTableAsync($"UPDATE [Ticket_Rate] SET Name = '{TicketRateNameTextBox.Text}', Price = {TicketRatePriceTextBox.Text} WHERE Name = '{selectedRow["Название"].ToString()}'");

            this.Close();
        }

        private void TicketRatePriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && (!TicketRatePriceTextBox.Text.Contains(".") && TicketRatePriceTextBox.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }
    }
}
