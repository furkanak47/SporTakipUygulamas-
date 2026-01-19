# 🏋️ SporTakip - Web Tabanlı Spor ve Beslenme Takip Uygulaması

Bu proje, **Web Tabanlı Programlama** dersi final ödevi kapsamında geliştirilmiş, kullanıcıların günlük beslenme ve antrenman aktivitelerini takip edebilecekleri, gelişimlerini grafiklerle izleyebilecekleri kapsamlı bir web uygulamasıdır.

🔗 **Proje Tanıtım Videosu:** [YouTube - Web Tabanli Programlama Spor Takip Uygulaması](https://www.youtube.com/watch?v=8Nc7UqCqhVo)


## 📋 İçindekiler
- [Proje Hakkında](#proje-hakkında)
- [Özellikler](#özellikler)
- [Teknolojiler](#teknolojiler)
- [Veritabanı Tasarımı](#veritabanı-tasarımı)
- [Kurulum ve Çalıştırma](#kurulum-ve-çalıştırma)

## ℹ️ Proje Hakkında
**SporTakip**, kullanıcıların sağlıklı yaşam hedeflerine ulaşmalarına yardımcı olmak için tasarlanmıştır. Kullanıcılar kişisel bilgilerine (yaş, boy, kilo, cinsiyet) göre otomatik hesaplanan günlük kalori ve makro besin hedeflerini takip edebilirler.

## ✨ Özellikler

### 🔐 Kimlik Doğrulama ve Yetkilendirme
- **Kullanıcı Kaydı & Girişi:** Güvenli üyelik sistemi (ASP.NET Core Identity).
- **Profil Yönetimi:** Kişisel bilgileri ve hedefleri güncelleme.

### 🍽️ Beslenme Modülü (`MealsController`)
- **Yemek Veritabanı:** Önceden tanımlı yemekler arasından seçim yapma.
- **Akıllı Hesaplama:** Kullanıcının fiziksel özelliklerine göre (BMR) günlük kalori, protein, yağ ve karbonhidrat ihtiyacının otomatik hesaplanması.
- **Günlük Takip:** Öğünleri kaydetme ve günlük limitlere göre kalan miktarı görme.
- **Validasyon:** Hatalı veya aşırı miktar girişlerinin engellenmesi (Örn: 5kg üzeri yemek girişi engeli).

### 💪 Antrenman Modülü (`WorkoutsController`)
- **Egzersiz Kütüphanesi:** Çeşitli egzersiz türleri (Kardiyo, Ağırlık).
- **Aktivite Kaydı:** Yapılan egzersizlerin süre veya tekrar bazlı kaydedilmesi.
- **Kalori Yakımı:** Yapılan spora göre yakılan kalorinin hesaplanması ve günlük net kaloriye yansıtılması.

### 📊 Raporlama ve Analiz
- **Dashboard:** Günlük özet tablosu.
- **Görsel Grafikler:** Chart.js kullanılarak oluşturulan dinamik grafikler (Haftalık kalori değişimi vb.).

## 🛠️ Teknolojiler

Bu proje aşağıdaki modern web teknolojileri kullanılarak geliştirilmiştir:

| Katman | Teknoloji | Notlar |
|--------|-----------|--------|
| **Backend** | ASP.NET Core MVC 9.0 | En güncel .NET sürümü |
| **Language** | C# 12 | |
| **Database** | SQLite | Entity Framework Core (Code-First) ile |
| **Frontend** | Bootstrap 5, Razor Views | Responsive tasarım |
| **Scripts** | JavaScript, jQuery, Chart.js | Dinamik interaksiyonlar ve grafikler |

## 🗄️ Veritabanı Tasarımı
Proje **Entity Framework Core** kullanılarak Code-First yaklaşımıyla geliştirilmiştir. Ana tablolar:
- `AspNetUsers`: Kullanıcı ve profil bilgileri.
- `Foods`: Besin değerleri ile birlikte yemek listesi.
- `MealLogs`: Kullanıcıların öğün kayıtları.
- `Exercises`: Tanımlı egzersizler.
- `WorkoutLogs`: Antrenman kayıtları.

## 🚀 Kurulum ve Çalıştırma

Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyin:

1.  **Gereksinimler:**
    - .NET 9.0 SDK
    - Visual Studio 2022 veya VS Code kodu

2.  **Projeyi Klonlayın:**
    ```bash
    git clone https://github.com/kullaniciadi/SporTakip.git
    cd SporTakip
    ```

3.  **Veritabanını Hazırlayın:**
    ```bash
    dotnet ef database update
    ```

4.  **Uygulamayı Başlatın:**
    ```bash
    dotnet run
    ```
    Tarayıcınızda `http://localhost:5000` (veya terminalde belirtilen port) adresine gidin.

---
**Geliştirici:** Furkan Akdemir
**Ders:** Web Tabanlı Programlama
**Tarih:** Ocak 2026
