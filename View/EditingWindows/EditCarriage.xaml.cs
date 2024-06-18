using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Collections.ObjectModel;
using System.Data;
using AdminPanel.Model.Structures;


namespace AdminPanel.View.EditingWindows
{
    public partial class EditCarriage : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable carriagesTable;
        private ObservableCollection<CarriageType> carriageTypesCollection = new ObservableCollection<CarriageType>();
        private ObservableCollection<int> trainsCollection = new ObservableCollection<int>();
        private DataRowView selectedRow;

        public EditCarriage(DataTable dataTable, ObservableCollection<CarriageType> carriageTypes, ObservableCollection<int> trains, DataRowView dataRow)
        {
            InitializeComponent();

            carriagesTable = dataTable;
            carriageTypesCollection = carriageTypes;
            trainsCollection = trains;
            selectedRow = dataRow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarriageNumberTextBox.Text = selectedRow["Номер вагона"].ToString();

            CarriageTypeComboBox.ItemsSource = carriageTypesCollection;
            TrainNumberComboBox.ItemsSource = trainsCollection;

            SelectCarriageTypeInComboBox();

            int trainIDIndex = TrainNumberComboBox.Items.IndexOf(selectedRow["Номер поезда"]);
            TrainNumberComboBox.SelectedIndex = trainIDIndex;
        }

        private void SelectCarriageTypeInComboBox()
        {
            for (int i = 0; i < CarriageTypeComboBox.Items.Count; i++)
            {
                CarriageType currentCategory = CarriageTypeComboBox.Items[i] as CarriageType;
                if (selectedRow["Тип вагона"].ToString() == currentCategory.Name)
                    CarriageTypeComboBox.SelectedIndex = i;
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
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

            if(CarriageNumberTextBox.Text != selectedRow["Номер вагона"].ToString())
                foreach (DataRow row in carriagesTable.Rows)
                {
                    if (row["Номер вагона"].ToString().Equals($"{CarriageNumberTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Такой Вагон уже есть", "Внимание");
                        return;
                    }
                }

            CarriageType selectedType = CarriageTypeComboBox.SelectedItem as CarriageType;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE [Carriage] SET ID = {CarriageNumberTextBox.Text}, ID_Train = {TrainNumberComboBox.SelectedValue}, ID_Type_Carriage = {selectedType.ID} WHERE ID = {selectedRow["Номер вагона"]}");

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Seat] WHERE ID_Carriage = {selectedRow["Номер вагона"]}");

            for (int i = 1; i <= selectedType.Seats_Count; i++)
            {
                _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Seat] (ID_Carriage, Seat_Number, [Status]) VALUES ({CarriageNumberTextBox.Text}, {i}, '{0}')");
            }

            this.Close();
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

        private void CarriageNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
    }
}
