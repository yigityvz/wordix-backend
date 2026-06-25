# Wordix Project Scope

## 1. Projenin Amacı

Wordix, kullanıcıların İngilizce kelime, phrase ve cümleleri arayabildiği, anlamlarını ve örnek kullanımlarını görebildiği, istedikleri içerikleri kişisel dictionary alanlarına kaydedebildiği ve bu içerikler üzerinden quiz/review yapabildiği bir dil öğrenme uygulamasıdır.

Bu projenin backend tarafındaki temel amacı sadece basit CRUD endpointleri yazmak değildir. Wordix backend; authentication, authorization, kullanıcı profili eşleştirme, kelime arama, öğrenme kayıtları, quiz oturumları, cevap değerlendirme ve öğrenme ilerlemesi gibi gerçek backend problemlerini profesyonel bir mimariyle çözmeyi hedefler.

Proje Clean Architecture ve SOLID prensiplerine uygun geliştirilecektir. Böylece sistem ileride yeni öğrenilebilir içerik tipleri, yeni quiz türleri, yeni provider servisleri, yeni diller ve Teacher-Student gibi modüller eklendiğinde mevcut yapıyı baştan değiştirmeden genişletilebilir olacaktır.

---

## 2. İlk MVP / Prototip Kapsamı

İlk MVP’nin amacı tüm Wordix ürününü bitirmek değildir.

İlk MVP’nin amacı, gerçek authentication, gerçek authorization, gerçek database ve gerçek API endpointleriyle çalışan küçük ama uçtan uca test edilebilir bir backend prototipi oluşturmaktır.

İlk prototipte şu akış çalışacaktır:

1. Kullanıcı Keycloak üzerinden giriş yapar.
2. Keycloak kullanıcıya JWT access token üretir.
3. Backend gelen JWT token’ı doğrular.
4. Backend token içindeki kullanıcı bilgilerini okur.
5. `/api/profile/me` endpointi token’daki kullanıcıyı Wordix `UserProfile` kaydıyla eşleştirir.
6. Kullanıcı kelime lookup işlemi yapar.
7. Sistem ilk prototipte input’u `Word` olarak ele alır.
8. Aranan kelime database’de varsa mevcut `LearningItem`, `Word` ve `Meaning` bilgileri döner.
9. Aranan kelime database’de yoksa ilk provider implementasyonu üzerinden veri alınır.
10. Yeni `LearningItem`, `Word` ve `Meaning` kayıtları oluşturulur.
11. Kullanıcının araması `LookupHistory` olarak kaydedilir.
12. Kullanıcı arama sonucunu dictionary alanına kaydeder.
13. `UserLearningItem` oluşturulur.
14. `UserLearningProgress` oluşturulur.
15. Kullanıcı kendi dictionary kayıtları üzerinden basit test quiz başlatır.
16. `QuizSession`, `QuizQuestion` ve `QuizOption` kayıtları oluşur.
17. Kullanıcı quiz sorusuna cevap verir.
18. `QuizAnswer` kaydı oluşur.
19. Cevap doğru/yanlış olarak değerlendirilir.
20. `UserLearningProgress` güncellenir.
21. `LearningProgressHistory` kaydı oluşturulur.
22. Swagger veya Postman üzerinden tüm akış test edilir.

---

## 3. MVP’de Aktif Olacak Temel Modüller

İlk prototipte aşağıdaki modüller aktif olarak geliştirilecektir:

### 3.1 Authentication & Authorization

* Keycloak kullanılacaktır.
* Fake login veya mock user kullanılmayacaktır.
* Kullanıcı gerçek JWT token ile backend’e istek atacaktır.
* Backend JWT Bearer Authentication ile token doğrulaması yapacaktır.
* Role based authorization altyapısı kurulacaktır.

İlk roller:

* basic_user
* admin

Future-ready roller:

* teacher
* student

---

### 3.2 User Profile

Keycloak kullanıcısı ile Wordix database içindeki kullanıcı profili eşleştirilecektir.

Bu amaçla `UserProfile` entity’si kullanılacaktır.

Keycloak authentication’dan sorumlu olacaktır. Wordix ise kullanıcının uygulama içindeki profil, dictionary, quiz ve learning progress verilerini kendi database’inde tutacaktır.

---

### 3.3 Learning Catalog

Sistemin merkezinde `LearningItem` bulunacaktır.

`LearningItem`, öğrenilebilir içerikler için ortak çatıdır.

İleride şu içerik tiplerini temsil edecektir:

* Word
* Phrase
* Sentence

İlk prototipte sadece `Word` aktif kullanılacaktır. Ancak mimari baştan `Phrase` ve `Sentence` destekleyecek şekilde kurulacaktır.

---

### 3.4 Lookup

Kullanıcı kelime araması yapabilecektir.

İlk prototipte lookup sistemi şu şekilde çalışacaktır:

* Kullanıcı bir text gönderir.
* Text normalize edilir.
* İlk prototipte input Word olarak kabul edilir.
* Database’de kelime aranır.
* Varsa mevcut kayıt döner.
* Yoksa provider altyapısı üzerinden yeni kayıt oluşturulur.
* Lookup işlemi `LookupHistory` olarak saklanır.

---

### 3.5 User Dictionary

Kullanıcı aradığı kelimeyi kendi dictionary alanına kaydedebilir.

Kullanıcıya frontend’de bu alan “Dictionary” olarak görünecektir. Teknik olarak ise bu alan `UserLearningItem` tablosundan beslenecektir.

