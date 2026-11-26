
using FleetManager.Data;
using FleetManager.Models;
using FleetManager.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
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
            btnGepjarmuSzerkesztes.Click += BtnGepjarmuSzerkesztes_Click;
            btnGepjarmuTorles.Click += BtnGepjarmuTorles_Click;
        }

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await GepjarmuvekBetolteseAsync();
        }

        private async Task GepjarmuvekBetolteseAsync()
        {
            try
            {
                await using var db = new FleetManagerDbContext();

                var gepjarmuvek = await db.Gepjarmuvek
                    .AsNoTracking()
                    .OrderBy(g => g.Rendszam)
                    .ToListAsync();

                dgGepjarmuvek.ItemsSource = gepjarmuvek;

                txtOsszesGepjarmu.Text =
                    gepjarmuvek.Count.ToString();

                txtAktivGepjarmu.Text =
                    gepjarmuvek.Count(g => g.Allapot == "Aktív").ToString();

                txtSzervizben.Text =
                    gepjarmuvek.Count(g => g.Allapot == "Szervizben").ToString();

                DateTime harmincNapMulva = DateTime.Today.AddDays(30);

                txtLejaroMuszaki.Text =
                    gepjarmuvek.Count(g =>
                        g.MuszakiLejarat.HasValue &&
                        g.MuszakiLejarat.Value.Date <= harmincNapMulva
                    ).ToString();
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

        private async void BtnGepjarmuSzerkesztes_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgGepjarmuvek.SelectedItem is not Gepjarmu kivalasztottGepjarmu)
            {
                MessageBox.Show(
                    "Először válassz ki egy gépjárművet a táblázatból!",
                    "FleetManager – Nincs kiválasztott gépjármű",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var ablak = new GepjarmuForm(kivalasztottGepjarmu)
            {
                Owner = this
            };

            bool? sikeresMentes = ablak.ShowDialog();

            if (sikeresMentes == true)
            {
                await GepjarmuvekBetolteseAsync();
            }
        }

        private async void BtnGepjarmuTorles_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgGepjarmuvek.SelectedItem is not Gepjarmu kivalasztottGepjarmu)
            {
                MessageBox.Show(
                    "Először válassz ki egy gépjárművet a táblázatból!",
                    "FleetManager – Nincs kiválasztott gépjármű",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult valasz = MessageBox.Show(
                $"Biztosan törölni szeretnéd a(z) {kivalasztottGepjarmu.Rendszam} rendszámú gépjárművet?\n\n" +
                "A művelet nem vonható vissza!",
                "FleetManager – Gépjármű törlése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (valasz != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                btnGepjarmuTorles.IsEnabled = false;

                await using var db = new FleetManagerDbContext();

                int gepjarmuId = kivalasztottGepjarmu.GepjarmuID;

                bool vanSzervizeles = await db.Szervizelesek
                    .AnyAsync(s => s.GepjarmuID == gepjarmuId);

                bool vanKilometerallas = await db.Kilometerallasok
                    .AnyAsync(k => k.GepjarmuID == gepjarmuId);

                if (vanSzervizeles || vanKilometerallas)
                {
                    MessageBox.Show(
                        "A gépjármű nem törölhető, mert szervizelési vagy kilométeróra-adatok kapcsolódnak hozzá.",
                        "FleetManager – Törlés nem engedélyezett",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Gepjarmu? gepjarmu = await db.Gepjarmuvek
                    .FindAsync(gepjarmuId);

                if (gepjarmu == null)
                {
                    MessageBox.Show(
                        "A kiválasztott gépjármű már nem található az adatbázisban.",
                        "FleetManager – Hiba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    await GepjarmuvekBetolteseAsync();
                    return;
                }

                db.Gepjarmuvek.Remove(gepjarmu);

                await db.SaveChangesAsync();

                MessageBox.Show(
                    "A gépjármű sikeresen törölve!",
                    "FleetManager – Sikeres törlés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await GepjarmuvekBetolteseAsync();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show(
                    "A gépjármű törlése nem sikerült. Lehetséges, hogy más adatbázisrekordok kapcsolódnak hozzá.",
                    "FleetManager – Törlési hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült törölni a gépjárművet.\n\n{ex.Message}",
                    "FleetManager – Törlési hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnGepjarmuTorles.IsEnabled = true;
            }
        }
    }
}