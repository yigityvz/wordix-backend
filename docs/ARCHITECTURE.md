# Wordix Architecture

## 1. Mimari Yaklaşım

Wordix backend projesi Clean Architecture yaklaşımıyla geliştirilecektir.

Clean Architecture kullanmamızın temel amacı, iş kurallarını dış teknolojilerden korumaktır.

Bu projede backend tarafında birçok dış teknoloji kullanılacaktır:

* ASP.NET Core
* Entity Framework Core
* MSSQL
* Keycloak
* JWT Bearer Authentication
* Swagger
* Docker
* Provider servisleri

Ancak bu teknolojiler projenin merkezindeki iş kurallarına doğrudan karışmamalıdır.

Örneğin bir `LearningItem` entity’si, kendisinin MSSQL’de hangi tabloya yazıldığını bilmemelidir.

Aynı şekilde bir `QuizAnswer` entity’si, cevabın HTTP request üzerinden geldiğini bilmemelidir.

Bu yüzden proje katmanlara ayrılacaktır.

---

## 2. Solution Yapısı

Planlanan solution yapısı aşağıdaki gibidir:

```text
Wordix.sln

src/
  Wordix.Api
  Wordix.Application
  Wordix.Domain
  Wordix.Persistence
  Wordix.Infrastructure
  Wordix.Shared

tests/
  Wordix.UnitTests
  Wordix.IntegrationTests
```

Bu yapı sayesinde her proje kendi sorumluluğuna sahip olur.

Örneğin:

* Entity classları `Wordix.Domain` içinde durur.
* Command ve Query handlerları `Wordix.Application` içinde durur.
* DbContext ve EF Core configuration dosyaları `Wordix.Persistence` içinde durur.
* Keycloak claim okuma servisi `Wordix.Infrastructure` içinde durur.
* Controller ve middleware dosyaları `Wordix.Api` içinde durur.
* Ortak response modelleri `Wordix.Shared` içinde durur.

---

## 3. Katmanların Görevleri

## 3.1 Wordix.Domain

`Wordix.Domain` projenin en merkezindeki katmandır.

Bu katman iş modelini ve temel domain kurallarını tutar.

Bu katmanda bulunacak yapılar:

* Entities
* Enums
* ValueObjects
* Domain Exceptions
* Business Rules
* BaseEntity
* AuditableEntity
* SoftDeleteEntity

Örnek entityler:

* UserProfile
* Language
* LearningItem
* Word
* Meaning
* LookupHistory
* UserLearningItem
* UserLearningProgress
* QuizSession
* QuizQuestion
* QuizOption
* QuizAnswer

Domain katmanı hiçbir dış teknolojiye bağımlı olmayacaktır.

Yani Domain katmanı şunları bilmeyecek:

* EF Core
* MSSQL
* Keycloak
* JWT
* Swagger
* HTTP
* Controller
* DbContext

Bu karar önemlidir çünkü Domain katmanı projenin en saf iş modelidir.

Örneğin `LearningItem` ileride Word, Phrase veya Sentence olabilir. Bu bilginin kendisi bir iş modelidir. Bu yüzden Domain içinde durmalıdır.

---

## 3.2 Wordix.Application

`Wordix.Application` use-case katmanıdır.

Use-case, kullanıcının sistemde yapmak istediği anlamlı işlem demektir.

Örnek use-case’ler:

* Kullanıcı profilini getir.
* Kelime lookup yap.
* Kelimeyi dictionary’ye kaydet.
* Kullanıcının dictionary listesini getir.
* Quiz başlat.
* Quiz cevabı gönder.
* Quiz özetini getir.

Bu katmanda bulunacak yapılar:

* Commands
* Queries
* Handlers
* DTOs
* Validators
* Interfaces
* Mapping ayarları
* Pipeline behaviors

Bu projede MediatR ve CQRS yaklaşımı Application katmanında kullanılacaktır.

