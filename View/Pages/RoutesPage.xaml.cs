﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;

using AdminPanel.Model.Structures;
using AdminPanel.Logic;
using System.Collections.ObjectModel;

namespace AdminPanel.View.Pages
{
    public partial class RoutesPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        public RoutesPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable tempCollectionRoutes = await dataBaseAdapter.GetTableAsync("SELECT Depart.Name as DeparturePoint, Destin.Name as DestinationPoint FROM Route JOIN City as Depart ON Depart.ID = Route.ID_City_Of_Departure JOIN City AS Destin ON Destin.ID = Route.ID_Destination_City");
                DataRow[] row = tempCollectionRoutes.Select();

                ObservableCollection<Route> routes = new ObservableCollection<Route>();

                for (int i = 0; i < row.Length; i++)
                {
                    Route route = new Route();
                    route.DeparturePoint = row[i]["DeparturePoint"].ToString();
                    route.DestinationPoint = row[i]["DestinationPoint"].ToString();
                    routes.Add(route);
                }

                RoutesListBox.ItemsSource = routes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}", "Ошибка");
            }
        }
    }
}
