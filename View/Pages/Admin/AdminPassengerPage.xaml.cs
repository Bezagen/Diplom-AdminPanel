using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminPanel.Logic;
using System.Data;
using System.Collections.ObjectModel;
using AdminPanel.View.EditingWindows;

namespace AdminPanel.View.Pages.Admin
{
    public partial class AdminPassengerPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        DataTable passengerTable;
        ObservableCollection<Passenger> passengersCollection = new ObservableCollection<Passenger>();

        public AdminPassengerPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetPassengers();

            PassengerDataGrid.ItemsSource = passengersCollection;
        }

        private async Task GetPassengers()
        {
            passengerTable = await dataBaseAdapter.GetTableAsync("SELECT ID, Surname, [Name], Patronymic, DateOfBirth FROM [Passenger]; ");
            DataRow[] rows = passengerTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                Passenger passenger = new Passenger();
                passenger.ID = Convert.ToInt32(rows[i]["ID"]);
                passenger.Surname = rows[i]["Surname"].ToString();
                passenger.Name = rows[i]["Name"].ToString();
                passenger.Patronymic = rows[i]["Patronymic"].ToString();
                passenger.DateOfBirth = rows[i]["DateOfBirth"].ToString();

                passengersCollection.Add(passenger);
            }
        }
    }

    public class Passenger
    {
        public int ID { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string DateOfBirth { get; set; }
    }
}
