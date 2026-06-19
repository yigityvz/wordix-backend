# Wordix API Plan

## 1. Amaç

Bu doküman, Wordix backend projesinde ilk prototip için geliştirilecek API endpointlerini planlamak için oluşturulmuştur.

Bu dosyanın amacı, daha kod yazmadan önce endpoint akışlarını netleştirmektir.

Böylece Controller, Command, Query, DTO ve Handler dosyalarını yazarken hangi endpointin hangi sorumluluğa sahip olduğunu önceden biliriz.

---

## 2. Genel API Yaklaşımı

Wordix backend API, REST mantığına uygun geliştirilecektir.

API endpointleri şu temel prensiplere göre yazılacaktır:

* Controller içinde iş kuralı yazılmayacaktır.
* Controller sadece HTTP request alacaktır.
* Controller MediatR üzerinden ilgili Command veya Query nesnesini Application katmanına gönderecektir.
* Authentication Keycloak üzerinden yapılacaktır.
* Backend fake login endpointi sağlamayacaktır.
* Kullanıcı gerçek Bearer token ile endpointlere istek atacaktır.
* Hata cevapları standart response modeli ile dönecektir.
* Swagger ve Postman ile tüm prototip akışı test edilecektir.

---

## 3. Authentication Kararı

İlk prototipte authentication Keycloak üzerinden yapılacaktır.

Backend tarafında kullanıcı adı/şifre ile giriş yapan özel bir login endpointi yazılmayacaktır.

Doğru akış şu şekildedir:

1. Kullanıcı Keycloak üzerinden token alır.
2. Kullanıcı backend endpointlerine Bearer token ile istek atar.
3. Backend JWT token'ı doğrular.
4. Backend token claim bilgilerini okur.
5. Kullanıcı Wordix database içindeki UserProfile kaydıyla eşleştirilir.

Bu kararın sebebi, ilk prototipten itibaren gerçek authentication ve authorization akışını kurmaktır.

---

## 4. İlk Prototip Endpoint Listesi

İlk prototipte geliştirilecek endpointler:

```text
GET  /api/profile/me

POST /api/lookups

POST /api/user-dictionary
GET  /api/user-dictionary
GET  /api/user-dictionary/{id}

POST /api/quizzes
POST /api/quizzes/{quizSessionId}/answers
GET  /api/quizzes/{quizSessionId}/summary
```

---

## 5. Profile Endpointleri

## 5.1 Get Current User Profile

```http
GET /api/profile/me
```

### Amaç

Token’daki kullanıcıyı Wordix database içindeki `UserProfile` kaydıyla eşleştirmek.

Bu endpoint, kullanıcının Wordix sistemindeki profilini döner.

Eğer kullanıcı Keycloak’ta var ama Wordix database içinde henüz yoksa, bu endpoint ilk istekte kullanıcıyı oluşturur.

---

### Authorization

Bu endpoint authentication gerektirir.

Yani istek Bearer token ile atılmalıdır.

```http
Authorization: Bearer {access_token}
```

---

### Akış

1. Kullanıcı endpoint’e Bearer token ile istek atar.
2. Backend JWT token’ı doğrular.
3. `ICurrentUserService` token claimlerinden kullanıcı bilgisini okur.
4. `KeycloakUserId`, email, username ve roller alınır.
5. Wordix database içinde bu `KeycloakUserId` ile `UserProfile` aranır.
6. UserProfile varsa döner.
7. UserProfile yoksa oluşturulur.
8. Response döner.

---

### Beklenen Response İçeriği

```json
{
  "id": "guid",
  "keycloakUserId": "keycloak-user-id",
  "email": "basicuser@test.com",
  "username": "basicuser",
  "accountType": "Basic",
  "roles": [
    "basic_user"
  ]
}
```

---

### İlgili Katmanlar

```text
Wordix.Api
- ProfileController

Wordix.Application
- GetCurrentUserProfileQuery
- GetCurrentUserProfileQueryHandler
- IUserProfileSyncService
- ICurrentUserService

Wordix.Infrastructure
- KeycloakCurrentUserService

Wordix.Persistence
- UserProfile repository / DbContext

Wordix.Domain
- UserProfile
```

---

## 6. Lookup Endpointleri

## 6.1 Create Lookup

```http
POST /api/lookups
```

### Amaç

Kullanıcının kelime araması yapmasını sağlar.

İlk prototipte lookup input’u `Word` olarak ele alınacaktır.

