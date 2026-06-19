# Wordix Development Notes

## 1. Amaç

Bu dosya, Wordix backend projesinin geliştirme sürecinde günlük notları, öğrenilen kavramları, karşılaşılan sorunları ve yapılan teknik kararları kaydetmek için oluşturulmuştur.

Bu dosya aynı zamanda staj defteri yazarken kaynak olarak kullanılacaktır.

Backend projesi büyüdükçe hangi gün ne yaptığımızı, hangi dosyaları oluşturduğumuzu, hangi kavramları öğrendiğimizi ve hangi endpointleri test ettiğimizi düzenli şekilde takip etmek önemlidir.

---

## 2. Neden Development Notes Tutuyoruz?

Wordix kapsamlı bir backend projesidir.

Projede ilerledikçe şu konular geliştirilecektir:

* Clean Architecture
* SOLID prensipleri
* Keycloak authentication
* JWT Bearer authorization
* Entity Framework Core
* MSSQL
* MediatR
* CQRS
* Repository Pattern
* Unit of Work
* FluentValidation
* ExceptionMiddleware
* Swagger/Postman testleri
* Dictionary ve quiz akışları
* Learning progress güncellemeleri

Bu kadar çok kavram bir araya geldiği için günlük not tutmak hem teknik öğrenmeyi hem de staj defteri yazımını kolaylaştırır.

---

## 3. Günlük Not Formatı

Her geliştirme günü için aşağıdaki format kullanılacaktır:

```text
Tarih:

Bugün yapılanlar:

Karşılaşılan sorunlar:

Öğrenilen kavramlar:

Yazılan dosyalar:

Test edilen endpointler:

Bir sonraki adım:
```

---

## 4. Alanların Açıklaması

## 4.1 Tarih

Çalışmanın yapıldığı gün yazılır.

Örnek:

```text
Tarih: 2026-06-19
```

---

## 4.2 Bugün yapılanlar

O gün projede yapılan işler kısa maddeler halinde yazılır.

Örnek:

```text
Bugün yapılanlar:
- Wordix proje kapsamı netleştirildi.
- Clean Architecture katmanları planlandı.
- docs klasörü oluşturuldu.
- PROJECT_SCOPE.md dosyası dolduruldu.
```

Bu alan staj defteri için en önemli bölümlerden biridir.

---

## 4.3 Karşılaşılan sorunlar

O gün karşılaşılan hata, kararsızlık veya teknik problem yazılır.

Örnek:

```text
Karşılaşılan sorunlar:
- Visual Studio’da proje oluşturma ekranında hangi template’in seçileceği karıştırıldı.
- Şu an solution oluşturulmadığı için Console App veya Blazor template’i seçilmemesi gerektiği öğrenildi.
```

Eğer sorun yaşanmadıysa şöyle yazılabilir:

```text
Karşılaşılan sorunlar:
- Bugün teknik bir sorunla karşılaşılmadı.
```

---

## 4.4 Öğrenilen kavramlar

O gün öğrenilen teknik kavramlar yazılır.

Örnek:

```text
Öğrenilen kavramlar:
- Clean Architecture’da Domain katmanının dış teknolojilere bağımlı olmaması gerektiği öğrenildi.
- LearningItem ortak çatı yapısının Word, Phrase ve Sentence desteği için neden önemli olduğu anlaşıldı.
- Controller içinde iş kuralı yazılmaması gerektiği öğrenildi.
```

---

## 4.5 Yazılan dosyalar

O gün oluşturulan veya güncellenen dosyalar yazılır.

Örnek:

```text
Yazılan dosyalar:
- docs/PROJECT_SCOPE.md
- docs/ARCHITECTURE.md
- docs/API_PLAN.md
- docs/ENTITY_PLAN.md
- docs/DEVELOPMENT_NOTES.md
```

Bu alan ileride commit hazırlarken de yardımcı olur.

---

## 4.6 Test edilen endpointler

O gün test edilen endpointler yazılır.

Henüz endpoint yoksa şöyle yazılır:

```text
Test edilen endpointler:
- Henüz endpoint yazılmadığı için test yapılmadı.
```

İleride örnek:

```text
Test edilen endpointler:
- GET /api/profile/me
- POST /api/lookups
- POST /api/user-dictionary
```

---

## 4.7 Bir sonraki adım

Bir sonraki geliştirme adımı yazılır.

Örnek:

```text
Bir sonraki adım:
- Faz 1 kapsamında Wordix.sln solution dosyası oluşturulacak.
```

Bu alan proje yönetimi açısından önemlidir. Çünkü bir sonraki gün projeye döndüğümüzde nereden devam edeceğimizi hızlıca anlarız.

---

## 5. İlk Gün Notu

