using FleetManager.Data;
using System;
using System.Windows;

namespace FleetManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await using var db = new FleetManagerDbContext();

                bool sikeresKapcsolat =
                    await db.Database.CanConnectAsync();

                if (sikeresKapcsolat)
                {
                    MessageBox.Show(
                        "Sikeres kapcsolódás a FleetManagerDB adatbázishoz!",
                        "FleetManager – Adatbázis",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Nem sikerült kapcsolódni az adatbázishoz.",
                        "FleetManager – Kapcsolati hiba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Adatbázis-kapcsolati hiba:\n\n{ex.Message}",
                    "FleetManager – Hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}