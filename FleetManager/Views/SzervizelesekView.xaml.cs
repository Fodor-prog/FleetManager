using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager.Views
{
    public partial class SzervizelesekView : UserControl
    {
        private List<Szervizeles> szervizelesek = new();

        public SzervizelesekView()
        {
            InitializeComponent();

            Loaded += SzervizelesekView_Loaded;

            txtSzervizKereses.TextChanged += TxtSzervizKereses_TextChanged;

            btnUjSzerviz.Click += BtnUjSzerviz_Click;
            btnSzervizSzerkesztes.Click += BtnSzervizSzerkesztes_Click;
            btnSzervizTorles.Click += BtnSzervizTorles_Click;
        }

        private async void SzervizelesekView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await SzervizelesekBetolteseAsync();
        }

        private async Task SzervizelesekBetolteseAsync()
        {
            try
            {
                await using var db = new FleetManagerDbContext();

                szervizelesek = await db.Szervizelesek
                    .Include(s => s.Gepjarmu)
                    .AsNoTracking()
                    .OrderByDescending(s => s.SzervizDatum)
                    .ThenByDescending(s => s.SzervizID)
                    .ToListAsync();

                SzervizelesekSzurese();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült betölteni a szervizeléseket.\n\n{ex.Message}",
                    "FleetManager – Adatbázishiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TxtSzervizKereses_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            SzervizelesekSzurese();
        }

        private void SzervizelesekSzurese()
        {
            string kereses = txtSzervizKereses.Text.Trim();

            if (string.IsNullOrWhiteSpace(kereses))
            {
                dgSzervizelesek.ItemsSource = szervizelesek;
                return;
            }

            var talalatok = szervizelesek
                .Where(s =>
                    (s.Gepjarmu?.Rendszam.Contains(
                        kereses,
                        StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    s.SzervizTipus.Contains(
                        kereses,
                        StringComparison.CurrentCultureIgnoreCase) ||
                    (s.Leiras?.Contains(
                        kereses,
                        StringComparison.CurrentCultureIgnoreCase) ?? false))
                .ToList();

            dgSzervizelesek.ItemsSource = talalatok;
        }

        private async void BtnUjSzerviz_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ablak = new SzervizelesForm
            {
                Owner = Window.GetWindow(this)
            };

            bool? sikeresMentes = ablak.ShowDialog();

            if (sikeresMentes == true)
            {
                await SzervizelesekBetolteseAsync();
            }
        }

        private async void BtnSzervizSzerkesztes_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgSzervizelesek.SelectedItem is not Szervizeles kivalasztottSzervizeles)
            {
                MessageBox.Show(
                    "Először válassz ki egy szervizelést a táblázatból!",
                    "FleetManager – Nincs kiválasztott szervizelés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var ablak = new SzervizelesForm(kivalasztottSzervizeles)
            {
                Owner = Window.GetWindow(this)
            };

            bool? sikeresMentes = ablak.ShowDialog();

            if (sikeresMentes == true)
            {
                await SzervizelesekBetolteseAsync();
            }
        }

        private async void BtnSzervizTorles_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgSzervizelesek.SelectedItem is not Szervizeles kivalasztottSzervizeles)
            {
                MessageBox.Show(
                    "Először válassz ki egy szervizelést a táblázatból!",
                    "FleetManager – Nincs kiválasztott szervizelés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string rendszam = kivalasztottSzervizeles.Gepjarmu?.Rendszam
                ?? "Ismeretlen rendszám";

            MessageBoxResult valasz = MessageBox.Show(
                $"Biztosan törölni szeretnéd ezt a szervizelést?\n\n" +
                $"Rendszám: {rendszam}\n" +
                $"Dátum: {kivalasztottSzervizeles.SzervizDatum:yyyy.MM.dd}\n" +
                $"Típus: {kivalasztottSzervizeles.SzervizTipus}\n\n" +
                "A művelet nem vonható vissza!",
                "FleetManager – Szervizelés törlése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (valasz != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                btnSzervizTorles.IsEnabled = false;

                await using var db = new FleetManagerDbContext();

                Szervizeles? szervizeles = await db.Szervizelesek
                    .FindAsync(kivalasztottSzervizeles.SzervizID);

                if (szervizeles == null)
                {
                    MessageBox.Show(
                        "A kiválasztott szervizelés már nem található az adatbázisban.",
                        "FleetManager – Hiba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    await SzervizelesekBetolteseAsync();
                    return;
                }

                db.Szervizelesek.Remove(szervizeles);

                await db.SaveChangesAsync();

                MessageBox.Show(
                    "A szervizelés sikeresen törölve!",
                    "FleetManager – Sikeres törlés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await SzervizelesekBetolteseAsync();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show(
                    "Nem sikerült törölni a szervizelést az adatbázisból.",
                    "FleetManager – Törlési hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült törölni a szervizelést.\n\n{ex.Message}",
                    "FleetManager – Törlési hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnSzervizTorles.IsEnabled = true;
            }
        }
    }
}