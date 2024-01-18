using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Data.Common;
using Bunifu.UI.WinForms;
using System.Reflection.Emit;
using chat_TUSAPP;


namespace chat_TUSAPP
{
    public partial class Form2 : Form
    {
        string BaglantiAdresi = "Server=Sueda_Ulus\\SQLEXPRESS;Database=TusApp;User Id=TusApp;Password=123; connection timeout=30;";
        SqlConnection Baglanti = new SqlConnection();
        private string girisyapan_kullaniciadi;
        private string girisyapan_foto;
        private string girisyapan_sifre;
        private string girisyapan_email;
        private string girisyapan_telefon;
        private Timer mesajyenileme;
        int bildirimsayi = 0;
        private DateTime sonMesajZamani;



        public Form2(string kullaniciAdi, string girisYapanKullaniciSifre, string resimyolu)
        {
            InitializeComponent();
            girisyapan_kullaniciadi = kullaniciAdi;
            girisyapan_sifre = girisYapanKullaniciSifre;
            pictureBox1.ImageLocation = resimyolu;
            // KullaniciListesiniGuncelle();
            //KullaniciBilgileriniGoster(kullaniciAdi);
            mesajyenileme = new Timer();
            mesajyenileme.Interval = 1000; // 1 saniye
            mesajyenileme.Tick += mesajyenileme_Tick;
            mesajyenileme.Start();
            sonMesajZamani = DateTime.Now;
            label1.Text = girisyapan_kullaniciadi;
            pictureBox1.ImageLocation = resimyolu;

            // Fotoğraf yolunu kontrol et ve pictureBox1'e yükle
            if (!string.IsNullOrEmpty(resimyolu) && System.IO.File.Exists(resimyolu))
            {
               
                pictureBox1.ImageLocation = resimyolu;
            }
            else
            {
                // Eğer resim yolu geçerli değilse veya resim bulunamazsa varsayılan bir resim yükle
                //pictureBox1.Image = Properties.Resources.DefaultUserImage; // Varsayılan resminiz varsa bunu kullanabilirsiniz.
            }
        }
        private void mesajyenileme_Tick(object sender, EventArgs e)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
                {
                    connection.Open();

                    int gonderenID = KullaniciAdiniIDyeCevir(label1.Text);

                    // ListView1'de seçili bir kişi varsa
                    if (listView1.SelectedItems.Count > 0)
                    {
                        // Seçili kişiyi al
                        string secilenKisi = listView1.SelectedItems[0].Text;
                        int aliciID = KullaniciAdiniIDyeCevir(secilenKisi);

                        if (gonderenID == -1 || aliciID == -1)
                        {
                            MessageBox.Show("Gönderen veya alıcı ID bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        string query = $"SELECT COUNT(*) " +
                                       $"FROM Mesaj " +
                                       $"WHERE (GonderenID = @Alici AND AliciID = @Gonderen AND MesajTuru = 1 AND OkunduBilgisi = 0) ";




                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Gonderen", gonderenID);
                            command.Parameters.AddWithValue("@Alici", aliciID);

                            int yeniMesajSayisi = (int)command.ExecuteScalar();

                            // Yeni mesaj varsa bildirim göster
                            if (yeniMesajSayisi > 0)
                            {
                                bildirimsayi = yeniMesajSayisi;
                                button9.Text = bildirimsayi.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mesaj yenileme sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel_kisiekle.BringToFront();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Kullanıcı adını al
            string yeniKullaniciAdi = txt_kisiekle.Text.Trim();

            // Veritabanına bağlantı oluştur
            using (SqlConnection baglanti = new SqlConnection(BaglantiAdresi))
            {
                baglanti.Open();

                // Sorguyu oluşturun
                SqlCommand kontrolKomut = new SqlCommand("SELECT COUNT(*) FROM kullanici WHERE KullaniciAdi = @KullaniciAdi", baglanti);
                kontrolKomut.Parameters.AddWithValue("@KullaniciAdi", yeniKullaniciAdi);

                // Sorguyu çalıştırın
                int kullaniciSayisi = (int)kontrolKomut.ExecuteScalar();

                // Kullanıcı adı bulunursa
                if (kullaniciSayisi > 0)
                {
                    // Sorguyu oluştur
                    SqlCommand eklemeKomut = new SqlCommand("INSERT INTO arkadas (KullaniciAdi1, KullaniciAdi2) VALUES (@KullaniciAdi1, @KullaniciAdi2)", baglanti);

                    // Sorguyu parametrelerle çalıştır
                    eklemeKomut.Parameters.AddWithValue("@KullaniciAdi1", label1.Text);
                    eklemeKomut.Parameters.AddWithValue("@KullaniciAdi2", yeniKullaniciAdi);

                    // Sorguyu çalıştır ve hata olup olmadığını kontrol et
                    try
                    {
                        eklemeKomut.ExecuteNonQuery();

                        // Ekleme başarılıysa mesaj göster
                        MessageBox.Show("Arkadaş Eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Arkadaş listesini güncelle
                        VeriCekVeListeyeEkle();

                        // listView2'deki kişileri listView1'den al
                        listView2.Items.Clear();
                        foreach (ListViewItem item in listView1.Items)
                        {
                            listView2.Items.Add((ListViewItem)item.Clone());
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Hata mesajını göster
                        MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Kullanıcı adı zaten var
                    MessageBox.Show("Kullanıcı adı bulunamadı. Doğru yazdığınızdan emin olun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Arkadaş listesini güncelle
            VeriCekVeListeyeEkle();


        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                // Seçilen öğeyi al
                string secilenKisi = listView1.SelectedItems[0].Text;

                // SQL sorgusu
                string query = "SELECT Profil_foto FROM kullanici WHERE KullaniciAdi = @KullaniciAdi";

                // SQL bağlantısı
                using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
                {
                    // Komut ve bağlantıyı ilişkilendir
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parametre ekle
                        command.Parameters.AddWithValue("@KullaniciAdi", secilenKisi);

                        // Bağlantıyı aç
                        connection.Open();

                        // Verileri oku
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Veritabanından gelen veriyi kontrol et
                            if (reader.Read())
                            {
                                // Profil fotoğraf yolunu al
                                string profilFotoYol = reader["Profil_foto"].ToString();

                                // Label4'e kullanıcı adını yaz
                                label4.Text = secilenKisi;

                                // Fotoğrafı göstermek için uygun bir kontrolü kullanabilirsiniz
                                // Örneğin, bir PictureBox kontrolüne atayabilirsiniz
                                pictureBox2.ImageLocation = profilFotoYol;
                            }
                        }
                    }
                }

                // KullaniciAdiniIDyeCevir fonksiyonu burada çağrılmalı
                int gonderenID = KullaniciAdiniIDyeCevir(label1.Text);
                int aliciID = KullaniciAdiniIDyeCevir(secilenKisi);

                // Mesajları göster
                MesajlariWebBrowserdaGoster(gonderenID, aliciID);
            }
        }

        private void VeriCekVeListeyeEkle()
        {
            // ListView'i temizle
            listView1.Items.Clear();
            listView1.Columns.Clear();

            // SQL sorgusu
            string query = "SELECT KullaniciAdi2 FROM arkadas WHERE KullaniciAdi1 = @KullaniciAdi";

            // SQL bağlantısı
            using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
            {
                // Komut ve bağlantıyı ilişkilendir
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Parametre ekle
                    command.Parameters.AddWithValue("@KullaniciAdi", label1.Text);

                    // Bağlantıyı aç
                    connection.Open();

                    // Verileri oku
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // View özelliğini Details olarak ayarla
                        listView1.View = View.Details;

                        // Kullanıcı Adı sütununu ekle
                        listView1.Columns.Add("KİŞİLER", 180);


                        // Her bir veri satırını ListView'e ekle
                        while (reader.Read())
                        {
                            // KullaniciAdi2 sütununu kontrol et
                            if (!reader.IsDBNull(reader.GetOrdinal("KullaniciAdi2")))
                            {
                                string arkadasAdi = reader["KullaniciAdi2"].ToString();
                                ListViewItem item = new ListViewItem(arkadasAdi);
                                listView1.Items.Add(item);
                            }
                        }
                    }
                }
            }
            listView1.SelectedIndexChanged += new EventHandler(listView1_SelectedIndexChanged);
            GruplariListeyeEkle();
        }
        private void GruplariListeyeEkle()
        { // ListView1'i temizle
            listView_gruplar.Items.Clear();
            listView_gruplar.Columns.Clear();

            // SQL sorgusu
            string query = "SELECT ID, GrupAdi FROM Grup WHERE KullaniciID = @KullaniciID";

            // SQL bağlantısı
            using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
            {
                // Komut ve bağlantıyı ilişkilendir
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Parametre ekle
                    command.Parameters.AddWithValue("@KullaniciID", KullaniciAdiniIDyeCevir(label1.Text));

                    // Bağlantıyı aç
                    connection.Open();

                    // Verileri oku
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // View özelliğini Details olarak ayarla
                        listView_gruplar.View = View.Details;

                        // Grup Adı sütununu ekle
                        listView_gruplar.Columns.Add("GRUPLAR", 180);

                        // Her bir veri satırını ListView'e ekle
                        while (reader.Read())
                        {
                            string grupAdi = reader["GrupAdi"].ToString();

                            ListViewItem item = new ListViewItem(new[] { grupAdi });
                            listView_gruplar.Items.Add(item);
                        }

                    }
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            VeriCekVeListeyeEkle();
            GruplariListeyeEkle();
        }



        private int KullaniciAdiniIDyeCevir(string kullaniciAdi)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
                {
                    connection.Open();

                    string query = "SELECT KullaniciID FROM kullanici WHERE KullaniciAdi = @KullaniciAdi";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@KullaniciAdi", kullaniciAdi);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Convert.ToInt32(reader["KullaniciID"]);
                            }
                            else
                            {
                                MessageBox.Show("Kullanıcı ID bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return -1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcı ID'ye çevirme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }


        private void button7_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen bir kişi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Seçilen kişiye gönderen ve alıcı ID'leri al
            string secilenKisi = listView1.SelectedItems[0].Text;
            int gonderenID = KullaniciAdiniIDyeCevir(label1.Text);
            int aliciID = KullaniciAdiniIDyeCevir(secilenKisi);

            // Mesajın içeriğini al
            string mesajIcerik = textBox1.Text;

            // Mesajın tarih ve saatini al
            DateTime tarih = DateTime.Now;

            // SQL sorgusu oluşturun
            string query = "INSERT INTO Mesaj (GonderenID, AliciID, Mesaj, Tarih) VALUES (@GonderenID, @AliciID, @Mesaj, @Tarih)";

            // Sorguyu veri tabanına gönderin
            using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@GonderenID", gonderenID);
                    command.Parameters.AddWithValue("@AliciID", aliciID);
                    command.Parameters.AddWithValue("@Mesaj", mesajIcerik);
                    command.Parameters.AddWithValue("@Tarih", tarih);

                    // Mesaj içeriğini kontrol et ve null ise bir değer ata
                    if (mesajIcerik == null)
                    {
                        mesajIcerik = "Bu mesaj boş.";
                    }

                    command.ExecuteNonQuery();
                }
            }
            MesajlariWebBrowserdaGoster(gonderenID, aliciID);
        }

        private void MesajGonder(int gonderenID, int aliciID, string mesajIcerik)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
                {
                    connection.Open();

                    if (gonderenID != aliciID)
                    {
                        string query = "INSERT INTO Mesaj (GonderenID, AliciID, MesajTuru, Mesaj, Tarih, OkunduBilgisi) " +
                                       "VALUES (@GonderenID, @AliciID, 1, @MesajIcerik, GETDATE(), 0)";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@GonderenID", gonderenID);
                            command.Parameters.AddWithValue("@AliciID", aliciID);
                            command.Parameters.AddWithValue("@MesajIcerik", mesajIcerik);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mesaj gönderirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MesajlariWebBrowserdaGoster(int gonderenID, int aliciID)
        {
            try
            {
                // SQL sorgusu
                string query = "SELECT Mesaj, GonderenID, AliciID, Tarih FROM Mesaj WHERE (GonderenID = @GonderenID AND AliciID = @AliciID) OR (GonderenID = @AliciID AND AliciID = @GonderenID) ORDER BY Tarih";

                // SQL bağlantısı
                using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
                {
                    // Komut ve bağlantıyı ilişkilendir
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parametreleri ekle
                        command.Parameters.AddWithValue("@GonderenID", gonderenID);
                        command.Parameters.AddWithValue("@AliciID", aliciID);

                        // Bağlantıyı aç
                        connection.Open();

                        // Verileri oku
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // HTML içeriği oluştur
                            StringBuilder htmlContent = new StringBuilder();
                            htmlContent.Append(@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
   
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 10px;
        }

        #messageContainer {
            max-height: 300px;
            overflow-y: auto;
            border: 1px solid #ccc;
            border-radius: 5px;
            padding: 10px;
        }

        .message {
            margin-bottom: 10px;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
            max-width: 70%;
        }

        .sentMessage {
            align-self: flex-end;
            background-color: #DCF8C6;
        }

        .receivedMessage {
            align-self: flex-start;
            background-color: #E0E0E0;
        }
    </style>
</head>
<body>
  
    
    

    <script>
        function sendMessage() {
            var sender = document.getElementById(""sender"").value;
            var message = document.getElementById(""textBox1"").value;
            var messageContainer = document.getElementById(""messageContainer"");
            var formattedTime = new Date().toLocaleString();
            var messageDiv = document.createElement(""div"");
            
            if (message) {
                var messageHTML = '<p><strong>' + sender + '</strong>: ' + message + '</p><span>' + formattedTime + '</span>';
                messageDiv.innerHTML = messageHTML;
                messageDiv.classList.add(""message"", ""sentMessage"");
                messageContainer.appendChild(messageDiv);
                messageContainer.scrollTop = messageContainer.scrollHeight;
                document.getElementById(""textBox1"").value = """";
            }
        }
    </script>
</body>
</html>

"



                                );

                            // Her bir mesajı HTML içeriğine ekle
                            while (reader.Read())
                            {
                                string mesaj = reader["Mesaj"].ToString();
                                int gonderen = Convert.ToInt32(reader["GonderenID"]);
                                DateTime tarih = Convert.ToDateTime(reader["Tarih"]);

                                // Mesajı HTML içeriğine ekle
                                htmlContent.AppendFormat("<p><strong>{0}</strong>: {1} ({2})</p>", (gonderen == gonderenID ? label1.Text : label4.Text), mesaj, tarih);
                            }

                            // HTML içeriğini tamamla
                            htmlContent.Append("</body></html>");

                            // WebBrowser kontrolüne HTML içeriğini yükle
                            webBrowser1.DocumentText = htmlContent.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mesajları gösterirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen bir kişi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Seçilen kişiye gönderen ve alıcı ID'leri al
            string secilenKisi = listView1.SelectedItems[0].Text;

            // Kullanıcı ID'yi al
            int kullaniciID = KullaniciAdiniIDyeCevir(secilenKisi);

            if (kullaniciID == -1)
            {
                MessageBox.Show("Seçilen kişi veritabanında bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Seçilen kişiye gönderen ve alıcı ID'leri al
            int gonderenID = KullaniciAdiniIDyeCevir(label1.Text);
            int aliciID = KullaniciAdiniIDyeCevir(secilenKisi);

            // SQL sorgusu oluşturun
            string query = "DELETE FROM arkadas WHERE (KullaniciAdi1 = @KullaniciAdi1 AND KullaniciAdi2 = @KullaniciAdi2) OR (KullaniciAdi1 = @KullaniciAdi2 AND KullaniciAdi2 = @KullaniciAdi1)";

            // Sorguyu veri tabanına gönderin
            using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@KullaniciAdi1", label1.Text);
                    command.Parameters.AddWithValue("@KullaniciAdi2", secilenKisi);

                    // Sorguyu çalıştır ve hata olup olmadığını kontrol et
                    try
                    {
                        command.ExecuteNonQuery();

                        // Silme başarılıysa mesaj göster
                        MessageBox.Show("Arkadaş silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        // Hata mesajını göster
                        MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            // Seçilen kişiyi listView1'den kaldır
            listView1.Items.Remove(listView1.SelectedItems[0]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel4.BringToFront();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Seçili kişileri listView1'den al
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                // Yeni bir ListViewItem oluştur
                ListViewItem newItem = new ListViewItem(item.Text);

                // Kişiyi listView2'ye ekleyin
                listView2.Items.Add(newItem);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            // Yeni bir Form4 örneği oluşturun
            Form4 form4 = new Form4();

            // Mevcut formu gizleyin
            this.Hide();

            // Form4'ü gösterin
            form4.Show();

        }

        private void grup_olustur_Click(object sender, EventArgs e)
        {
            // Grup adını al
            string grupAdi = textBox2.Text.Trim();

            // Grubu oluştur
            int grupID = GrupOlustur(grupAdi);

            // Grup ID'sini kontrol et
            if (grupID > 0)
            {
                // Seçili üyeleri al
                List<string> seciliUyeler = new List<string>();
                foreach (ListViewItem item in listView2.Items)
                {
                    seciliUyeler.Add(item.Text);
                }

                // Oluşturulan gruba üyeleri ekle
                GrubaUyeleriEkle(grupID, seciliUyeler);

                // Başarılı bir şekilde grupta üyeleri eklediyse bilgi mesajı göster
                MessageBox.Show("Grup başarıyla oluşturuldu ve üyeler eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Grup adı ve üyeleri temizle
                textBox2.Clear();
                listView2.Items.Clear();

            }
            else
            {
                // Grup oluşturma başarısızsa hata mesajı göster
                MessageBox.Show("Grup oluşturulurken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private int GrupOlustur(string grupAdi)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
                {
                    connection.Open();

                    // Grup oluşturma sorgusu
                    string query = "INSERT INTO Grup (GrupAdi, KullaniciID) VALUES (@GrupAdi, @KullaniciID); SELECT SCOPE_IDENTITY();";

                    // Komut ve bağlantıyı ilişkilendir
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parametreleri ekle
                        command.Parameters.AddWithValue("@GrupAdi", grupAdi);
                        command.Parameters.AddWithValue("@KullaniciID", KullaniciAdiniIDyeCevir(label1.Text));

                        // Oluşturulan grup ID'sini döndür
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Grup oluştururken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        private void GrubaUyeleriEkle(int grupID, List<string> uyeler)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(BaglantiAdresi))
                {
                    connection.Open();

                    // Gruba üye ekleme sorgusu
                    string query = "INSERT INTO grup_uye (uye_id, grup_id) VALUES (@uye_id, @grup_id)";

                    // Seçili her üye için sorguyu çalıştır
                    foreach (string kullaniciAdi in uyeler)
                    {
                        int kullaniciID = KullaniciAdiniIDyeCevir(kullaniciAdi);

                        if (kullaniciID > 0)
                        {
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                // Parametreleri ekle
                                command.Parameters.AddWithValue("@uye_id", kullaniciID);
                                command.Parameters.AddWithValue("@grup_id", grupID);

                                // Sorguyu çalıştır
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Üyeleri gruba eklerken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {

            // Bildirim sayısını 1 arttır
            bildirimsayi++;

            // Bildirim sayısını güncelle
           // bildirim_Geldi();


        }

        private void bildirim_Geldi(object sender, EventArgs e)
        {

            // Bildirim sayısını güncelle
            bildirimsayi = bildirimsayi + 1;

            // Bildirim sayısını kontrol et
            if (bildirimsayi != 1)
            {
                // Bildirim butonunun metnini kontrol et
                if (button9.Text != "1")
                {
                    throw new Exception("Bildirim butonunun metni güncellenmedi.");
                }
            }

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        bool move;
        int mouse_x;
        int mouse_y;


        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            move = true;
            mouse_x = e.X;
            mouse_y = e.Y;

        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {

            move = false;
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (move)
            {
                this.SetDesktopLocation(MousePosition.X - mouse_x, MousePosition.Y - mouse_y);
            }
        }
    }
}