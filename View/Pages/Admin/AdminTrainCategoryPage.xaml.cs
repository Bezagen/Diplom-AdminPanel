using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AdminPanel.Logic;
using System.Data;
using AdminPanel.View.EditingWindows;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminTrainTypePage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable trainCategoryTable;

        public AdminTrainTypePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                trainCategoryTable = await dataBaseAdapter.GetTableAsync("SELECT Name as Название FROM [Train_Category]");

                TrainCategoryDataGrid.ItemsSource = trainCategoryTable.DefaultView;
            }
            catch
            {
                MessageBox.Show("Не удалось загрузить данные", "Ошибка");
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainCategoryNameTextBox.Text == "")
            {
                MessageBox.Show("Заполните название категории поезда", "Внимание");
                return;
            }

            foreach (DataRow row in trainCategoryTable.Rows)
            {
                if (row["Название"].ToString().Equals($"{TrainCategoryNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такая категория поезда уже есть", "Внимание");
                    return;
                }
            }

            try
            {
                _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO Train_Category (Name) VALUES('{TrainCategoryNameTextBox.Text}')");

                await UpdateTable();
            }
            catch
            {
                MessageBox.Show("Не удалось внести в запись в базу данных", "Ошибка");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TrainCategoryDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберете строку которую надо удалить", "Внимание");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить данную строку?", "Внимание");

                if (result == MessageBoxResult.No)
                    return;

                var dataRow = TrainCategoryDataGrid.SelectedItem as DataRowView;
                string selectedTrainCategory = dataRow["Название"].ToString();

                int selectedCategoryID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM Train_Category WHERE Name = '{selectedTrainCategory}'"));

                if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM Train WHERE ID_Train_Category = {selectedCategoryID}") != null)
                {
                    MessageBox.Show("Эта категория поезда используется", "Внимание");
                    return;
                }

                _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Train_Category] WHERE Name = '{selectedTrainCategory}'");

                await UpdateTable();
            }
            catch
            {
                MessageBox.Show("Не удалось удалить строку", "Ошибка");
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainCategoryDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберете строку которую хотите изменить", "Внимание");
                return;
            }

            var dataRow = TrainCategoryDataGrid.SelectedItem as DataRowView;

            string columnValue = "";

            if (dataRow != null)
                columnValue = dataRow["Название"].ToString();

            EditTrainCategory editTrainCategory = new EditTrainCategory(columnValue, trainCategoryTable);
            editTrainCategory.ShowDialog();

            try
            {
                await UpdateTable();
            }
            catch
            {
                MessageBox.Show("Не получилось обновить данные", "Внимание");
            }
        }

        private async Task UpdateTable()
        {
            trainCategoryTable = await dataBaseAdapter.GetTableAsync("SELECT Name as Название FROM [Train_Category]");
            TrainCategoryDataGrid.ItemsSource = null;
            TrainCategoryDataGrid.ItemsSource = trainCategoryTable.DefaultView;
        }
    }
}
