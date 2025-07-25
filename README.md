# ERP Projesi Kurulum Rehberi

Bu rehber, ERP projesini yerel geliştirme ortamınızda kurmak ve çalıştırmak için gerekli adımları içermektedir. Proje, bir .NET 9 Backend API ve bir Vue.js Frontend uygulamasından oluşmaktadır.

## 1. Ön Gereksinimler

Projeyi çalıştırmadan önce sisteminizde aşağıdaki araçların kurulu olduğundan emin olun:

- **.NET 9 SDK:** Backend'i derlemek ve çalıştırmak için gereklidir.
- **Node.js (LTS sürümü önerilir):** Frontend uygulamasının bağımlılıklarını yönetmek ve çalıştırmak için gereklidir.
- **SQL Server (Express veya Developer sürümü):** Veritabanı için gereklidir. Proje, `appsettings.json` dosyasında belirtilen bağlantı dizesini kullanarak bir veritabanına erişmeye çalışacaktır.

## 2. Kurulum Adımları

### Backend Kurulumu (.NET)

1.  **Bağımlılıkları Yükleyin:**
    Projenin kök dizinindeyken aşağıdaki komutu çalıştırarak .NET bağımlılıklarını (NuGet paketleri) yükleyin:

    ```bash
    dotnet restore src/Backend/ERP.sln
    ```

2.  **Veritabanı Yapılandırması:**
    Backend projesinin veritabanı bağlantı bilgilerini yapılandırmanız gerekmektedir.

    -   `src/Backend/API.Web/` dizininde bulunan `appsettings.Development.json.example` dosyasını kopyalayın ve kopyanın adını `appsettings.Development.json` olarak değiştirin.
    -   Oluşturduğunuz `appsettings.Development.json` dosyasını açın ve içerisindeki `ConnectionStrings` bölümünü kendi SQL Server yapılandırmanıza göre düzenleyin.

        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ERP_DB;Trusted_Connection=True;TrustServerCertificate=True;"
        }
        ```

3.  **Veritabanını Oluşturun ve Güncelleyin:**
    Entity Framework Core migration'larını uygulayarak veritabanını oluşturun ve şemayı en son sürüme güncelleyin.

    ```bash
    dotnet ef database update --project src/Backend/Infrastructure --startup-project src/Backend/API.Web
    ```

4.  **Backend API'sini Çalıştırın:**
    Aşağıdaki komut ile API'yi başlatın:

    ```bash
    dotnet run --project src/Backend/API.Web/API.Web.csproj
    ```

    API, varsayılan olarak **`https://localhost:7277`** ve **`http://localhost:5245`** adreslerinde çalışmaya başlayacaktır.

### Frontend Kurulumu (Vue.js)

1.  **Frontend Dizinine Geçin:**
    Yeni bir terminal açın ve frontend projesinin dizinine gidin:

    ```bash
    cd src/Frontend/client-app
    ```

2.  **Bağımlılıkları Yükleyin:**
    `package.json` dosyasında listelenen Node.js bağımlılıklarını yüklemek için aşağıdaki komutu çalıştırın:

    ```bash
    npm install
    ```

3.  **Frontend Uygulamasını Çalıştırın:**
    Vite geliştirme sunucusunu başlatın:

    ```bash
    npm run dev
    ```

    Uygulama, genellikle **`http://localhost:5173`** adresinde çalışmaya başlayacak ve tarayıcınızda otomatik olarak açılacaktır.

## 3. Projeyi Kullanma

-   Backend API'si `https://localhost:7277` adresinde çalışır durumda olacaktır.
-   Frontend uygulamasına `http://localhost:5173` adresinden erişebilirsiniz. Frontend, API isteklerini bu adrese yapacak şekilde yapılandırılmıştır (gerekirse `src/Frontend/client-app/src/services` altındaki servis dosyalarını kontrol edin).

