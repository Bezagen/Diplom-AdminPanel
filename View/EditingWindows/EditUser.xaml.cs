using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Collections.ObjectModel;
using AdminPanel.View.Pages.Admin;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditUser : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<UserStructure> usersCollection = new ObservableCollection<UserStructure>();
        private UserStructure selectedUser;
        public EditUser(ObservableCollection<UserStructure> users, UserStructure user)
        {
            InitializeComponent();
            usersCollection = users;
            selectedUser = user;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            int selectedUserTypeIndex = UserTypeComboBox.Items.IndexOf(selectedUser.Type);

            UserTypeComboBox.SelectedIndex = selectedUserTypeIndex;

            UserNameTextBox.Text = selectedUser.Name;
            UserSurNameTextBox.Text = selectedUser.Surname;
            UserPrtronymicTextBox.Text = selectedUser.Patronymic;
            BirtDayPicker.Text = selectedUser.BirthDate;
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

        private void BirtDayPicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!DateTime.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
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

            if (BirtDayPicker.Text == "")
            {
                MessageBox.Show("Необходимо выбрать дату рождения", "Внимание");
                return;
            }

            int selectedUserTypeID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM User_Type WHERE Name = '{UserTypeComboBox.SelectedValue}'"));

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE[User] SET Surname = '{UserSurNameTextBox.Text}', [Name] = '{UserNameTextBox.Text}', Patronymic = '{UserPrtronymicTextBox.Text}', DateOfBirth = '{BirtDayPicker.Text}' WHERE ID = {selectedUser.ID}");

            this.Close();
        }
    }
}