İleride aynı endpoint üzerinden Phrase ve Sentence desteği de eklenebilir.

---

### Authorization

Bu endpoint authentication gerektirir.

```http
Authorization: Bearer {access_token}
```

---

### Örnek Request

```json
{
  "text": "achieve",
  "sourceLanguageCode": "en",
  "targetLanguageCode": "tr"
}
```

---

### Request Alanları

| Alan               | Açıklama                                                         |
| ------------------ | ---------------------------------------------------------------- |
| text               | Kullanıcının aradığı kelime veya ileride phrase/sentence input’u |
| sourceLanguageCode | Aranan içeriğin kaynak dili                                      |
| targetLanguageCode | Anlam/çeviri hedef dili                                          |

---

### Akış

1. Kullanıcı Bearer token ile istek atar.
2. Backend token’ı doğrular.
3. Current user bilgisi alınır.
4. UserProfile bulunur veya oluşturulur.
5. Gelen text normalize edilir.
6. İlk prototipte input `Word` olarak sınıflandırılır.
7. Database’de normalized text ile Word aranır.
8. Word varsa ilişkili `LearningItem` ve `Meaning` bilgileri getirilir.
9. Word yoksa `IDictionaryProvider` üzerinden veri alınır.
10. Yeni `LearningItem`, `Word` ve `Meaning` kayıtları oluşturulur.
11. `LookupHistory` kaydı oluşturulur.
12. Kullanıcının bu item’ı dictionary’ye kaydedip kaydetmediği kontrol edilir.
13. Response döner.

---

### Beklenen Kayıtlar

Bu endpoint sonucunda aşağıdaki kayıtlar oluşabilir:

```text
LearningItem
Word
Meaning
LookupHistory
```

Eğer kelime daha önce database’de varsa sadece `LookupHistory` oluşabilir.

---

### Örnek Response

```json
{
  "lookupHistoryId": "guid",
  "learningItemId": "guid",
  "wordId": "guid",
  "text": "achieve",
  "normalizedText": "achieve",
  "itemType": "Word",
  "sourceLanguageCode": "en",
  "targetLanguageCode": "tr",
  "meanings": [
    {
      "id": "guid",
      "translation": "başarmak",
      "partOfSpeech": "verb"
    }
  ],
  "isAlreadySaved": false
}
```

---

### İlgili Katmanlar

```text
Wordix.Api
- LookupsController

Wordix.Application
- LookupRequest
- LookupResponse
- CreateLookupCommand
- CreateLookupCommandHandler
- CreateLookupCommandValidator
- ITextNormalizer
- ILookupClassifier
- IDictionaryProvider

Wordix.Infrastructure
- PrototypeDictionaryProvider
- TextNormalizer
- LookupClassifier

Wordix.Persistence
- LearningItem repository
- LookupHistory repository
- UnitOfWork

Wordix.Domain
- LearningItem
- Word
- Meaning
- LookupHistory
```

---

## 7. User Dictionary Endpointleri

## 7.1 Save Learning Item To Dictionary

```http
POST /api/user-dictionary
```

### Amaç

Kullanıcının bir `LearningItem` kaydını kendi dictionary alanına eklemesini sağlar.

Frontend’de kullanıcı bunu “Dictionary’ye kaydet” olarak görecektir.

Teknik olarak ise bu işlem `UserLearningItem` oluşturur.

---

### Authorization

Bu endpoint authentication gerektirir.

```http
Authorization: Bearer {access_token}
```

---

### Örnek Request

```json
{
  "learningItemId": "guid",
  "selectedMeaningId": "guid"
}
```

---

### Request Alanları

| Alan              | Açıklama                                        |
| ----------------- | ----------------------------------------------- |
| learningItemId    | Dictionary’ye kaydedilecek öğrenilebilir içerik |
| selectedMeaningId | Kullanıcının seçtiği ana anlam                  |

---

### Akış

1. Kullanıcı Bearer token ile istek atar.
2. Current user bilgisi alınır.
3. UserProfile bulunur.
4. `LearningItem` var mı kontrol edilir.
5. `selectedMeaningId` ilgili LearningItem’a ait mi kontrol edilir.
6. Kullanıcı bu LearningItem’ı daha önce kaydetmiş mi kontrol edilir.
7. Daha önce kaydetmediyse `UserLearningItem` oluşturulur.
8. Bu kayıt için `UserLearningProgress` oluşturulur.
9. Response döner.

---

### Beklenen Kayıtlar

