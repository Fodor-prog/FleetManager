using FleetManager.Data;
using FleetManager.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager.Views
{
    public partial class GepjarmuForm : Window
    {
        public GepjarmuForm()
        {
            InitializeComponent();

            btnMentes.Click += BtnMentes_Click;
            btnMegse.Click += BtnMegse_Click;
        }

        private async void BtnMentes_Click(
            object sender,
            RoutedEventArgs e)
        {
            string rendszam = txtRendszam.Text.Trim().ToUpperInvariant();
            string marka = txtMarka.Text.Trim();
            string modell = txtModell.Text.Trim();
            string alvazszam = txtAlvazszam.Text.Trim();

            if (string.IsNullOrWhiteSpace(rendszam) ||
                string.IsNullOrWhiteSpace(marka) ||
                string.IsNullOrWhiteSpace(modell))
            {
                MessageBox.Show(
                    "A rendszám, a márka és a modell megadása kötelező.",
                    "Hiányzó adatok",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (rendszam.Length > 20 ||
                marka.Length > 50 ||
                modell.Length > 50 ||
                alvazszam.Length > 17)
            {
                MessageBox.Show(
                    "Egy vagy több mező meghaladja a megengedett hosszúságot.",
                    "Hibás adatok",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int? evjarat = null;

            if (!string.IsNullOrWhiteSpace(txtEvjarat.Text))
            {
                if (!int.TryParse(txtEvjarat.Text.Trim(), out int megadottEvjarat) ||
                    megadottEvjarat < 1886 ||
                    megadottEvjarat > DateTime.Now.Year + 1)
                {
                    MessageBox.Show(
                        "Adj meg egy érvényes évjáratot.",
                        "Hibás évjárat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                evjarat = megadottEvjarat;
            }

            if (!int.TryParse(txtKilometerora.Text.Trim(), out int kilometerora) ||
                kilometerora < 0)
            {
                MessageBox.Show(
                    "A kilométeróra-állás nem lehet negatív, és egész számnak kell lennie.",
                    "Hibás kilométeróra-állás",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string? uzemanyag =
                (cmbUzemanyag.SelectedItem as ComboBoxItem)?.Content.ToString();

            string allapot =
                (cmbAllapot.SelectedItem as ComboBoxItem)?.Content.ToString()
                ?? "Aktív";

            var gepjarmu = new Gepjarmu
            {
                Rendszam = rendszam,
                Marka = marka,
                Modell = modell,
                Evjarat = evjarat,
                Alvazszam = string.IsNullOrWhiteSpace(alvazszam)
                    ? null
                    : alvazszam,
                Uzemanyag = uzemanyag,
                Kilometerora = kilometerora,
                MuszakiLejarat = dpMuszakiLejarat.SelectedDate,
                BiztositasLejarat = dpBiztositasLejarat.SelectedDate,
                Allapot = allapot,
                DolgozoID = null
            };

            try
            {
                btnMentes.IsEnabled = false;

                await using var db = new FleetManagerDbContext();

                db.Gepjarmuvek.Add(gepjarmu);

                await db.SaveChangesAsync();

                MessageBox.Show(
                    "A gépjármű sikeresen elmentve!",
                    "FleetManager – Sikeres mentés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sqlEx &&
                      (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                MessageBox.Show(
                    "Ezzel a rendszámmal már szerepel gépjármű az adatbázisban.",
                    "Duplikált rendszám",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült elmenteni a gépjárművet.\n\n{ex.Message}",
                    "FleetManager – Mentési hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnMentes.IsEnabled = true;
            }
        }

        private void BtnMegse_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}