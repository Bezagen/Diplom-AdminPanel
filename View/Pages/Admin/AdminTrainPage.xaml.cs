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
    public partial class AdminTrainPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<TrainsCategories> trainCategoriesCollection = new ObservableCollection<TrainsCategories>();
        private DataTable trainsTable;
        public AdminTrainPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetTrains();

            await GetTrainsCategories();

            TrainCategoryComboBox.ItemsSource = trainCategoriesCollection;
        }

        private async Task GetTrains()
        {
            trainsTable = await dataBaseAdapter.GetTableAsync("SELECT Train.ID as [Номер Поезда], Train_Category.Name as Категория FROM Train JOIN Train_Category ON Train.ID_Train_Category = Train_Category.ID");
            TrainsDataGrid.ItemsSource = trainsTable.DefaultView;
        }

        private async Task GetTrainsCategories()
        {
            DataTable trainsCategoriesTable = await dataBaseAdapter.GetTableAsync("SELECT ID, Name FROM [Train_Category]");
            DataRow[] rows = trainsCategoriesTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                TrainsCategories trainsCategories = new TrainsCategories();
                trainsCategories.ID = Convert.ToInt32(rows[i]["ID"]);
                trainsCategories.Name = rows[i]["Name"].ToString();
                trainCategoriesCollection.Add(trainsCategories);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
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

            foreach (DataRow row in trainsTable.Rows)
            {
                if (row["Номер Поезда"].ToString().Equals($"{TrainNumberTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такой поезд уже есть", "Внимание");
                    return;
                }
            }

            TrainsCategories selectedCategory = TrainCategoryComboBox.SelectedItem as TrainsCategories;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Train] (ID, ID_Train_Category) VALUES ({TrainNumberTextBox.Text}, {selectedCategory.ID})");

            await GetTrains();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно удалить", "Внимние");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить данную строку?", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            var dataRow = TrainsDataGrid.SelectedItem as DataRowView;

            int trainID = 0;
            if (dataRow != null)
                trainID = Convert.ToInt32(dataRow["Номер Поезда"]);
            else
                MessageBox.Show("Не удалос получить поезд для удаления", "Внимание");
             
            if(await dataBaseAdapter.GetCellAsync($"SELECT * FROM Carriage WHERE ID_Train = {trainID}") != null)
            {
                MessageBox.Show("У данного поезда есть вагоны", "Внимние");
                return;
            }
                
            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Train] WHERE ID = {trainID}");

            await GetTrains();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно изменить", "Внимние");
                return;
            }

            var dataRow = TrainsDataGrid.SelectedItem as DataRowView;

            if (dataRow != null)
            {
                EditTrain editTrain = new EditTrain(trainCategoriesCollection, dataRow, trainsTable);
                editTrain.ShowDialog();
            }

            await GetTrains();
        }

        private void TrainNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) 
                e.Handled = true;
        }
    }
}