```text
UserLearningItem
UserLearningProgress
```

---

### Örnek Response

```json
{
  "id": "guid",
  "learningItemId": "guid",
  "selectedMeaningId": "guid",
  "text": "achieve",
  "translation": "başarmak",
  "learningStatus": "New",
  "confidenceScore": 0
}
```

---

### Business Rule

Aynı kullanıcı aynı LearningItem’ı birden fazla kez kaydedemez.

Bu durumda sistem business error döndürmelidir.

Örnek hata:

```json
{
  "message": "This learning item is already saved in your dictionary."
}
```

---

## 7.2 Get My Dictionary

```http
GET /api/user-dictionary
```

### Amaç

Kullanıcının kendi dictionary kayıtlarını listeler.

Bu endpoint sadece giriş yapan kullanıcının kayıtlarını döndürmelidir.

---

### Authorization

Bu endpoint authentication gerektirir.

```http
Authorization: Bearer {access_token}
```

---

### Akış

1. Kullanıcı Bearer token ile istek atar.
2. Current user bilgisi alınır.
3. UserProfile bulunur.
4. Kullanıcının `UserLearningItem` kayıtları getirilir.
5. Her kayıt için ilişkili `LearningItem`, `Word`, `Meaning` ve progress bilgileri eklenir.
6. Liste response olarak döner.

---

### Örnek Response

```json
[
  {
    "id": "guid",
    "learningItemId": "guid",
    "itemType": "Word",
    "text": "achieve",
    "translation": "başarmak",
    "learningStatus": "New",
    "confidenceScore": 0,
    "correctCount": 0,
    "wrongCount": 0
  }
]
```

---

### Güvenlik Kararı

Kullanıcı sadece kendi dictionary kayıtlarını görebilmelidir.

Başka kullanıcının dictionary kayıtları response içine karışmamalıdır.

---

## 7.3 Get My Dictionary Item Detail

```http
GET /api/user-dictionary/{id}
```

### Amaç

Kullanıcının kendi dictionary’sindeki tek bir kaydın detayını getirir.

Buradaki `{id}`, `UserLearningItemId` değeridir.

---

### Authorization

Bu endpoint authentication gerektirir.

```http
Authorization: Bearer {access_token}
```

---

### Akış

1. Kullanıcı Bearer token ile istek atar.
2. Current user bilgisi alınır.
3. Route üzerinden gelen `id` ile UserLearningItem aranır.
4. Kayıt yoksa NotFound döner.
5. Kayıt varsa bu kaydın giriş yapan kullanıcıya ait olup olmadığı kontrol edilir.
6. Başkasına aitse Forbidden döner.
7. Kullanıcıya aitse detay response döner.

---

### Örnek Response

```json
{
  "id": "guid",
  "learningItemId": "guid",
  "itemType": "Word",
  "text": "achieve",
  "normalizedText": "achieve",
  "meanings": [
    {
      "id": "guid",
      "translation": "başarmak",
      "partOfSpeech": "verb"
    }
  ],
  "selectedMeaningId": "guid",
  "learningProgress": {
    "learningStatus": "New",
    "confidenceScore": 0,
    "correctCount": 0,
    "wrongCount": 0,
    "consecutiveCorrectCount": 0,
    "lastReviewedAt": null
  }
}
```

---

### Güvenlik Kararı

Bu endpoint ownership kontrolü gerektirir.

Yani kullanıcı sadece kendi `UserLearningItem` detayını görebilir.

---

## 8. Quiz Endpointleri

## 8.1 Start Quiz

```http
POST /api/quizzes
```

### Amaç

Kullanıcının kendi dictionary kayıtları üzerinden quiz başlatmasını sağlar.

İlk prototipte quiz tipi çoktan seçmeli test quiz olacaktır.

---

### Authorization

Bu endpoint authentication gerektirir.

```http
Authorization: Bearer {access_token}
```

---

### Örnek Request

```json
{
  "quizType": "Test",
  "quizSourceType": "Dictionary",
  "quizContentMode": "WordsOnly",
  "questionCount": 5
}
```

---

### Request Alanları

| Alan            | Açıklama                  |
| --------------- | ------------------------- |
| quizType        | İlk prototipte Test       |
| quizSourceType  | İlk prototipte Dictionary |
| quizContentMode | İlk prototipte WordsOnly  |
| questionCount   | Oluşturulacak soru sayısı |

---

### Akış

