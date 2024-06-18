using System;
using System.Windows;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;

namespace AdminPanel.View.EditingWindows
{
    public partial class EditUserType : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private DataTable userTypesTable;
        private DataRowView selectedRow;
        public EditUserType(DataTable dataTable, DataRowView dataRow)
        {
            InitializeComponent();

            userTypesTable = dataTable;
            selectedRow = dataRow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UserTypeNameTextBox.Text = selectedRow["Название"].ToString();

            UserTypeAccessLevelTextBox.Text = selectedRow["Уровень доступа"].ToString();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
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

            if (UserTypeNameTextBox.Text != selectedRow["Название"].ToString())
                foreach (DataRow row in userTypesTable.Rows)
                {
                    if (row["Название"].ToString().Equals($"{UserTypeNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Такой тип пользователя уже есть", "Внимание");
                        return;
                    }
                }

            _ = await dataBaseAdapter.GetTableAsync($"UPDATE [User_Type] SET Name = '{UserTypeNameTextBox.Text}', AccessLevel = {UserTypeAccessLevelTextBox.Text} WHERE Name = '{selectedRow["Название"].ToString()}'");

            this.Close();
        }

        private void UserTypeAccessLevelTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
