using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using AdminPanel.View.EditingWindows;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminUserTypePage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        DataTable userTypesTable;
        public AdminUserTypePage()
        {
            InitializeComponent();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserTypeNameTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести название", "Внимание");
                return;
            }

            if (UserTypeAccessLevelTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести цену", "Внимание");
                return;
            }

            foreach (DataRow row in userTypesTable.Rows)
            {
                if (row["Название"].ToString().Equals($"{UserTypeNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такой тип пользователя уже есть", "Внимание");
                    return;
                }
            }

            int maxID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync("SELECT MAX(ID) AS MaxID FROM [User_Type]")) + 1;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [User_Type] (ID, Name, AccessLevel) VALUES ({maxID}, '{UserTypeNameTextBox.Text}', {UserTypeAccessLevelTextBox.Text})");

            await GetUserTypes();
        }

        private async Task GetUserTypes()
        {
            userTypesTable = await dataBaseAdapter.GetTableAsync("SELECT [Name] as Название, AccessLevel as [Уровень доступа] FROM User_Type");
            UserTypeDataGrid.ItemsSource = userTypesTable.DefaultView;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetUserTypes();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserTypeDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно удалить", "Внимние");
                return;
            }

            MessageBoxResult result = MessageBox.Show("УДАЛЕНИЕ ТИПОВ ПОЛЬЗОВАТЕЛЕЙ МОЖЕТ ПРИВЕСТИ К СЕРЬЕЗНЫМ ПРОБЛЕМАМ", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            var dataRow = UserTypeDataGrid.SelectedItem as DataRowView;

            string typeName = "";
            if (dataRow != null)
                typeName = dataRow["Название"].ToString();
            else
                MessageBox.Show("Не удалось получить тип пользователя для удаления", "Внимание");

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [User_Type] WHERE Name = '{typeName}'");

            await GetUserTypes();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserTypeDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую хотите изменить", "Внимание");
                return;
            }

            var dataRow = UserTypeDataGrid.SelectedItem as DataRowView;

            if (dataRow != null)
            {
                EditUserType editUserType = new EditUserType(userTypesTable, dataRow);
                editUserType.ShowDialog();
            }

            await GetUserTypes();
        }

        private void UserTypeAccessLevelTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
    }
}
