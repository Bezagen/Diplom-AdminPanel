using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using System.Collections.ObjectModel;
using AdminPanel.View.EditingWindows;
using AdminPanel.Model.Structures;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminCarriagePage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable carriagesTable;
        private ObservableCollection<CarriageType> carriageTypesCollection = new ObservableCollection<CarriageType>();
        private ObservableCollection<int> trainsCollection = new ObservableCollection<int>();

        public AdminCarriagePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetCarriages();

            await GetTrains();

            TrainNumberComboBox.ItemsSource = trainsCollection;

            await GetCarriageTypes();

            CarriageTypeComboBox.ItemsSource = carriageTypesCollection;
        }

        private async Task GetCarriages()
        {
            carriagesTable = await dataBaseAdapter.GetTableAsync("SELECT [Carriage].ID as [Номер вагона], [Carriage].ID_Train as [Номер поезда], [Type_Carriage].Name as [Тип вагона] FROM Carriage JOIN Type_Carriage ON [Carriage].ID_Type_Carriage = [Type_Carriage].ID ");
            CarriagesDataGrid.ItemsSource = carriagesTable.DefaultView;
        }

        private async Task GetTrains()
        {
            DataTable citiesTable = await dataBaseAdapter.GetTableAsync("SELECT ID FROM [Train]");
            DataRow[] dataRows = citiesTable.Select();

            for (int i = 0; i < dataRows.Length; i++)
            {
                trainsCollection.Add(Convert.ToInt32(dataRows[i]["ID"]));
            }
        }

        private async Task GetCarriageTypes()
        {
            DataTable carriageTypesTable = await dataBaseAdapter.GetTableAsync("SELECT ID, Name, Seats_Count FROM [Type_Carriage]");
            DataRow[] dataRows = carriageTypesTable.Select();

            for (int i = 0; i < dataRows.Length; i++)
            {
                CarriageType carriageType = new CarriageType();
                carriageType.ID = Convert.ToInt32(dataRows[i]["ID"]);
                carriageType.Name = dataRows[i]["Name"].ToString();
                carriageType.Seats_Count = Convert.ToInt32(dataRows[i]["Seats_Count"]);
                carriageTypesCollection.Add(carriageType);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainNumberComboBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать поезд", "Внимание");
                return;
            }

            if (CarriageTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать тип вагона", "Внимание");
                return;
            }

            if (CarriageNumberTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести номер вагона", "Внимание");
                return;
            }

            foreach (DataRow row in carriagesTable.Rows)
            {
                if (row["Номер вагона"].ToString().Equals($"{CarriageNumberTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такой Вагон уже есть", "Внимание");
                    return;
                }
            }

            CarriageType selectedType = CarriageTypeComboBox.SelectedItem as CarriageType;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Carriage] (ID, ID_Train, ID_Type_Carriage) VALUES ({CarriageNumberTextBox.Text}, {TrainNumberComboBox.SelectedValue}, {selectedType.ID})");
            
            for (int i = 1; i <= selectedType.Seats_Count; i++)
            {
                _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Seat] (ID_Carriage, Seat_Number, [Status]) VALUES ({CarriageNumberTextBox.Text}, {i}, '{0}')");
            }

            await GetCarriages();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

            if (CarriagesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно удалить", "Внимние");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить данную строку?", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            var dataRow = CarriagesDataGrid.SelectedItem as DataRowView;

            int carriageID = 0;
            if (dataRow != null)
                carriageID = Convert.ToInt32(dataRow["Номер вагона"]);
            else
                MessageBox.Show("Не удалос получить поезд для удаления", "Внимание");

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Seat] WHERE ID_Carriage = {carriageID}");

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Carriage] WHERE ID = {carriageID}");

            await GetCarriages();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (CarriagesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно изменить", "Внимние");
                return;
            }

            var dataRow = CarriagesDataGrid.SelectedItem as DataRowView;

            if (dataRow != null)
            {
                EditCarriage editCarriage = new EditCarriage(carriagesTable, carriageTypesCollection, trainsCollection, dataRow);
                editCarriage.ShowDialog();
            }

            await GetCarriages();
        }

        private void CarriageNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
    }
}