Command, sistemde değişiklik yapan işlemdir.

Örnek:

```text
CreateLookupCommand
SaveLearningItemCommand
StartQuizCommand
SubmitQuizAnswerCommand
```

Query, sadece veri okuyan işlemdir.

Örnek:

```text
GetCurrentUserProfileQuery
GetMyDictionaryQuery
GetQuizSummaryQuery
```

Application katmanı Domain katmanını kullanabilir.

Application ayrıca Shared katmanındaki ortak response veya result modellerini kullanabilir.

Ancak Application katmanı doğrudan EF Core, MSSQL veya Keycloak implementasyonuna bağımlı olmamalıdır.

Bunun yerine interface tanımlar.

Örnek:

```text
ICurrentUserService
IUserProfileSyncService
IUnitOfWork
IDictionaryProvider
ITextNormalizer
IQuizQuestionGenerator
```

Bu interface’lerin gerçek implementasyonları Persistence veya Infrastructure katmanında yapılır.

Bu yaklaşım Dependency Inversion Principle ile uyumludur.

---

## 3.3 Wordix.Persistence

`Wordix.Persistence` database işlemlerinden sorumlu katmandır.

Bu katmanda bulunacak yapılar:

* WordixDbContext
* Entity configurations
* Repositories
* UnitOfWork
* Migrations
* Seed data

Entity Framework Core bu katmanda kullanılacaktır.

MSSQL bağlantısı bu katmandaki DbContext üzerinden yönetilecektir.

Örnek dosyalar:

```text
Wordix.Persistence/
  Context/
    WordixDbContext.cs

  Configurations/
    UserProfileConfiguration.cs
    LearningItemConfiguration.cs
    WordConfiguration.cs
    MeaningConfiguration.cs
    QuizSessionConfiguration.cs

  Repositories/
    Repository.cs
    LearningItemRepository.cs
    UserLearningItemRepository.cs

  UnitOfWork/
    UnitOfWork.cs
```

Persistence katmanı Domain entitylerini bilir çünkü onları database tablolarına map eder.

Persistence katmanı Application interface’lerini de bilir çünkü Application içinde tanımlanan repository ve unit of work sözleşmelerinin gerçek implementasyonunu burada yapar.

Örnek:

Application içinde:

```text
IUnitOfWork
```

Persistence içinde:

```text
UnitOfWork
```

Bu sayede Application, database teknolojisini bilmez. Sadece interface üzerinden konuşur.

---

## 3.4 Wordix.Infrastructure

`Wordix.Infrastructure` dış servisler ve teknik implementasyonlar için kullanılacaktır.

Bu katmanda bulunacak yapılar:

* Keycloak claim okuma servisleri
* CurrentUserService implementasyonu
* Dictionary providers
* Translation providers
* Import services
* DateTime service
* Cache servisleri
* Provider log servisleri

Örnek dosyalar:

```text
Wordix.Infrastructure/
  Identity/
    KeycloakCurrentUserService.cs

  Providers/
    PrototypeDictionaryProvider.cs

  Time/
    DateTimeService.cs
```

Neden KeycloakCurrentUserService Infrastructure içinde durur?

Çünkü Keycloak dış bir authentication provider’dır.

Application katmanı sadece şunu bilmek ister:

```text
Bana mevcut kullanıcı bilgisini ver.
```

Ama bu bilginin JWT claimlerinden mi, Keycloak’tan mı, başka bir identity provider’dan mı geldiğini bilmek istemez.

Bu yüzden Application içinde `ICurrentUserService` interface’i olur.

Infrastructure içinde ise `KeycloakCurrentUserService` bu interface’i implemente eder.

Bu yaklaşım Open/Closed Principle için önemlidir.

Yarın Keycloak yerine başka bir identity provider gelirse Application katmanını baştan yazmak zorunda kalmayız.

---

## 3.5 Wordix.Api

`Wordix.Api` dış dünyaya açılan HTTP katmanıdır.

