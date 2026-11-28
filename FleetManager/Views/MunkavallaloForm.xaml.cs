using FleetManager.Data;
using FleetManager.Models;
using System;
using System.Net.Mail;
using System.Windows;

namespace FleetManager.Views
{
    public partial class MunkavallaloForm : Window
    {
        private readonly int? szerkesztettDolgozoId;

        public MunkavallaloForm()
        {
            InitializeComponent();

            btnMentes.Click += BtnMentes_Click;
            btnMegse.Click += BtnMegse_Click;
        }

        public MunkavallaloForm(Munkavallalo munkavallalo) : this()
        {
            szerkesztettDolgozoId = munkavallalo.DolgozoID;

            Title = "FleetManager - Munkavállaló szerkesztése";

            txtNev.Text = munkavallalo.Nev;
            txtReszleg.Text = munkavallalo.Reszleg ?? "";
            txtEmail.Text = munkavallalo.Email ?? "";
            txtTelefonszam.Text = munkavallalo.Telefonszam ?? "";
        }

        private async void BtnMentes_Click(
            object sender,
            RoutedEventArgs e)
        {
            string nev = txtNev.Text.Trim();
            string reszleg = txtReszleg.Text.Trim();
            string email = txtEmail.Text.Trim();
            string telefonszam = txtTelefonszam.Text.Trim();

            if (string.IsNullOrWhiteSpace(nev))
            {
                MessageBox.Show(
                    "A munkavállaló nevének megadása kötelező.",
                    "Hiányzó adat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtNev.Focus();
                return;
            }

            if (nev.Length > 100 ||
                reszleg.Length > 100 ||
                email.Length > 150 ||
                telefonszam.Length > 30)
            {
                MessageBox.Show(
                    "Egy vagy több mező meghaladja a megengedett hosszúságot.",
                    "Hibás adatok",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    var emailCim = new MailAddress(email);

                    if (emailCim.Address != email)
                    {
                        throw new FormatException();
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show(
                        "Adj meg egy érvényes e-mail-címet.",
                        "Hibás e-mail-cím",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtEmail.Focus();
                    return;
                }
            }

            try
            {
                btnMentes.IsEnabled = false;

                await using var db = new FleetManagerDbContext();

                Munkavallalo munkavallalo;

                if (szerkesztettDolgozoId.HasValue)
                {
                    var meglevoMunkavallalo =
                        await db.Munkavallalok.FindAsync(
                            szerkesztettDolgozoId.Value);

                    if (meglevoMunkavallalo == null)
                    {
                        MessageBox.Show(
                            "A munkavállaló már nem található az adatbázisban.",
                            "FleetManager – Hiba",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    munkavallalo = meglevoMunkavallalo;
                }
                else
                {
                    munkavallalo = new Munkavallalo();
                    db.Munkavallalok.Add(munkavallalo);
                }

                munkavallalo.Nev = nev;
                munkavallalo.Reszleg =
                    string.IsNullOrWhiteSpace(reszleg) ? null : reszleg;
                munkavallalo.Email =
                    string.IsNullOrWhiteSpace(email) ? null : email;
                munkavallalo.Telefonszam =
                    string.IsNullOrWhiteSpace(telefonszam) ? null : telefonszam;

                await db.SaveChangesAsync();

                MessageBox.Show(
                    szerkesztettDolgozoId.HasValue
                        ? "A munkavállaló adatai sikeresen módosítva!"
                        : "A munkavállaló sikeresen elmentve!",
                    "FleetManager – Sikeres mentés",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nem sikerült elmenteni a munkavállalót.\n\n{ex.Message}",
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