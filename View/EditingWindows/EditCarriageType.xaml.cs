using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using System.Text.RegularExpressions;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditCarriageType : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable carriageTypesTable;
        private DataRowView selectedRow;

        public EditCarriageType(DataTable dataTable, DataRowView dataRow)
        {
            InitializeComponent();

            carriageTypesTable = dataTable;
            selectedRow = dataRow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarriageTypeNameTextBox.Text = selectedRow["Название"].ToString();

            string price = selectedRow["Цена"].ToString();

            if (price.Contains(","))
                CarriageTypePriceTextBox.Text = price.Replace(",", ".");
            else
                CarriageTypePriceTextBox.Text = price;

            SeatsCountTextBox.Text = selectedRow["Количество мест"].ToString();
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
            if (CarriageTypeNameTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести название", "Внимание");
                return;
            }

            if (CarriageTypePriceTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести цену", "Внимание");
                return;
            }

            if (!Regex.IsMatch($"{CarriageTypePriceTextBox.Text}", @"(\d+.\d+)|(\d+)"))
            {
                MessageBox.Show("Введенная цена не является допустимой", "Внимание");
                return;
            }

            if (Convert.ToInt32(SeatsCountTextBox.Text) > 70)
            {
                MessageBox.Show("В вагоне не может быть больше 70 мест", "Внимание");
                return;
            }

            if (CarriageTypeNameTextBox.Text != selectedRow["Название"].ToString())
                foreach (DataRow row in carriageTypesTable.Rows)
                {
                    if (row["Название"].ToString().Equals($"{CarriageTypeNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Такой тип вагона уже есть", "Внимание");
                        return;
                    }
                }

            _ = await dataBaseAdapter.GetTableAsync($"UPDATE [Type_Carriage] SET Name = '{CarriageTypeNameTextBox.Text}', Price = {CarriageTypePriceTextBox.Text}, Seats_Count = {SeatsCountTextBox.Text} WHERE Name = '{selectedRow["Название"].ToString()}'");

            this.Close();
        }

        private void CarriageTypePriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && (!CarriageTypePriceTextBox.Text.Contains(".") && CarriageTypePriceTextBox.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }

        private void SeatsCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
    }
}