1. Kullanıcı Bearer token ile istek atar.
2. Current user bilgisi alınır.
3. UserProfile bulunur.
4. Kullanıcının dictionary kayıtları getirilir.
5. Quiz için uygun Word item’lar seçilir.
6. `QuizSession` oluşturulur.
7. Her seçilen item için `QuizQuestion` oluşturulur.
8. Her soru için `QuizOption` kayıtları oluşturulur.
9. Doğru seçenek ve yanlış seçenekler belirlenir.
10. Response içinde quiz session, sorular ve seçenekler döner.

---

### Beklenen Kayıtlar

```text
QuizSession
QuizQuestion
QuizOption
```

---

### Örnek Response

```json
{
  "quizSessionId": "guid",
  "quizType": "Test",
  "questionCount": 5,
  "questions": [
    {
      "quizQuestionId": "guid",
      "learningItemId": "guid",
      "questionText": "achieve",
      "questionType": "MultipleChoiceTranslation",
      "options": [
        {
          "quizOptionId": "guid",
          "text": "başarmak"
        },
        {
          "quizOptionId": "guid",
          "text": "geliştirmek"
        },
        {
          "quizOptionId": "guid",
          "text": "unutmak"
        },
        {
          "quizOptionId": "guid",
          "text": "koşmak"
        }
      ]
    }
  ]
}
```

---

### Business Rule

Kullanıcının dictionary’sinde quiz oluşturmak için yeterli item yoksa sistem business error döndürmelidir.

Örnek:

```json
{
  "message": "You need at least 4 saved words to start a quiz."
}
```

---

## 8.2 Submit Quiz Answer

```http
POST /api/quizzes/{quizSessionId}/answers
```

### Amaç

Kullanıcının quiz sorusuna verdiği cevabı kaydeder ve progress bilgisini günceller.

---

### Authorization

Bu endpoint authentication gerektirir.

```http
Authorization: Bearer {access_token}
```

---

### Örnek Request

```json
{
  "quizQuestionId": "guid",
  "selectedQuizOptionId": "guid"
}
```

---

### Request Alanları

| Alan                 | Açıklama                     |
| -------------------- | ---------------------------- |
| quizQuestionId       | Cevaplanan soru              |
| selectedQuizOptionId | Kullanıcının seçtiği seçenek |

---

### Akış

1. Kullanıcı Bearer token ile istek atar.
2. Current user bilgisi alınır.
3. `quizSessionId` ile quiz session aranır.
4. QuizSession var mı kontrol edilir.
5. QuizSession giriş yapan kullanıcıya mı ait kontrol edilir.
6. QuizQuestion var mı kontrol edilir.
7. Seçilen QuizOption bu soruya ait mi kontrol edilir.
8. Seçeneğin doğru olup olmadığı hesaplanır.
9. `QuizAnswer` kaydı oluşturulur.
10. İlgili `UserLearningProgress` güncellenir.
11. `LearningProgressHistory` kaydı oluşturulur.
12. Response döner.

---

### Beklenen Kayıtlar

```text
QuizAnswer
LearningProgressHistory
```

Ayrıca şu kayıt güncellenir:

```text
UserLearningProgress
```

---

### Örnek Response

```json
{
  "quizAnswerId": "guid",
  "quizQuestionId": "guid",
  "selectedQuizOptionId": "guid",
  "isCorrect": true,
  "answerResult": "Correct",
  "correctOptionId": "guid",
  "updatedProgress": {
    "learningStatus": "Learning",
    "confidenceScore": 15,
    "correctCount": 1,
    "wrongCount": 0,
    "consecutiveCorrectCount": 1
  }
}
```

---

### Güvenlik Kararı

Kullanıcı sadece kendi QuizSession kayıtlarına cevap gönderebilir.

Başka kullanıcıya ait quizSessionId üzerinden cevap gönderilirse Forbidden dönmelidir.

---

## 8.3 Get Quiz Summary

```http
GET /api/quizzes/{quizSessionId}/summary
```

### Amaç

Quiz oturumunun sonucunu özetler.

---

### Authorization

Bu endpoint authentication gerektirir.

```http
Authorization: Bearer {access_token}
```

---

### Akış

1. Kullanıcı Bearer token ile istek atar.
2. Current user bilgisi alınır.
3. QuizSession bulunur.
4. QuizSession giriş yapan kullanıcıya ait mi kontrol edilir.
5. Quiz soruları ve cevapları getirilir.
6. Toplam soru sayısı hesaplanır.
7. Doğru cevap sayısı hesaplanır.
8. Yanlış cevap sayısı hesaplanır.
9. Başarı oranı hesaplanır.
10. Response döner.

