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
    public partial class MunkavallalokView : UserControl
    {
        private List<Munkavallalo> munkavallalok = new();

        public MunkavallalokView()
        {
            InitializeComponent();

            Loaded += MunkavallalokView_Loaded;

            btnUjMunkavallalo.Click += BtnUjMunkavallalo_Click;
            btnMunkavallaloSzerkesztes.Click += BtnMunkavallaloSzerkesztes_Click;
            btnMunkavallaloTorles.Click += BtnMunkavallaloTorles_Click;

            txtMunkavallaloKereses.TextChanged += TxtMunkavallaloKereses_TextChanged;
        }

        private async void MunkavallalokView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await MunkavallalokBetolteseAsync();
        }

        private async Task MunkavallalokBetolteseAsync()
        {
            try
            {
                await using var db = new FleetManagerDbContext();

                munkavallalok = await db.Munkavallalok
                    .AsNoTracking()
                    .OrderBy(m => m.Nev)
                    .ToListAsync();

                MunkavallalokSzurese();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült betölteni a munkavállalókat.\n\n{ex.Message}",
                    "FleetManager – Adatbázishiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TxtMunkavallaloKereses_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            MunkavallalokSzurese();
        }

        private void MunkavallalokSzurese()
        {
            string kereses = txtMunkavallaloKereses.Text.Trim();

            if (string.IsNullOrWhiteSpace(kereses))
            {
                dgMunkavallalok.ItemsSource = munkavallalok;
                return;
            }

            var talalatok = munkavallalok
                .Where(m =>
                    m.Nev.Contains(
                        kereses,
                        StringComparison.CurrentCultureIgnoreCase) ||
                    (m.Reszleg?.Contains(
                        kereses,
                        StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (m.Email?.Contains(
                        kereses,
                        StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (m.Telefonszam?.Contains(
                        kereses,
                        StringComparison.CurrentCultureIgnoreCase) ?? false))
                .ToList();

            dgMunkavallalok.ItemsSource = talalatok;
        }

        private async void BtnUjMunkavallalo_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ablak = new MunkavallaloForm
            {
                Owner = Window.GetWindow(this)
            };

            bool? sikeresMentes = ablak.ShowDialog();

            if (sikeresMentes == true)
            {
                await MunkavallalokBetolteseAsync();
            }
        }

        private async void BtnMunkavallaloSzerkesztes_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgMunkavallalok.SelectedItem is not Munkavallalo kivalasztottMunkavallalo)
            {
                MessageBox.Show(
                    "Először válassz ki egy munkavállalót a táblázatból!",
                    "FleetManager – Nincs kiválasztott munkavállaló",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var ablak = new MunkavallaloForm(kivalasztottMunkavallalo)
            {
                Owner = Window.GetWindow(this)
            };

            bool? sikeresMentes = ablak.ShowDialog();

            if (sikeresMentes == true)
            {
                await MunkavallalokBetolteseAsync();
            }
        }

        private async void BtnMunkavallaloTorles_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgMunkavallalok.SelectedItem is not Munkavallalo kivalasztottMunkavallalo)
            {
                MessageBox.Show(
                    "Először válassz ki egy munkavállalót a táblázatból!",
                    "FleetManager – Nincs kiválasztott munkavállaló",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult valasz = MessageBox.Show(
                $"Biztosan törölni szeretnéd a következő munkavállalót?\n\n" +
                $"{kivalasztottMunkavallalo.Nev}\n\n" +
                "A művelet nem vonható vissza!",
                "FleetManager – Munkavállaló törlése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (valasz != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                btnMunkavallaloTorles.IsEnabled = false;

                await using var db = new FleetManagerDbContext();

                int dolgozoId = kivalasztottMunkavallalo.DolgozoID;

                bool vanHozzarendeltGepjarmu = await db.Gepjarmuvek
                    .AnyAsync(g => g.DolgozoID == dolgozoId);

                if (vanHozzarendeltGepjarmu)
                {
                    MessageBox.Show(
                        "A munkavállaló nem törölhető, mert gépjármű van hozzárendelve.",
                        "FleetManager – Törlés nem engedélyezett",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Munkavallalo? munkavallalo = await db.Munkavallalok
                    .FindAsync(dolgozoId);

                if (munkavallalo == null)
                {
                    MessageBox.Show(
                        "A munkavállaló már nem található az adatbázisban.",
                        "FleetManager – Hiba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    await MunkavallalokBetolteseAsync();
                    return;
                }

                db.Munkavallalok.Remove(munkavallalo);

                await db.SaveChangesAsync();

                MessageBox.Show(
                    "A munkavállaló sikeresen törölve!",
                    "FleetManager – Sikeres törlés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await MunkavallalokBetolteseAsync();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show(
                    "A munkavállaló törlése nem sikerült, mert más adatbázisrekordok kapcsolódhatnak hozzá.",
                    "FleetManager – Törlési hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült törölni a munkavállalót.\n\n{ex.Message}",
                    "FleetManager – Törlési hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnMunkavallaloTorles.IsEnabled = true;
            }
        }
    }
}