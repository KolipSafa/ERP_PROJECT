using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class Customer
    {
        // Neden Guid?
        // Standart bir 'int' Id kullansaydık, Id'ler 1, 2, 3 gibi sıralı ve tahmin edilebilir olurdu.
        // Bu, güvenlik açısından bir zafiyet oluşturabilir (örn: /api/customers/4 adresini deneyerek başkasının bilgisine erişmeye çalışmak).
        // Guid ise (örn: "a1b2c3d4-e5f6-7890-1234-567890abcdef") evrensel olarak benzersiz, tahmin edilemez bir kimliktir.
        // Dağıtık sistemlerde veya birden fazla veritabanının birleştiği senaryolarda çakışma riskini sıfırlar.
        public Guid Id { get; set; }

        // Neden "= string.Empty;"?
        // Bu, "null-forgiving" operatörü kullanmak yerine, bu özelliğin hiçbir zaman 'null' olmayacağını garanti eder.
        // Bir Customer nesnesi oluşturulduğunda, FirstName ve LastName özellikleri bellekte 'null' olarak değil,
        // boş bir string "" olarak başlar. Bu, kodun ilerleyen kısımlarında beklenmedik 'NullReferenceException'
        // hatalarını önleyen proaktif bir savunma mekanizmasıdır.
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Neden "string?" (Nullable Reference Type)?
        // Her müşterinin bir şirket adı veya vergi numarası olmak zorunda değil.
        // '?' işareti, C# derleyicisine "Bu alanın null olması normal ve beklenen bir durumdur." der.
        // Bu sayede, bu özelliği kullanmaya çalıştığımız her yerde derleyici bizi "Dikkat et, bu null olabilir!"
        // diye uyarır ve gerekli null kontrollerini yapmaya teşvik eder. Bu da kodun güvenliğini artırır.
        
        // Müşterinin bağlı olduğu şirketin ID'si. Bu alan zorunludur.
        public Guid CompanyId { get; set; }
        
        // Entity Framework'ün CompanyId üzerinden ilgili Company nesnesini otomatik olarak
        // yüklemesini sağlayan "navigation property".
        // [ForeignKey] attribute'u ile bu ilişkinin hangi property üzerinden kurulduğunu netleştiriyoruz.
        public Company? Company { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        // Neden "decimal"?
        // Finansal hesaplamalarda 'float' veya 'double' kullanmak çok tehlikelidir. Çünkü bu tipler ikilik tabanda
        // ondalıklı sayıları temsil ederken küçük hassasiyet kayıpları yaşayabilirler (örn: 0.1 + 0.2 = 0.30000000000000004).
        // 'decimal' ise onluk tabanda çalışır ve para gibi mutlak hassasiyet gerektiren durumlar için tasarlanmıştır.
        // Kuruşların bile doğru hesaplandığından emin oluruz.
        public decimal Balance { get; set; }

        // Neden "= true"?
        // Yeni oluşturulan bir müşterinin varsayılan olarak "aktif" olmasını bekleriz.
        // Bu başlangıç değerini burada vererek, bu mantığı kodu yazan kişinin hatırlamasına gerek bırakmayız.
        // Nesne yaratıldığı anda doğru durumuyla başlar.
        public bool IsActive { get; set; } = true;

        // Bu müşteri için bir kullanıcı hesabı oluşturulduysa, o hesabın ID'sini burada tutacağız.
        // Henüz davet edilmemiş veya hesabını aktive etmemiş müşteriler için bu alan NULL olacaktır.
        // Bu, Infrastructure katmanındaki ApplicationUser'a bir referanstır.
        // Normalde Domain katmanı Infrastructure'a bağımlı olmaz, ancak bu property
        // bir veritabanı ilişkisi için gereklidir ve EF Core tarafından yönetilecektir.
        // Asıl entity referansı (navigation property) bu dosyada yer almayacak.
        public Guid? ApplicationUserId { get; set; }

        // Neden "DateTime" ve "DateTime?"?
        // Her kaydın ne zaman oluşturulduğunu bilmek önemlidir. Bu yüzden 'CreatedDate' null olamaz.
        // Ancak 'UpdatedDate', kayıt ilk oluşturulduğunda 'null' olacaktır. Sadece bir güncelleme işlemi
        // yapıldığında bir değere sahip olur. '?' işareti bu "henüz güncellenmedi" durumunu mükemmel bir şekilde temsil eder.
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}