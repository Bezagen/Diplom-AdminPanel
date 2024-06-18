using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using System.Text.RegularExpressions;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditService : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable servicesTable;
        private DataRowView selectedRow;

        public EditService(DataTable dataTable, DataRowView dataRow)
        {
            InitializeComponent();

            servicesTable = dataTable;

            selectedRow = dataRow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ServiceNameTextBox.Text = selectedRow["Название"].ToString();

            string price = selectedRow["Цена"].ToString();

            if (price.Contains(","))
                ServicePriceTextBox.Text = price.Replace(",", ".");
            else
                ServicePriceTextBox.Text = price;
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
            if (ServiceNameTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести название", "Внимание");
                return;
            }

            if (ServicePriceTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести цену", "Внимание");
                return;
            }

            if (!Regex.IsMatch($"{ServicePriceTextBox.Text}", @"(\d+.\d+)|(\d+)"))
            {
                MessageBox.Show("Введенная цена не является допустимой", "Внимание");
                return;
            }

            if (ServiceNameTextBox.Text != selectedRow["Название"].ToString())
                foreach (DataRow row in servicesTable.Rows)
                {
                    if (row["Название"].ToString().Equals($"{ServiceNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Такой тип вагона уже есть", "Внимание");
                        return;
                    }
                }

            _ = await dataBaseAdapter.GetTableAsync($"UPDATE [Additional_Services] SET [Name] = '{ServiceNameTextBox.Text}', [Price] = {ServicePriceTextBox.Text} WHERE [Name] = '{selectedRow["Название"].ToString()}'");
            
            this.Close();
        }

        private void ServicePriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && (!ServicePriceTextBox.Text.Contains(".") && ServicePriceTextBox.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }
    }
}
