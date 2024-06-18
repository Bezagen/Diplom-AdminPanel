using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using AdminPanel.View.EditingWindows;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminUserPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        DataTable usersTable;
        ObservableCollection<string> userTypesCollection = new ObservableCollection<string>();
        ObservableCollection<UserStructure> usersCollection = new ObservableCollection<UserStructure>();

        public AdminUserPage()
        {
            InitializeComponent();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать тип пользователя отправления", "Внимание");
                return;
            }

            if (UserSurNameTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести фамилию", "Внимание");
                return;
            }

            if (UserNameTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести имя", "Внимание");
                return;
            }

            if (UserLoginTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести логин", "Внимание");
                return;
            }

            if (UserPasswordTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести пароль", "Внимание");
                return;
            }

            if (BirtDayPicker.Text == "")
            {
                MessageBox.Show("Необходимо выбрать дату рождения", "Внимание");
                return;
            }
            SecurityService securityService = new SecurityService();

            string encryptedLogin = securityService.GetHashedText(UserLoginTextBox.Text),
                   encryptedPassword = securityService.GetHashedText(UserPasswordTextBox.Text);

            if (await dataBaseAdapter.GetCellAsync($"SELECT Login FROM [User] WHERE Login = '{encryptedLogin}'") != null)
            {
                MessageBox.Show("Такой логин уже существует", "Внимание");
                return;
            }

            int selectedUserTypeID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM User_Type WHERE Name = '{UserTypeComboBox.SelectedValue}'"));

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [User] (ID_User_Type, Surname, [Name], Patronymic, DateOfBirth, [Login], [Password]) VALUES({selectedUserTypeID}, '{UserSurNameTextBox.Text}', '{UserNameTextBox.Text}', '{UserPrtronymicTextBox.Text}', '{BirtDayPicker.Text}', '{encryptedLogin}', '{encryptedPassword}')");

            await GetUsers();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetUsers();

            UserDataGrid.ItemsSource = usersCollection;

            await GetUserTypes();

            UserTypeComboBox.ItemsSource = userTypesCollection;

            BirtDayPicker.DisplayDateEnd = DateTime.Now;
        }

        private async Task GetUserTypes()
        {
            DataTable userTypesTable = await dataBaseAdapter.GetTableAsync("SELECT Name FROM [User_Type]");
            DataRow[] rows = userTypesTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                userTypesCollection.Add(rows[i]["Name"].ToString());
            }
        }

        private async Task GetUsers()
        {
            usersTable = await dataBaseAdapter.GetTableAsync("SELECT [User].ID, [User].Surname as [Фамилия], [User].[Name] as [Имя], [User].Patronymic as [Отчество], [User_Type].Name AS [Тип пользователя], [User].DateOfBirth as [Дата рождения] FROM [User] JOIN [User_Type] ON [User].ID_User_Type = [User_Type].ID");
            
            DataRow[] rows = usersTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                UserStructure userStructure = new UserStructure();
                userStructure.ID = Convert.ToInt32(rows[i]["ID"]);
                userStructure.Name = rows[i]["Имя"].ToString();
                userStructure.Surname = rows[i]["Фамилия"].ToString();
                userStructure.Patronymic = rows[i]["Отчество"].ToString();
                userStructure.BirthDate = rows[i]["Дата рождения"].ToString();
                userStructure.Type = rows[i]["Тип пользователя"].ToString();
                usersCollection.Add(userStructure);
            }

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать строку для удаления", "Внимание");
                return;
            }
            MessageBoxResult result = MessageBox.Show("БУДЬТЕ ОСТОРОЖНЫ\nУДАЛЕНИЕ ПОЛЬЗОВАТЕЛЕЙ МОЖЕТ ПРИВЕСТИ К ПРОБЛЕМАМ", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            UserStructure selectedUser = UserDataGrid.SelectedItem as UserStructure;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [User] WHERE ID = {selectedUser.ID}");

            await GetUsers();

            UserDataGrid.ItemsSource = null;
            UserDataGrid.ItemsSource = usersCollection;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class UserStructure
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Type { get; set; }
        public string BirthDate { get; set; }
    }
}