---

### Örnek Response

```json
{
  "quizSessionId": "guid",
  "totalQuestions": 5,
  "answeredQuestions": 5,
  "correctCount": 4,
  "wrongCount": 1,
  "successRate": 80,
  "results": [
    {
      "quizQuestionId": "guid",
      "questionText": "achieve",
      "selectedAnswer": "başarmak",
      "correctAnswer": "başarmak",
      "isCorrect": true
    }
  ]
}
```

---

### Güvenlik Kararı

Kullanıcı sadece kendi quiz summary bilgisini görebilir.

Başka kullanıcıya ait quizSessionId ile summary istenirse Forbidden dönmelidir.

---

## 9. Standart Hata Senaryoları

API genelinde aşağıdaki hata senaryoları standart şekilde ele alınacaktır.

## 9.1 Unauthorized

Kullanıcı token göndermediyse veya token geçersizse:

```http
401 Unauthorized
```

## 9.2 Forbidden

Kullanıcı erişmeye çalıştığı kaydın sahibi değilse:

```http
403 Forbidden
```

## 9.3 Not Found

İstenen kayıt bulunamadıysa:

```http
404 Not Found
```

## 9.4 Validation Error

Request modeli geçersizse:

```http
400 Bad Request
```

Örnek:

```json
{
  "message": "Validation failed.",
  "errors": [
    {
      "field": "text",
      "message": "Text is required."
    }
  ]
}
```

## 9.5 Business Rule Error

İş kuralı ihlali varsa:

```http
400 Bad Request
```

Örnek:

```json
{
  "message": "This learning item is already saved in your dictionary."
}
```

---

## 10. İlk Prototip Test Akışı

İlk prototip tamamlandığında Swagger veya Postman ile şu sırayla test yapılacaktır:

1. Keycloak üzerinden token alınır.
2. Swagger Authorize alanına Bearer token girilir.
3. `GET /api/profile/me` çağrılır.
4. Kullanıcının Wordix UserProfile kaydı oluştu mu kontrol edilir.
5. `POST /api/lookups` ile kelime aranır.
6. Database’de LearningItem, Word, Meaning ve LookupHistory oluştu mu kontrol edilir.
7. `POST /api/user-dictionary` ile kelime dictionary’ye kaydedilir.
8. UserLearningItem ve UserLearningProgress oluştu mu kontrol edilir.
9. `GET /api/user-dictionary` ile kayıt listelenir.
10. `POST /api/quizzes` ile quiz başlatılır.
11. QuizSession, QuizQuestion ve QuizOption oluştu mu kontrol edilir.
12. `POST /api/quizzes/{quizSessionId}/answers` ile cevap gönderilir.
13. QuizAnswer oluştu mu kontrol edilir.
14. UserLearningProgress güncellendi mi kontrol edilir.
15. `GET /api/quizzes/{quizSessionId}/summary` ile quiz özeti alınır.

---

## 11. MVP Endpoint Başarı Kriterleri

İlk prototip API başarılı sayılmak için aşağıdaki kriterleri sağlamalıdır:

* Token olmadan protected endpointlere erişilememeli.
* Geçerli token ile profile endpoint çalışmalı.
* UserProfile otomatik oluşturulmalı veya mevcut profil dönmeli.
* Lookup endpoint gerçek database kaydı oluşturmalı.
* Dictionary save endpoint duplicate kayıtları engellemeli.
* Dictionary list endpoint sadece giriş yapan kullanıcının kayıtlarını dönmeli.
* Dictionary detail endpoint ownership kontrolü yapmalı.
* Quiz endpoint kullanıcının dictionary kayıtlarından soru üretmeli.
* Answer endpoint cevabı kaydetmeli ve progress güncellemeli.
* Summary endpoint doğru quiz sonucunu dönmeli.
* Swagger ve Postman ile tüm akış test edilebilmeli.

---

## 12. Notlar

Bu doküman ilk API planıdır.

Kodlama ilerledikçe request ve response modelleri değişebilir.

Ancak büyük mimari kararlar korunacaktır:

* Authentication Keycloak ile yapılacak.
* Controller içinde iş kuralı yazılmayacak.
* Use-case akışları Application katmanında olacak.
* Dictionary sistemi LearningItem üzerinden çalışacak.
* Quiz sistemi ileride yeni quiz tiplerine açık olacak.
