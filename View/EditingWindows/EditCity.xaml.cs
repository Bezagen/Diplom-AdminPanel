using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditCity : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private string selectedCity;
        private DataTable citiesTable;
        
        public EditCity(string cityName, DataTable cities)
        {
            InitializeComponent();

            selectedCity = cityName;
            CityNameTextBox.Text = selectedCity;

            citiesTable = cities;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] rows = citiesTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i]["Название"].ToString() == CityNameTextBox.Text)
                {
                    MessageBox.Show("Такой город уже есть в базе данных", "Внимание");
                    return;
                }
            }
            try
            {
                _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE [City] SET Name = '{CityNameTextBox.Text}' WHERE Name = '{selectedCity}'");
                this.Close();
            }
            catch
            {
                MessageBox.Show("Не удалось обновить запись", "Ошибка");
            }
        }
    }
}
