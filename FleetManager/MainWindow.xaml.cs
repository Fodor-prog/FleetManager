using FleetManager.Data;
using FleetManager.Views;
using Microsoft.EntityFrameworkCore;
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
            btnUjGepjarmu.Click += BtnUjGepjarmu_Click;
        }

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await GepjarmuvekBetolteseAsync();
        }

        private async System.Threading.Tasks.Task GepjarmuvekBetolteseAsync()
        {
            try
            {
                await using var db = new FleetManagerDbContext();

                var gepjarmuvek = await db.Gepjarmuvek
                    .AsNoTracking()
                    .OrderBy(g => g.Rendszam)
                    .ToListAsync();

                dgGepjarmuvek.ItemsSource = gepjarmuvek;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült betölteni a gépjárműveket.\n\n{ex.Message}",
                    "FleetManager – Adatbázishiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnUjGepjarmu_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ablak = new GepjarmuForm
            {
                Owner = this
            };

            bool? sikeresMentes = ablak.ShowDialog();

            if (sikeresMentes == true)
            {
                await GepjarmuvekBetolteseAsync();
            }
        }
    }
}