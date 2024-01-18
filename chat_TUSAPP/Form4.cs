using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Policy;

namespace chat_TUSAPP
{
    public partial class Form4 : Form
    {
        string BaglantiAdresi = "Server=Sueda_Ulus\\SQLEXPRESS;Database=TusApp;User Id=TusApp;Password=123; connection timeout=30;";
        SqlConnection Baglanti = new SqlConnection();
        private string girisyapan_kullaniciadi;
        private string girisyapan_foto;
        private string girisyapan_sifre;
        private string girisyapan_email;
        private string girisyapan_telefon;
        public Form4()
        {
            InitializeComponent();
            // ComboBox'a temaları ekle
            comboBox1.Items.Add("Açık Mod");
            comboBox1.Items.Add("Koyu Mod");

            // Başlangıçta seçili tema
            comboBox1.SelectedIndex = 0;

        }
        private void KullaniciBilgisiGuncelle(int kullaniciID, string yeniKullaniciAdi, string yeniSifre, string yeniProfilFoto, string yeniDurum)
        {
            try
            {
                // Veritabanında kullanıcı bilgilerini güncelle
                var query = "UPDATE Kullanici SET KullaniciAdi = @KullaniciAdi, Sifre = @Sifre, Email = @Email, Profil_foto = @ProfilFoto, Telefon = @Telefon, Durum = @Durum WHERE KullaniciID = @KullaniciID";

                using (var connection = new SqlConnection(BaglantiAdresi))
                {
                    connection.Open();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                        command.Parameters.AddWithValue("@KullaniciAdi", yeniKullaniciAdi);
                        command.Parameters.AddWithValue("@Sifre", yeniSifre);
                        command.Parameters.AddWithValue("@ProfilFoto", yeniProfilFoto);
                        command.Parameters.AddWithValue("@Durum", yeniDurum);

                        command.ExecuteNonQuery();

                        MessageBox.Show("Kullanıcı bilgileri başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcı bilgileri güncellenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Örneğin, TextBox'lardan alınan yeni bilgilerle güncelleme yapabilirsiniz.

            string yeniKullaniciAdi = textBox3.Text;
            string yeniSifre = textBox1.Text;
            string yeniProfilFoto = pictureBox3.Text;
            string yeniDurum = textBox4.Text;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("tussapp1@gmail.com", "gafr djuk qbqv carq"),
                EnableSsl = true
            };
            client.Send("tussapp1@gmail.com", "nnefiseatik91@gmail.com", "test", "testbody");

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ComboBox'da seçilen temaya göre formu güncelle
            string secilenTema = comboBox1.SelectedItem.ToString();

            if (secilenTema == "Açık Mod")
            {
                UygulaAcikMod();
            }
            else if (secilenTema == "Koyu Mod")
            {
                UygulaKoyuMod();
            }
        }
        private void UygulaAcikMod()
        {
            // Açık Mod teması için renkleri ayarla
            this.BackColor = SystemColors.Control;
            foreach (Control control in this.Controls)
            {
                control.BackColor = SystemColors.Control;
                if (control is TextBox)
                {
                    ((ComboBox)comboBox1).ForeColor = SystemColors.ControlText;
                }
            }
        }

        private void UygulaKoyuMod()
        {
            // Koyu Mod teması için renkleri ayarla
            this.BackColor = Color.DarkGray;
            foreach (Control control in this.Controls)
            {
                control.BackColor = Color.DarkGray;
                if (control is TextBox)
                {
                    ((TextBox)control).ForeColor = Color.White;
                    ((ComboBox)control).BackgroundImage = Image.FromFile("koyumod.png");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
       


        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}
    