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
    public partial class AdminCarriageTypePage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        DataTable carriageTypesTable;
        public AdminCarriageTypePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetCarriageTypes();
        }

        private async Task GetCarriageTypes()
        {
            carriageTypesTable = await dataBaseAdapter.GetTableAsync("SELECT [Name] as Название, Price as Цена, Seats_Count as [Количество мест] FROM Type_Carriage");
            CarriageTypeDataGrid.ItemsSource = carriageTypesTable.DefaultView;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
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

            if (SeatsCountTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести количество мест в вагоне", "Внимание");
                return;
            }

            if (Convert.ToInt32(SeatsCountTextBox.Text) > 70)
            {
                MessageBox.Show("В вагоне не может быть больше 70 мест", "Внимание");
                return;
            }

            foreach (DataRow row in carriageTypesTable.Rows)
            {
                if (row["Название"].ToString().Equals($"{CarriageTypeNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такой тип вагона уже есть", "Внимание");
                    return;
                }
            }

            int maxID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync("SELECT MAX(ID) AS MaxID FROM [Type_Carriage]")) + 1;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Type_Carriage] (ID, Name, Price, Seats_Count) VALUES ({maxID}, '{CarriageTypeNameTextBox.Text}', {CarriageTypePriceTextBox.Text}, {SeatsCountTextBox.Text})");

            await GetCarriageTypes();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CarriageTypeDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно удалить", "Внимние");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить данную строку?", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            var dataRow = CarriageTypeDataGrid.SelectedItem as DataRowView;

            string typeName = "";
            if (dataRow != null)
                typeName = dataRow["Название"].ToString();
            else
                MessageBox.Show("Не удалось получить тип вагона для удаления", "Внимание");

            int typeID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [Type_Carriage] WHERE Name = '{typeName}'"));

            if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Carriage WHERE ID_Type_Carriage = {typeID}") != null)
            {
                MessageBox.Show("Данный тип вагона уже используется", "Внимание");
                return;
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Type_Carriage] WHERE Name = '{typeName}'");

            await GetCarriageTypes();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (CarriageTypeDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую хотите изменить", "Внимание");
                return;
            }

            var dataRow = CarriageTypeDataGrid.SelectedItem as DataRowView;

            if (dataRow != null)
            {
                EditCarriageType editType = new EditCarriageType(carriageTypesTable, dataRow);
                editType.ShowDialog();
            }

            await GetCarriageTypes();
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
