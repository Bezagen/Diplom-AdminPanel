using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Collections.ObjectModel;
using System.Data;
using AdminPanel.Model.Structures;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditTrain : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<TrainsCategories> trainCategoriesCollection = new ObservableCollection<TrainsCategories>();
        private DataRowView selectedRow;
        private DataTable trainsTable;

        public EditTrain(ObservableCollection<TrainsCategories> trainsCategories, DataRowView dataRow, DataTable table)
        {
            InitializeComponent();

            trainCategoriesCollection = trainsCategories;
            selectedRow = dataRow;
            trainsTable = table;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TrainCategoryComboBox.ItemsSource = trainCategoriesCollection;

            SelectCategoryInComboBox();
            
            TrainNumberTextBox.Text = selectedRow["Номер Поезда"].ToString();
        }
        private void SelectCategoryInComboBox()
        {
            for (int i = 0; i < TrainCategoryComboBox.Items.Count; i++)
            {
                TrainsCategories currentCategory = TrainCategoryComboBox.Items[i] as TrainsCategories;
                if (selectedRow["Категория"].ToString() == currentCategory.Name)
                    TrainCategoryComboBox.SelectedIndex = i;
            }
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
            if (TrainCategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать категорию поезда", "Внимание");
                return;
            }

            if (TrainNumberTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести номер поезда", "Внимание");
                return;
            }

            TrainsCategories selectedCategory = TrainCategoryComboBox.SelectedItem as TrainsCategories;

            if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Carriage WHERE ID_Train = {selectedCategory.ID}") != null)
            {
                MessageBox.Show("У данного поезда есть вагоны", "Внимние");
                return;
            }

            foreach (DataRow row in trainsTable.Rows)
            {
                if (TrainNumberTextBox.Text != selectedRow["Номер поезда"].ToString())
                    if (row["Номер Поезда"].ToString().Equals($"{TrainNumberTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Такой поезд уже есть", "Внимание");
                        return;
                    }
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE [Train] Set ID = {TrainNumberTextBox.Text}, ID_Train_Category = {selectedCategory.ID} WHERE ID = {selectedRow["Номер поезда"]}");

            this.Close();
        }

        private void TrainNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
    }
}