```text
Tarih: 2026-06-19

Bugün yapılanlar:
- Wordix backend projesinin genel kapsamı netleştirildi.
- İlk MVP/prototip kapsamı yazılı hale getirildi.
- Teknik kararlar belirlendi.
- Backend-first ilerleme kararı netleştirildi.
- İlk prototipin gerçek API, gerçek Keycloak authentication, gerçek JWT authorization ve gerçek MSSQL database ile geliştirileceği kararlaştırıldı.
- Wordix’in sadece Word üzerine değil, LearningItem ortak çatısı üzerine kurulacağı netleştirildi.
- Phrase ve Sentence desteğinin ilk prototipten sonra ekleneceği ama mimarinin baştan buna hazır olacağı belirlendi.
- docs klasörü oluşturuldu.
- Başlangıç dokümantasyon dosyaları oluşturuldu ve dolduruldu.

Karşılaşılan sorunlar:
- Visual Studio’da yeni proje oluşturma ekranında Console App, Blazor ve Aspire seçeneklerinin göründüğü fark edildi.
- Bu aşamada herhangi bir .NET projesi oluşturulmayacağı, sadece klasör açılıp dokümantasyon dosyalarının hazırlanacağı netleştirildi.
- Faz 1’e geçildiğinde ilk seçilecek template’in Console App değil, Blank Solution / Boş Çözüm olacağı öğrenildi.

Öğrenilen kavramlar:
- MVP kapsamı ile future-ready kapsam arasındaki fark öğrenildi.
- Clean Architecture’da katmanların sorumluluklarının ayrılması gerektiği öğrenildi.
- Domain katmanının EF Core, Keycloak, Swagger, HTTP veya MSSQL gibi teknolojilere bağımlı olmaması gerektiği öğrenildi.
- Controller içinde iş kuralı yazılmaması gerektiği öğrenildi.
- LearningItem ortak çatısının Open/Closed Principle için neden önemli olduğu anlaşıldı.
- Dictionary sisteminin teknik olarak UserLearningItem üzerinden besleneceği öğrenildi.
- Backend-first geliştirme yaklaşımının API sözleşmelerini netleştirmek için önemli olduğu anlaşıldı.

Yazılan dosyalar:
- docs/PROJECT_SCOPE.md
- docs/ARCHITECTURE.md
- docs/API_PLAN.md
- docs/ENTITY_PLAN.md
- docs/DEVELOPMENT_NOTES.md

Test edilen endpointler:
- Henüz endpoint yazılmadığı için test yapılmadı.

Bir sonraki adım:
- Faz 1 kapsamında Clean Architecture solution yapısı oluşturulacak.
- Wordix.sln oluşturulacak.
- src ve tests klasörleri hazırlanacak.
- Wordix.Api, Wordix.Application, Wordix.Domain, Wordix.Persistence, Wordix.Infrastructure ve Wordix.Shared projeleri oluşturulacak.
```

---

## 6. Commit Notları

Commit mesaj standardı aşağıdaki gibi kullanılacaktır:

```text
feat: yeni özellik
fix: hata düzeltme
chore: yapılandırma
docs: dokümantasyon
test: test ekleme
refactor: kod düzenleme
```

Faz 0 tamamlandığında önerilen commit mesajı:

```text
docs: add initial project documentation
```

---

## 7. Faz Sonu Kontrol Formatı

Her faz sonunda aşağıdaki kontrol yapılacaktır:

```text
Faz:

Tamamlanan işler:

Build durumu:

Test durumu:

Commit mesajı:

Notlar:
```

Örnek:

```text
Faz: Faz 0 - Proje Başlangıç Hazırlığı

Tamamlanan işler:
- Proje kapsamı yazıldı.
- Teknik kararlar sabitlendi.
- docs klasörü oluşturuldu.
- Başlangıç dokümantasyon dosyaları dolduruldu.
- Staj defteri not formatı belirlendi.

Build durumu:
- Henüz .NET solution oluşturulmadığı için build alınmadı.

Test durumu:
- Henüz endpoint yazılmadığı için Swagger/Postman testi yapılmadı.

Commit mesajı:
- docs: add initial project documentation

Notlar:
- Faz 1’de Clean Architecture solution kurulumu yapılacak.
```

---

## 8. Geliştirme Prensipleri

Wordix geliştirilirken şu prensipler takip edilecektir:

* Her faz küçük adımlara bölünecek.
* Bir faz tamamlanmadan diğerine geçilmeyecek.
* Her önemli dosyada öğretici açıklamalar olacak.
* Controller içinde iş kuralı yazılmayacak.
* Domain katmanı dış teknolojilere bağımlı olmayacak.
* Application katmanı use-case akışlarını yönetecek.
* Persistence katmanı database işlemlerini yönetecek.
* Infrastructure katmanı dış servis implementasyonlarını yönetecek.
* Api katmanı HTTP request/response yönetimini yapacak.
* Yeni özellikler eklenirken mevcut yapı bozulmayacak.
* Her faz sonunda build/test/commit adımı kontrol edilecek.