Bu karar önemlidir çünkü ileride kullanıcı sadece Word değil, Phrase ve Sentence kayıtlarını da aynı dictionary içinde görebilecektir.

---

### 3.6 Learning Progress

Kullanıcının her kaydettiği öğrenme içeriği için ilerleme durumu takip edilecektir.

Bunun için `UserLearningProgress` entity’si kullanılacaktır.

İlk prototipte basit olarak şu bilgiler takip edilebilir:

* Doğru cevap sayısı
* Yanlış cevap sayısı
* Ardışık doğru cevap sayısı
* Öğrenme durumu
* Confidence score

---

### 3.7 Quiz

İlk prototipte basit çoktan seçmeli test quiz yapılacaktır.

Quiz akışı şu temel entityler üzerinden ilerleyecektir:

* QuizSession
* QuizQuestion
* QuizOption
* QuizAnswer

Kullanıcı dictionary’sine kaydettiği kelimelerden quiz başlatabilecektir.

---

### 3.8 Learning Progress History

Quiz cevaplarından sonra kullanıcının öğrenme ilerlemesi güncellenecektir.

Her önemli değişiklik `LearningProgressHistory` tablosuna kaydedilecektir.

Bu sayede ileride kullanıcıya gelişim grafikleri, tekrar önerileri ve istatistikler sunulabilecektir.

---

## 4. MVP Dışında Bırakılan Ama Future-Ready Olacak Özellikler

Aşağıdaki özellikler ilk prototipte aktif olarak kodlanmayacaktır.

Ancak mimari bu özellikleri ileride eklemeye uygun olacak şekilde kurulacaktır.

* Phrase desteği
* Sentence desteği
* Deck modülü
* Writing quiz
* User notes
* User flags
* System recommendations
* Import/provider sistemi
* Admin analytics
* Motivation modülü
* Statistics ve dashboard
* Teacher-Student modülü

---

## 5. Teacher-Student Modülü Hakkında Karar

Teacher-Student modülü ilk prototipte yapılmayacaktır.

Ancak proje ileride öğretmen-öğrenci ilişkisi kurmaya uygun olacak şekilde geliştirilecektir.

İleride eklenebilecek yapılar:

* TeacherStudentInvitation
* TeacherStudentRelation
* Assignment
* AssignmentDeck
* StudentAssignmentProgress

Bu modül ilk aşamada kapsam dışında bırakılmıştır çünkü MVP’nin ana hedefi bireysel kullanıcının kelime araması, dictionary kaydı ve quiz/review akışını gerçek backend altyapısıyla çalıştırmaktır.

Teacher-Student modülü daha sonra ayrı bir faz olarak ele alınacaktır.

---

## 6. MVP Başarı Kriterleri

İlk prototip başarılı sayılmak için aşağıdaki maddeler çalışır durumda olmalıdır:

1. Keycloak Docker üzerinde çalışmalıdır.
2. Kullanıcı Keycloak üzerinden token alabilmelidir.
3. Backend JWT token doğrulamalıdır.
4. `/api/profile/me` endpointi kullanıcıyı oluşturmalı veya getirmelidir.
5. Kullanıcı kelime lookup yapabilmelidir.
6. Lookup sonucunda `LearningItem`, `Word`, `Meaning` ve `LookupHistory` kayıtları oluşmalıdır.
7. Kullanıcı kelimeyi dictionary alanına kaydedebilmelidir.
8. `UserLearningItem` ve `UserLearningProgress` kayıtları oluşmalıdır.
9. Kullanıcı dictionary üzerinden quiz başlatabilmelidir.
10. `QuizSession`, `QuizQuestion` ve `QuizOption` kayıtları oluşmalıdır.
11. Kullanıcı quiz cevabı gönderebilmelidir.
12. `QuizAnswer` kaydı oluşmalıdır.
13. Cevaba göre `UserLearningProgress` güncellenmelidir.
14. Swagger ve Postman ile tüm akış test edilebilmelidir.

---

## 7. Mimari Yaklaşım

Wordix backend Clean Architecture ile geliştirilecektir.

Katmanlar:

* Wordix.Domain
* Wordix.Application
* Wordix.Persistence
* Wordix.Infrastructure
* Wordix.Api
* Wordix.Shared

Controller içinde iş kuralı yazılmayacaktır.

Controller sadece HTTP request’i alacak, gerekli command/query nesnesini oluşturacak ve MediatR üzerinden Application katmanına gönderecektir.

İş kuralları Domain ve Application katmanlarında tutulacaktır.

Database işlemleri Persistence katmanında yapılacaktır.

Keycloak, provider, dış servis ve teknik implementasyonlar Infrastructure katmanında yer alacaktır.

API dış dünyaya açılan katman olacaktır.

---

## 8. Genişletilebilirlik Prensibi

Wordix, Open/Closed Principle dikkate alınarak geliştirilecektir.

Yani sistem yeni özelliklere açık, mevcut çalışan kodu gereksiz yere değiştirmeye kapalı olacaktır.

Örnek hedefler:

* Yeni provider eklenirken mevcut provider kodu bozulmamalıdır.
* Yeni quiz tipi eklenirken mevcut quiz sistemi baştan yazılmamalıdır.
* Phrase ve Sentence eklendiğinde dictionary sistemi bozulmamalıdır.
* Yeni dil eklendiğinde mevcut İngilizce-Türkçe yapı kırılmamalıdır.
* Teacher-Student modülü ileride ayrı bir modül olarak eklenebilmelidir.