Bu katmanda bulunacak yapılar:

* Controllers
* Middlewares
* Authentication setup
* Authorization setup
* Swagger/OpenAPI setup
* Dependency Injection registration
* CORS setup
* Health endpoint
* Program.cs

Controller içinde iş kuralı yazılmayacaktır.

Controller’ın görevi:

1. HTTP request almak.
2. Request modelini Command veya Query nesnesine çevirmek.
3. MediatR’a göndermek.
4. Gelen response’u HTTP response olarak dönmek.

Yanlış yaklaşım:

```text
Controller içinde database sorgusu yapmak
Controller içinde quiz cevabını hesaplamak
Controller içinde UserLearningProgress güncellemek
Controller içinde provider çağırmak
```

Doğru yaklaşım:

```text
Controller request alır.
Command veya Query oluşturur.
MediatR'a gönderir.
Application katmanı işi yapar.
Controller sadece sonucu döner.
```

Bu karar Single Responsibility Principle ile uyumludur.

Çünkü Controller’ın tek sorumluluğu HTTP iletişimini yönetmektir.

---

## 3.6 Wordix.Shared

`Wordix.Shared` ortak kullanılan yardımcı modelleri tutacaktır.

Bu katmanda bulunabilecek yapılar:

* ApiResponse
* ErrorResponse
* ValidationError
* PagedResult
* Result Pattern
* Constants

Örnek:

```text
ApiResponse<T>
ErrorResponse
ValidationError
```

Bu katmanın amacı, farklı katmanlarda tekrar tekrar aynı response veya helper modellerini yazmamaktır.

Örneğin tüm API endpointleri standart bir response formatı kullanacaksa bu model Shared katmanında durabilir.

---

## 4. Project Reference Kuralları

Katmanlar arasındaki bağımlılık yönü aşağıdaki gibi olacaktır:

```text
Wordix.Api -> Wordix.Application
Wordix.Api -> Wordix.Infrastructure
Wordix.Api -> Wordix.Persistence

Wordix.Application -> Wordix.Domain
Wordix.Application -> Wordix.Shared

Wordix.Persistence -> Wordix.Domain
Wordix.Persistence -> Wordix.Application

Wordix.Infrastructure -> Wordix.Application
Wordix.Infrastructure -> Wordix.Domain
```

En önemli kural:

```text
Wordix.Domain hiçbir katmana bağımlı olmayacaktır.
```

Bu kural Clean Architecture’ın temelidir.

Domain en merkezde durur.

Dış katmanlar Domain’i kullanabilir ama Domain dış katmanları bilmez.

---

## 5. Neden Domain Bağımsız Kalmalı?

Domain katmanı iş kurallarını temsil eder.

Örneğin:

* Bir LearningItem Word, Phrase veya Sentence olabilir.
* Bir kullanıcı bir LearningItem’ı dictionary’sine kaydedebilir.
* Bir QuizAnswer doğru veya yanlış olabilir.
* Bir UserLearningProgress kullanıcının öğrenme durumunu takip eder.

Bunlar framework bağımsız iş kurallarıdır.

Bu kuralların EF Core, MSSQL veya HTTP’ye bağımlı olması doğru değildir.

Eğer Domain katmanı dış teknolojilere bağımlı olursa, proje ileride değişikliklere karşı kırılgan hale gelir.

Örneğin yarın MSSQL yerine PostgreSQL kullanmak istersek Domain etkilenmemelidir.

Veya Keycloak yerine başka bir authentication sistemi gelirse Domain değişmemelidir.

---

## 6. Controller Kuralı

Controller içinde iş kuralı yazılmayacaktır.

Örnek kötü controller yaklaşımı:

```text
LookupsController:
- Kullanıcıyı token’dan oku
- DbContext ile UserProfile sorgula
- Kelime normalize et
- Word var mı bak
- Yoksa provider çağır
- LearningItem oluştur
- LookupHistory oluştur
- SaveChanges çağır
```

