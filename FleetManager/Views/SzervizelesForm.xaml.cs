using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FleetManager.Views
{
    public partial class SzervizelesForm : Window
    {
        private readonly int? szerkesztettSzervizId;
        private readonly int? eredetiGepjarmuId;

        public SzervizelesForm()
        {
            InitializeComponent();

            Loaded += SzervizelesForm_Loaded;
            btnMentes.Click += BtnMentes_Click;
            btnMegse.Click += BtnMegse_Click;

            btnMentes.IsEnabled = false;
        }

        public SzervizelesForm(Szervizeles szervizeles) : this()
        {
            szerkesztettSzervizId = szervizeles.SzervizID;
            eredetiGepjarmuId = szervizeles.GepjarmuID;

            Title = "FleetManager - Szervizelés szerkesztése";

            dpSzervizDatum.SelectedDate = szervizeles.SzervizDatum;
            cmbSzervizTipus.Text = szervizeles.SzervizTipus;
            txtLeiras.Text = szervizeles.Leiras ?? "";
            txtKoltseg.Text = szervizeles.Koltseg.ToString(
                CultureInfo.CurrentCulture);

            txtKilometerora.Text = szervizeles.Kilometerora?.ToString() ?? "";
        }

        private async void SzervizelesForm_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (!szerkesztettSzervizId.HasValue)
            {
                dpSzervizDatum.SelectedDate = DateTime.Today;
            }

            try
            {
                await using var db = new FleetManagerDbContext();

                var gepjarmuvek = await db.Gepjarmuvek
                    .AsNoTracking()
                    .OrderBy(g => g.Rendszam)
                    .ToListAsync();

                cmbGepjarmu.ItemsSource = gepjarmuvek;

                if (gepjarmuvek.Count == 0)
                {
                    MessageBox.Show(
                        "Az adatbázisban nincs gépjármű.",
                        "FleetManager – Nincs gépjármű",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (eredetiGepjarmuId.HasValue)
                {
                    cmbGepjarmu.SelectedValue = eredetiGepjarmuId.Value;
                }

                btnMentes.IsEnabled = true;
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

        private async void BtnMentes_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (cmbGepjarmu.SelectedItem is not Gepjarmu kivalasztottGepjarmu)
            {
                MessageBox.Show(
                    "Válassz ki egy gépjárművet!",
                    "Hiányzó adat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!dpSzervizDatum.SelectedDate.HasValue)
            {
                MessageBox.Show(
                    "Add meg a szervizelés dátumát!",
                    "Hiányzó adat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            DateTime szervizDatum = dpSzervizDatum.SelectedDate.Value.Date;

            if (szervizDatum > DateTime.Today)
            {
                MessageBox.Show(
                    "A szervizelési előzmény dátuma nem lehet jövőbeli.",
                    "Hibás dátum",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string szervizTipus = cmbSzervizTipus.Text.Trim();
            string leiras = txtLeiras.Text.Trim();

            if (string.IsNullOrWhiteSpace(szervizTipus))
            {
                MessageBox.Show(
                    "A szerviz típusának megadása kötelező.",
                    "Hiányzó adat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (szervizTipus.Length > 100 || leiras.Length > 500)
            {
                MessageBox.Show(
                    "A szerviz típusa legfeljebb 100, a leírás legfeljebb 500 karakter lehet.",
                    "Túl hosszú szöveg",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!decimal.TryParse(
                    txtKoltseg.Text.Trim(),
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal koltseg) ||
                koltseg < 0 ||
                koltseg > 9999999999.99m ||
                decimal.Round(koltseg, 2) != koltseg)
            {
                MessageBox.Show(
                    "Adj meg egy érvényes, nem negatív költséget, legfeljebb két tizedesjeggyel.",
                    "Hibás költség",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int? kilometerora = null;

            if (!string.IsNullOrWhiteSpace(txtKilometerora.Text))
            {
                if (!int.TryParse(
                        txtKilometerora.Text.Trim(),
                        out int megadottKilometer) ||
                    megadottKilometer < 0)
                {
                    MessageBox.Show(
                        "A kilométeróra-állás nem lehet negatív, és egész számnak kell lennie.",
                        "Hibás kilométeróra-állás",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                kilometerora = megadottKilometer;
            }

            try
            {
                btnMentes.IsEnabled = false;

                await using var db = new FleetManagerDbContext();

                bool letezikGepjarmu = await db.Gepjarmuvek
                    .AnyAsync(g => g.GepjarmuID == kivalasztottGepjarmu.GepjarmuID);

                if (!letezikGepjarmu)
                {
                    MessageBox.Show(
                        "A kiválasztott gépjármű már nem található az adatbázisban.",
                        "FleetManager – Hiba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Szervizeles szervizeles;

                if (szerkesztettSzervizId.HasValue)
                {
                    Szervizeles? meglevoSzervizeles = await db.Szervizelesek
                        .FindAsync(szerkesztettSzervizId.Value);

                    if (meglevoSzervizeles == null)
                    {
                        MessageBox.Show(
                            "A szerkesztendő szervizelés már nem található az adatbázisban.",
                            "FleetManager – Hiba",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    szervizeles = meglevoSzervizeles;
                }
                else
                {
                    szervizeles = new Szervizeles();
                    db.Szervizelesek.Add(szervizeles);
                }

                szervizeles.GepjarmuID = kivalasztottGepjarmu.GepjarmuID;
                szervizeles.SzervizDatum = szervizDatum;
                szervizeles.SzervizTipus = szervizTipus;
                szervizeles.Leiras = string.IsNullOrWhiteSpace(leiras)
                    ? null
                    : leiras;
                szervizeles.Koltseg = koltseg;
                szervizeles.Kilometerora = kilometerora;

                await db.SaveChangesAsync();

                MessageBox.Show(
                    szerkesztettSzervizId.HasValue
                        ? "A szervizelés módosítása sikeres!"
                        : "A szervizelés sikeresen elmentve!",
                    "FleetManager – Sikeres mentés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült elmenteni a szervizelést.\n\n{ex.Message}",
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