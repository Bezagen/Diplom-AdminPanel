using System;
using System.Data.SqlClient;
using System.Windows;
using AdminPanel.View;
using AdminPanel.Model.Structures;

namespace AdminPanel.Logic
{
    public class Authorization
    {
        DataBaseAdapter connection = new DataBaseAdapter();
        public Authorization() {}

        public void LogIn(string login, string password, Window currentWindow)
        {
            connection.Open();

            SecurityService security = new SecurityService();

            string encryptedLogin = security.GetHashedText(login),
                   encryptedPassword = security.GetHashedText(password);

            SqlCommand command = new SqlCommand("SELECT ID_User_Type From [User] Where @Login = Login AND @Password = Password", connection.Instance);
            command.Parameters.AddWithValue("@Login", encryptedLogin);
            command.Parameters.AddWithValue("@Password", encryptedPassword);

            object UserType = command.ExecuteScalar();
            int GetUserType = Convert.ToInt32(UserType);

            if (GetUserType > 0 && GetUserType != 1)
            {
                command = new SqlCommand($"SELECT [User].ID, [User].Name, [User].Surname, User_Type.Name AS Type_Name From [User] JOIN User_Type ON User_Type.ID = [User].ID_User_Type WHERE [User].[Login] = '{encryptedLogin}' AND [User].[Password] = '{encryptedPassword}'", connection.Instance);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            User.ID = reader.GetInt32(0);
                            User.Name = reader.GetString(1);
                            User.Surname = reader.GetString(2);
                            User.Type = reader.GetString(3);
                        }
                    }
                }
            }

            User.DateTimeEntry = DateTime.UtcNow.ToString();

            switch (GetUserType)
            {
                case 2:
                    CashierWindow cashierWindow = new CashierWindow();
                    cashierWindow.Show();
                    currentWindow.Hide();
                    break;
                case 3:
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                    currentWindow.Hide();
                    break;
                default:
                    MessageBox.Show("Неверные данные авторизации", "Ошибка");
                    break;
            }
        }
    }
}