Bu yaklaşım kötüdür çünkü Controller çok fazla sorumluluk alır.

Doğru yaklaşım:

```text
LookupsController:
- Request alır
- CreateLookupCommand oluşturur
- MediatR'a gönderir
- Response döner
```

Asıl iş Application katmanındaki handler içinde yapılır.

Örnek:

```text
CreateLookupCommandHandler
```

Bu sayede Controller sade kalır, test edilebilirlik artar ve iş kuralları doğru katmanda toplanır.

---

## 7. MediatR ve CQRS Kullanım Kararı

Wordix projesinde MediatR kullanılacaktır.

MediatR sayesinde Controller doğrudan servisleri çağırmak yerine command/query gönderir.

CQRS yaklaşımıyla okuma ve yazma işlemleri ayrılır.

Command örnekleri:

```text
CreateLookupCommand
SaveLearningItemCommand
StartQuizCommand
SubmitQuizAnswerCommand
```

Query örnekleri:

```text
GetCurrentUserProfileQuery
GetMyDictionaryQuery
GetUserDictionaryItemDetailQuery
GetQuizSummaryQuery
```

Bu yapı projenin büyümesini daha kontrollü hale getirir.

Yeni bir use-case geldiğinde yeni bir command veya query eklenir.

Mevcut kodu bozmak yerine sisteme yeni parça eklenmiş olur.

Bu da Open/Closed Principle ile uyumludur.

---

## 8. Repository Pattern ve Unit of Work Kararı

Database işlemlerinde Repository Pattern ve Unit of Work kullanılacaktır.

Repository Pattern, veri erişim kodlarını soyutlamak için kullanılır.

Unit of Work ise bir işlem akışındaki değişiklikleri tek noktadan kaydetmek için kullanılır.

Örnek lookup akışı:

1. LearningItem oluşturulur.
2. Word oluşturulur.
3. Meaning oluşturulur.
4. LookupHistory oluşturulur.
5. Tüm değişiklikler tek SaveChanges ile kaydedilir.

Bu işlemleri kontrol etmek için Unit of Work kullanılır.

Bu sayede transaction yönetimi ve veri tutarlılığı daha kolay yönetilir.

---

## 9. Authentication ve Authorization Mimari Kararı

Authentication Keycloak tarafından yönetilecektir.

Backend tarafında JWT Bearer Authentication kullanılacaktır.

Backend token içindeki claim bilgilerini okuyacaktır.

Okunacak temel bilgiler:

* KeycloakUserId
* Email
* Username
* Roles
* IsAuthenticated

Application katmanı mevcut kullanıcı bilgisine ihtiyaç duyduğunda doğrudan HTTP context okumayacaktır.

Bunun yerine şu interface kullanılacaktır:

```text
ICurrentUserService
```

Bu interface Application katmanında tanımlanacaktır.

Gerçek implementasyon ise Infrastructure katmanında olacaktır:

```text
KeycloakCurrentUserService
```

Bu karar Application katmanını ASP.NET Core HttpContext detaylarından korur.

---

## 10. LearningItem Mimari Kararı

Wordix sadece Word entity’sine bağlı tasarlanmayacaktır.

Sistemin merkezinde LearningItem bulunacaktır.

LearningItem şu içerikler için ortak çatı olacaktır:

* Word
* Phrase
* Sentence

İlk prototipte sadece Word aktif kullanılacaktır.

Ancak dictionary, quiz ve progress sistemleri LearningItem üzerinden tasarlanacaktır.

Doğru yaklaşım:

```text
UserLearningItem -> LearningItem
QuizQuestion -> LearningItem
Meaning -> LearningItem
LookupHistory -> LearningItem
```

Yanlış yaklaşım:

```text
UserLearningItem -> Word
QuizQuestion -> Word
Meaning -> Word
LookupHistory -> Word
```

