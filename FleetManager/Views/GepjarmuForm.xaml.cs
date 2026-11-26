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
        private readonly int? szerkesztettGepjarmuId;

        public GepjarmuForm()
        {
            InitializeComponent();

            btnMentes.Click += BtnMentes_Click;
            btnMegse.Click += BtnMegse_Click;
        }

        public GepjarmuForm(Gepjarmu gepjarmu) : this()
        {
            szerkesztettGepjarmuId = gepjarmu.GepjarmuID;

            Title = "FleetManager - Gépjármű szerkesztése";

            txtRendszam.Text = gepjarmu.Rendszam;
            txtMarka.Text = gepjarmu.Marka;
            txtModell.Text = gepjarmu.Modell;
            txtEvjarat.Text = gepjarmu.Evjarat?.ToString() ?? "";
            txtAlvazszam.Text = gepjarmu.Alvazszam ?? "";
            txtKilometerora.Text = gepjarmu.Kilometerora.ToString();

            dpMuszakiLejarat.SelectedDate = gepjarmu.MuszakiLejarat;
            dpBiztositasLejarat.SelectedDate = gepjarmu.BiztositasLejarat;

            ComboBoxErtekKivalasztasa(cmbUzemanyag, gepjarmu.Uzemanyag);
            ComboBoxErtekKivalasztasa(cmbAllapot, gepjarmu.Allapot);
        }

        private static void ComboBoxErtekKivalasztasa(
            ComboBox comboBox,
            string? ertek)
        {
            comboBox.SelectedIndex = -1;

            foreach (var elem in comboBox.Items)
            {
                if (elem is ComboBoxItem comboBoxItem &&
                    string.Equals(
                        comboBoxItem.Content?.ToString(),
                        ertek,
                        StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = comboBoxItem;
                    return;
                }
            }
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
                if (!int.TryParse(
                        txtEvjarat.Text.Trim(),
                        out int megadottEvjarat) ||
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

            if (!int.TryParse(
                    txtKilometerora.Text.Trim(),
                    out int kilometerora) ||
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
                (cmbUzemanyag.SelectedItem as ComboBoxItem)?
                .Content?.ToString();

            string allapot =
                (cmbAllapot.SelectedItem as ComboBoxItem)?
                .Content?.ToString() ?? "Aktív";

            try
            {
                btnMentes.IsEnabled = false;

                await using var db = new FleetManagerDbContext();

                Gepjarmu gepjarmu;

                if (szerkesztettGepjarmuId.HasValue)
                {
                    var meglevoGepjarmu =
                        await db.Gepjarmuvek.FindAsync(
                            szerkesztettGepjarmuId.Value);

                    if (meglevoGepjarmu == null)
                    {
                        MessageBox.Show(
                            "A kiválasztott gépjármű már nem található az adatbázisban.",
                            "FleetManager – Hiba",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    gepjarmu = meglevoGepjarmu;
                }
                else
                {
                    gepjarmu = new Gepjarmu();
                    db.Gepjarmuvek.Add(gepjarmu);
                }

                gepjarmu.Rendszam = rendszam;
                gepjarmu.Marka = marka;
                gepjarmu.Modell = modell;
                gepjarmu.Evjarat = evjarat;
                gepjarmu.Alvazszam =
                    string.IsNullOrWhiteSpace(alvazszam)
                        ? null
                        : alvazszam;
                gepjarmu.Uzemanyag = uzemanyag;
                gepjarmu.Kilometerora = kilometerora;
                gepjarmu.MuszakiLejarat = dpMuszakiLejarat.SelectedDate;
                gepjarmu.BiztositasLejarat = dpBiztositasLejarat.SelectedDate;
                gepjarmu.Allapot = allapot;

                await db.SaveChangesAsync();

                MessageBox.Show(
                    szerkesztettGepjarmuId.HasValue
                        ? "A gépjármű adatai sikeresen módosítva!"
                        : "A gépjármű sikeresen elmentve!",
                    "FleetManager – Sikeres mentés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
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
        }
    }
}
