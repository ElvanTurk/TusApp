using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using chat_TUSAPP;
using System.Net;
using System.Net.Mail;




namespace chat_TUSAPP
{
    
    public partial class Form1 : Form 
    {
        string BaglantiAdresi = "Server=Sueda_Ulus\\SQLEXPRESS;Database=TusApp;User Id=TusApp;Password=123; connection timeout=30;";
        SqlConnection Baglanti = new SqlConnection();
        public string resimyolu;
        // Doğrulama kodunu sınıf düzeyinde tanımla
        private int kayitliDogrulamaKodu;


        public Form1()
        {
            InitializeComponent();
            // Doğrulama kodunu sınıf kurucu metodunda oluştur
            kayitliDogrulamaKodu = new Random().Next(1000, 9999);


        }
        //Şifre Hashlendi.

        public  String sifrehashle(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            panel1.BringToFront();
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btn_fotoekle_Click_1(object sender, EventArgs e)
        {
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.ImageLocation = openFileDialog.FileName;
                    resimyolu = openFileDialog.FileName;
                }
            }
        }
        private void btn_kayitol_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_kullaniciadi.Text) || string.IsNullOrEmpty(txt_sifre.Text) || string.IsNullOrEmpty(txt_email.Text) || string.IsNullOrEmpty(txt_tel.Text) || string.IsNullOrEmpty(txt_sifretekrar.Text))
            {
                MessageBox.Show("Lütfen gerekli bilgileri doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (txt_sifre.Text != txt_sifretekrar.Text)
            {
                MessageBox.Show("Şifreler Eşleşmiyor. Doğru girdiğinizden emin olun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string query = "Insert Into TusApp.dbo.kullanici(KullaniciAdi,Sifre,Email,Profil_foto,telefon) Values('" + txt_kullaniciadi + "," + txt_sifre + "," + txt_email + "," + pictureBox1 + "," + txt_tel + "')";

            string sifre_hashle = sifrehashle(txt_sifre.Text);
            // veri tabanına kullanıcı tablosundaki bilgileri alma.
            string insertQuery = "INSERT INTO kullanici (KullaniciAdi,Sifre,Email,Profil_foto,telefon) Values (@KullaniciAdi,@Sifre,@Email,@Profil_foto,@telefon)";
            // SqlConnection Baglanti = new SqlConnection();
            SqlConnection Baglanti = new SqlConnection(BaglantiAdresi);
            // Doğru olan satırları ekleyin:
            using (SqlConnection baglanti = new SqlConnection(BaglantiAdresi))
            {
                // Kullanıcı adı kontrol sorgusu
                string kontrolSorgusu = "SELECT COUNT(*) FROM kullanici WHERE KullaniciAdi = @KullaniciAdi";
                using (SqlCommand kontrolKomutu = new SqlCommand(kontrolSorgusu, baglanti))
                {
                    kontrolKomutu.Parameters.AddWithValue("@KullaniciAdi", txt_kullaniciadi.Text);
                    baglanti.Open();
                    int kullaniciAdiVarMi = (int)kontrolKomutu.ExecuteScalar();
                    if (kullaniciAdiVarMi > 0)
                    {
                        MessageBox.Show("Bu kullanıcı adı zaten mevcut. Lütfen başka bir kullanıcı adı seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                    // Gelen doğrulama kodunu al
                    string gelenDogrulamaKodu = textBox1.Text;

                    // Gönderilen doğrulama koduyla karşılaştır
                    if (gelenDogrulamaKodu == kayitliDogrulamaKodu.ToString())
                    {
                        MessageBox.Show("Kaydınız Yapıldı");
                        // Kayıt işlemini yap
                        // ...
                    }
                    else
                    {
                        // Doğrulama kodu yanlış
                        label11.Text = "Doğrulama kodu yanlıştır.";
                    
                    }

                }

                using (SqlCommand command = new SqlCommand(insertQuery, Baglanti))
                {

                    //veri tabanından parametleri alma
                    command.Parameters.AddWithValue("@KullaniciAdi", txt_kullaniciadi.Text);
                    command.Parameters.AddWithValue("@Sifre", sifre_hashle);
                    command.Parameters.AddWithValue("@Email", txt_email.Text);
                    command.Parameters.AddWithValue("@Profil_foto", resimyolu);
                    command.Parameters.AddWithValue("@telefon", txt_tel.Text);

                    try
                    {
                        Baglanti.Open();
                        int Rows = command.ExecuteNonQuery();
                        if (Rows > 0)
                        {
                           
                            
                        }
                        else
                        {
                            MessageBox.Show("Kayıt sırasında bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
               
            }
        }

        private void btn_giris_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = txt_giris_kullaniciadi.Text;
            string sifre = txt_giris_sifre.Text;

            // Şifrenin hashlenmesi
            string sifre_hashle = sifrehashle(sifre);

            // Veritabanı bağlantısı
            SqlConnection baglanti = new SqlConnection(BaglantiAdresi);


            try
            {
                baglanti.Open();
                SqlCommand komut = new SqlCommand("SELECT COUNT(*) FROM kullanici WHERE KullaniciAdi = @KullaniciAdi AND Sifre = @Sifre", baglanti);
                komut.Parameters.AddWithValue("@KullaniciAdi", kullaniciAdi);
                komut.Parameters.AddWithValue("@Sifre", sifre_hashle);
                try
                {
                    int girisBasariliMi = (int)komut.ExecuteScalar();


                    if (girisBasariliMi > 0)
                    {
                        
                        this.Hide();
                        Form2 frm = new Form2(kullaniciAdi, sifre, resimyolu);
                        frm.Show();
                    }
                    else
                    {
                        MessageBox.Show(komut.Parameters[1].Value.ToString(), girisBasariliMi.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //MessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                


            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                baglanti.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel2.BringToFront();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Mail adresini al
            string mailAdresi = txt_email.Text;

            // Doğrulama kodunu e-posta olarak gönder
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("tussapp1@gmail.com", "gafr djuk qbqv carq"),
                EnableSsl = true
            };
            MailMessage mesaj = new MailMessage("tussapp1@gmail.com", mailAdresi, "Doğrulama Kodu", "Doğrulama kodunuz: " + kayitliDogrulamaKodu);
            client.Send(mesaj);

            // Mesaj görüntüle
            label10.Text = "Doğrulama kodu e-posta adresinize gönderilmiştir.";

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