Yanlış yaklaşım sistemi sadece kelimeyle sınırlar.

Doğru yaklaşım ise Phrase ve Sentence desteğine hazır hale getirir.

Bu karar Open/Closed Principle açısından kritik öneme sahiptir.

---

## 11. SOLID Prensipleri

Wordix geliştirilirken SOLID prensipleri dikkate alınacaktır.

## 11.1 Single Responsibility Principle

Her class tek bir sorumluluğa sahip olmalıdır.

Örnek:

* Controller sadece HTTP request/response yönetir.
* Handler use-case akışını yönetir.
* Repository veri erişimini yönetir.
* Provider dış veri kaynağından veri alır.
* Validator request doğrulaması yapar.

## 11.2 Open/Closed Principle

Sistem yeni özelliklere açık, mevcut kodu değiştirmeye kapalı olmalıdır.

Örnek:

* Yeni provider eklemek için mevcut provider kodu değiştirilmemelidir.
* Yeni quiz tipi eklemek için mevcut quiz sistemi baştan yazılmamalıdır.
* Phrase ve Sentence eklendiğinde dictionary sistemi bozulmamalıdır.

## 11.3 Liskov Substitution Principle

Bir interface’i implemente eden sınıflar, o interface beklenen her yerde sorunsuz kullanılabilmelidir.

Örnek:

```text
IDictionaryProvider
```

Bu interface’i ileride farklı providerlar implemente edebilir:

```text
PrototypeDictionaryProvider
ExternalDictionaryProvider
CachedDictionaryProvider
```

Application katmanı bu providerların hangisi olduğunu bilmeden çalışabilmelidir.

## 11.4 Interface Segregation Principle

Interface’ler gereksiz büyük olmamalıdır.

Yanlış yaklaşım:

```text
IWordixService
- Lookup yap
- Quiz başlat
- Dictionary kaydet
- Provider çağır
- Progress güncelle
```

Doğru yaklaşım:

```text
ITextNormalizer
IDictionaryProvider
IQuizQuestionGenerator
ILearningProgressUpdater
ICurrentUserService
```

Küçük ve amaca yönelik interface’ler daha sürdürülebilirdir.

## 11.5 Dependency Inversion Principle

Üst seviye katmanlar alt seviye teknik detaylara doğrudan bağımlı olmamalıdır.

Örnek:

Application katmanı doğrudan `WordixDbContext` kullanmamalıdır.

Bunun yerine interface kullanmalıdır:

```text
IUnitOfWork
```

Gerçek implementasyon Persistence katmanında yapılır.

---

## 12. Genişletilebilirlik Hedefleri

Wordix aşağıdaki genişletmelere hazır olacak şekilde tasarlanacaktır:

* Yeni dil ekleme
* Phrase desteği
* Sentence desteği
* Deck modülü
* Yeni quiz tipi
* Writing quiz
* Yeni dictionary provider
* Import sistemi
* Admin analytics
* Motivation modülü
* Teacher-Student modülü

Bu modüller ilk prototipte kodlanmayacaktır.

Ancak ilk prototip mimarisi bu modülleri ileride eklemeye uygun olacaktır.

---

## 13. Özet

Wordix backend mimarisi şu kurallara dayanacaktır:

1. Domain katmanı bağımsız kalacaktır.
2. Controller içinde iş kuralı yazılmayacaktır.
3. Application katmanı use-case akışlarını yönetecektir.
4. Persistence katmanı database işlemlerini yapacaktır.
5. Infrastructure katmanı dış servis implementasyonlarını yapacaktır.
6. Api katmanı HTTP üzerinden dış dünyaya açılacaktır.
7. Shared katmanı ortak modelleri tutacaktır.
8. LearningItem ortak çatı olarak kullanılacaktır.
9. Keycloak authentication baştan kurulacaktır.
10. Sistem Open/Closed Principle’a uygun şekilde genişletilebilir tasarlanacaktır.
