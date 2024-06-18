using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using AdminPanel.View.EditingWindows;
using System.Text.RegularExpressions;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminAdditionalServicesPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        DataTable servicesTable;
        public AdminAdditionalServicesPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetServices();
        }

        private async Task GetServices()
        {
            servicesTable = await dataBaseAdapter.GetTableAsync("SELECT [Name] as [Название], [Price] as [Цена] FROM Additional_Services");
            ServicesDataGrid.ItemsSource = servicesTable.DefaultView;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceNameTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести название", "Внимание");
                return;
            }

            if (ServicePriceTextBox.Text == "")
            {
                MessageBox.Show("Необходимо ввести цену", "Внимание");
                return;
            }

            if (!Regex.IsMatch($"{ServicePriceTextBox.Text}", @"(\d+.\d+)|(\d+)"))
            {
                MessageBox.Show("Введенная цена не является допустимой", "Внимание");
                return;
            }

            foreach (DataRow row in servicesTable.Rows)
            {
                if (row["Название"].ToString().Equals($"{ServiceNameTextBox.Text}", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Такая услуга уже есть", "Внимание");
                    return;
                }
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Additional_Services] (Name, [Price]) VALUES ('{ServiceNameTextBox.Text}', {ServicePriceTextBox.Text}); ");

            await GetServices();
        }


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую нужно удалить", "Внимние");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить данную строку?", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            var dataRow = ServicesDataGrid.SelectedItem as DataRowView;

            string typeName = "";
            if (dataRow != null)
                typeName = dataRow["Название"].ToString();
            else
                MessageBox.Show("Не удалос получить услугу для удаления", "Внимание");

            int selectedServiceID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM [Additional_Services] WHERE Name = '{typeName}'"));

            if (await dataBaseAdapter.GetCellAsync($"SELECT * From [Connected_Services] WHERE Additional_Services_ID = {selectedServiceID}") !=  null)
            {
                MessageBox.Show("Данная услуга уже используется", "Внимание");
                return;
            }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"DELETE FROM [Additional_Services] WHERE Name = '{typeName}'");

            await GetServices();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку которую хотите изменить", "Внимание");
                return;
            }

            var dataRow = ServicesDataGrid.SelectedItem as DataRowView;

            if (dataRow != null)
            {
                EditService editService = new EditService(servicesTable, dataRow);
                editService.ShowDialog();
            }

            await GetServices();
        }

        private void ServicePriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && (!ServicePriceTextBox.Text.Contains(".") && ServicePriceTextBox.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }
    }
}
