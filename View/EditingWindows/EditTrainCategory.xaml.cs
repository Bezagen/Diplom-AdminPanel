using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditTrainCategory : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private string selectedTrainCategory;
        private DataTable citiesTable;
        public EditTrainCategory(string cityName, DataTable cities)
        {
            InitializeComponent();
            selectedTrainCategory = cityName;
            TrainCategoryNameTextBox.Text = selectedTrainCategory;

            citiesTable = cities;
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
            DataRow[] rows = citiesTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i]["Название"].ToString() == TrainCategoryNameTextBox.Text)
                {
                    MessageBox.Show("Такой город уже есть в базе данных", "Внимание");
                    return;
                }
            }
            try
            {
                _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE [Train_Category] SET Name = '{TrainCategoryNameTextBox.Text}' WHERE Name = '{selectedTrainCategory}'");
                this.Close();
            }
            catch
            {
                MessageBox.Show("Не удалось обновить запись", "Ошибка");
            }
        }
    }
}
