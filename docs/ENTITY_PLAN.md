# Wordix Entity Plan

## 1. Amaç

Bu doküman, Wordix backend projesinde kullanılacak entity yapılarını planlamak için oluşturulmuştur.

Entity, veritabanında tabloya karşılık gelebilecek ve sistemde bir iş kavramını temsil eden class yapısıdır.

Örneğin:

* UserProfile bir kullanıcı profilini temsil eder.
* LearningItem öğrenilebilir bir içeriği temsil eder.
* Word bir kelimeyi temsil eder.
* Meaning bir anlamı temsil eder.
* QuizSession bir quiz oturumunu temsil eder.

Bu dosyada yazılan entityler ilerleyen fazlarda `Wordix.Domain` katmanı içinde class olarak kodlanacaktır.

---

## 2. Ana Domain Yaklaşımı

Wordix sadece `Word` entity’si üzerine kurulmayacaktır.

Sistemin merkezinde `LearningItem` bulunacaktır.

`LearningItem`, öğrenilebilir içerikler için ortak çatıdır.

İleride şu içerik tiplerini destekleyecektir:

* Word
* Phrase
* Sentence

İlk prototipte sadece `Word` aktif kullanılacaktır.

Ancak dictionary, quiz ve progress yapıları doğrudan `Word` üzerinden değil, `LearningItem` üzerinden tasarlanacaktır.

Bu kararın amacı sistemi future-ready hale getirmektir.

Yanlış yaklaşım:

```text
UserLearningItem -> Word
QuizQuestion -> Word
Meaning -> Word
```

Doğru yaklaşım:

```text
UserLearningItem -> LearningItem
QuizQuestion -> LearningItem
Meaning -> LearningItem
```

Bu sayede ileride Phrase ve Sentence eklendiğinde dictionary ve quiz sistemleri baştan yazılmak zorunda kalmaz.

---

## 3. İlk Prototipte Kullanılacak Entityler

İlk prototipte aşağıdaki entityler aktif olarak kodlanacaktır:

* UserProfile
* UserPreference
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
* LearningProgressHistory

---

## 4. Identity & Profile Entityleri

## 4.1 UserProfile

`UserProfile`, Keycloak kullanıcısının Wordix database içindeki karşılığıdır.

Keycloak authentication işlemini yönetir. Ancak Wordix, kullanıcının uygulama içindeki profil, dictionary, quiz ve progress verilerini kendi database’inde tutar.

Bu yüzden Keycloak kullanıcısını Wordix içindeki bir profile bağlamamız gerekir.

Örnek alanlar:

```text
UserProfile
- Id
- KeycloakUserId
- Email
- Username
- DisplayName
- AccountType
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Token’daki kullanıcıyı Wordix kullanıcısıyla eşleştirmek
* Dictionary kayıtlarını kullanıcıya bağlamak
* Quiz kayıtlarını kullanıcıya bağlamak
* Learning progress kayıtlarını kullanıcıya bağlamak

İlişkiler:

```text
UserProfile 1 - N LookupHistory
UserProfile 1 - N UserLearningItem
UserProfile 1 - N QuizSession
UserProfile 1 - 1 UserPreference
```

Önemli constraint kararları:

* `KeycloakUserId` unique olmalıdır.
* `Email` unique olabilir.
* Her Keycloak kullanıcısı Wordix tarafında bir UserProfile ile eşleşmelidir.

---

## 4.2 UserPreference

`UserPreference`, kullanıcının uygulama içi tercihlerini tutar.

İlk prototipte minimum alanlarla oluşturulabilir.

Örnek alanlar:

```text
UserPreference
- Id
- UserProfileId
- DefaultSourceLanguageId
- DefaultTargetLanguageId
- PreferredDifficultyGroup
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Kullanıcının varsayılan kaynak dilini tutmak
* Kullanıcının varsayılan hedef dilini tutmak
* İleride seviye, tekrar, bildirim gibi tercihleri yönetmek

İlişkiler:

```text
UserProfile 1 - 1 UserPreference
Language 1 - N UserPreference
```

İlk prototipte çok aktif kullanılmayabilir ama kullanıcı tercihleri için altyapı hazır olur.

---

## 5. Language Entityleri

## 5.1 Language

`Language`, sistemde desteklenen dilleri temsil eder.

Örnek diller:

* English / en
* Turkish / tr

Örnek alanlar:

```text
Language
- Id
- Name
- Code
- IsActive
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Lookup kaynak dilini belirtmek
* Meaning hedef dilini belirtmek
* İleride yeni dil desteği eklemek

Önemli constraint kararları:

* `Code` unique olmalıdır.
* Örnek: `en`, `tr`, `de`, `fr`

İlişkiler:

```text
Language 1 - N LearningItem
Language 1 - N Meaning
```

Neden string olarak sadece "en" veya "tr" tutmuyoruz?

Çünkü ileride dil yönetimini büyütmek isteyebiliriz.

Örneğin:

* Dil aktif mi?
* Dil adı ne?
* Hangi diller destekleniyor?
* Admin panelden dil eklenebilir mi?

Bu yüzden `Language` ayrı entity olarak tasarlanır.

---

## 6. Content Catalog Entityleri

## 6.1 LearningItem

`LearningItem`, Wordix sisteminin en önemli entitylerinden biridir.

Öğrenilebilir içerikler için ortak çatıdır.

İleride şu içerikleri temsil edecektir:

* Word
* Phrase
* Sentence

İlk prototipte sadece Word aktif kullanılacaktır.

Örnek alanlar:

```text
LearningItem
- Id
- ItemType
- SourceLanguageId
- DifficultyGroup
- CefrLevel
- IsActive
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Word, Phrase ve Sentence için ortak kimlik sağlamak
* Dictionary sistemini ortak hale getirmek
* Quiz sistemini ortak hale getirmek
* Progress sistemini ortak hale getirmek

İlişkiler:

```text
LearningItem 1 - 1 Word
LearningItem 1 - N Meaning
LearningItem 1 - N LookupHistory
LearningItem 1 - N UserLearningItem
LearningItem 1 - N QuizQuestion
Language 1 - N LearningItem
```

Neden önemli?

Eğer dictionary doğrudan `WordId` tutarsa ileride Phrase ve Sentence desteği eklemek zorlaşır.

Ama dictionary `LearningItemId` tutarsa sistem genişletilebilir olur.

Bu karar Open/Closed Principle ile uyumludur.

---

## 6.2 Word

`Word`, ilk prototipte aktif kullanılacak içerik tipidir.

Bir Word, bir LearningItem ile bire bir ilişkilidir.

Örnek alanlar:

```text
Word
- Id
- LearningItemId
- Text
- NormalizedText
- PartOfSpeech
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Kelimenin orijinal text değerini tutmak
* Normalize edilmiş halini tutmak
* Kelimeyi lookup sırasında hızlı bulmak

Örnek:

```text
Text: "Achieve"
NormalizedText: "achieve"
```

İlişkiler:

```text
LearningItem 1 - 1 Word
```

Önemli constraint kararları:

* `LearningItemId` unique olmalıdır.
* `NormalizedText + SourceLanguageId` birlikte unique olabilir.

Bu sayede aynı dilde aynı kelime gereksiz yere tekrar tekrar oluşmaz.

---

## 6.3 Meaning

`Meaning`, bir LearningItem’ın anlamlarını tutar.

İlk prototipte Word anlamları için kullanılacaktır.

Ancak ileride Phrase için de kullanılabilir.

Örnek alanlar:

```text
Meaning
- Id
- LearningItemId
- TargetLanguageId
- Translation
- PartOfSpeech
- Definition
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Kelimenin hedef dildeki anlamını tutmak
* Bir kelimenin birden fazla anlamını desteklemek
* Dictionary’ye kaydederken kullanıcının seçtiği anlamı belirlemek

Örnek:

```text
LearningItem: achieve
TargetLanguage: Turkish
Translation: başarmak
PartOfSpeech: verb
```

İlişkiler:

```text
LearningItem 1 - N Meaning
Language 1 - N Meaning
UserLearningItem N - 1 Meaning
QuizOption N - 1 Meaning
```

Neden Meaning doğrudan Word’e bağlanmıyor?

Çünkü ileride Phrase veya Sentence için de anlam/çeviri tutulabilir.

Bu yüzden Meaning, `LearningItem` üzerinden ilişkilendirilir.

---

## 7. Lookup/Search Entityleri

## 7.1 LookupHistory

`LookupHistory`, kullanıcının yaptığı arama işlemlerini saklar.

Örnek alanlar:

```text
LookupHistory
- Id
- UserProfileId
- LearningItemId
- InputText
- NormalizedInputText
- InputType
- SourceLanguageId
- TargetLanguageId
- CreatedAt
```

Kullanım amacı:

* Kullanıcının arama geçmişini tutmak
* En çok aranan kelimeleri analiz etmek
* Provider kullanımını izlemek
* Admin analytics için veri sağlamak

İlişkiler:

```text
UserProfile 1 - N LookupHistory
LearningItem 1 - N LookupHistory
Language 1 - N LookupHistory
```

İlk prototipte lookup yapıldığında en az bir `LookupHistory` kaydı oluşacaktır.

---

## 8. User Dictionary Entityleri

## 8.1 UserLearningItem

`UserLearningItem`, kullanıcının dictionary’ye kaydettiği öğrenilebilir içeriği temsil eder.

Frontend’de bu alan kullanıcıya “Dictionary” olarak görünecektir.

Teknik olarak ise dictionary, `UserLearningItem` tablosundan beslenecektir.

Örnek alanlar:

```text
UserLearningItem
- Id
- UserProfileId
- LearningItemId
- SelectedMeaningId
- AddedAt
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Kullanıcının kaydettiği Word/Phrase/Sentence itemlarını tutmak
* Dictionary listesini oluşturmak
* Quiz için kaynak sağlamak
* Progress ile ilişki kurmak

İlişkiler:

```text
UserProfile 1 - N UserLearningItem
LearningItem 1 - N UserLearningItem
Meaning 1 - N UserLearningItem
UserLearningItem 1 - 1 UserLearningProgress
```

Önemli constraint kararları:

```text
UserProfileId + LearningItemId unique olmalıdır.
```

Bu sayede kullanıcı aynı LearningItem’ı dictionary’ye birden fazla kez kaydedemez.

---

## 8.2 UserLearningProgress

`UserLearningProgress`, kullanıcının bir öğrenme item’ındaki ilerlemesini tutar.

Bu entity, dictionary’ye kaydedilen her item için oluşturulur.

Örnek alanlar:

```text
UserLearningProgress
- Id
- UserLearningItemId
- LearningStatus
- ConfidenceScore
- CorrectCount
- WrongCount
- ConsecutiveCorrectCount
- LastReviewedAt
- NextReviewAt
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Kullanıcının kelimeyi öğrenme durumunu takip etmek
* Quiz cevaplarına göre progress güncellemek
* İleride review schedule ve spaced repetition altyapısı kurmak
* Kullanıcı istatistiklerine veri sağlamak

Örnek LearningStatus değerleri:

```text
New
Learning
Review
Mastered
Difficult
```

İlişkiler:

```text
UserLearningItem 1 - 1 UserLearningProgress
UserLearningProgress 1 - N LearningProgressHistory
```

Önemli constraint kararları:

```text
UserLearningItemId unique olmalıdır.
```

Çünkü bir dictionary item için bir progress kaydı olmalıdır.

---

## 9. Quiz Entityleri

## 9.1 QuizSession

`QuizSession`, kullanıcının başlattığı bir quiz oturumunu temsil eder.

Örnek alanlar:

```text
QuizSession
- Id
- UserProfileId
- QuizType
- QuizSourceType
- QuizContentMode
- StartedAt
- CompletedAt
- CreatedAt
- UpdatedAt
```

Kullanım amacı:

* Quiz oturumunu takip etmek
* Soruları aynı session altında toplamak
* Summary endpointinde sonucu hesaplamak

İlişkiler:

```text
UserProfile 1 - N QuizSession
QuizSession 1 - N QuizQuestion
QuizSession 1 - N QuizAnswer
```

İlk prototipte quiz tipi:

```text
QuizType = Test
QuizSourceType = Dictionary
QuizContentMode = WordsOnly
```

---

## 9.2 QuizQuestion

`QuizQuestion`, quiz içindeki tek bir soruyu temsil eder.

Örnek alanlar:

```text
QuizQuestion
- Id
- QuizSessionId
- LearningItemId
- QuestionType
- QuestionText
- Order
- CreatedAt
```

Kullanım amacı:

* Quiz oturumundaki soruları saklamak
* Her soruyu ilgili LearningItem ile ilişkilendirmek
* Kullanıcının cevabını değerlendirmek

İlişkiler:

```text
QuizSession 1 - N QuizQuestion
LearningItem 1 - N QuizQuestion
QuizQuestion 1 - N QuizOption
QuizQuestion 1 - N QuizAnswer
```

İlk prototipte question type:

```text
QuestionType = MultipleChoiceTranslation
```

---

## 9.3 QuizOption

`QuizOption`, bir quiz sorusunun seçeneklerini temsil eder.

Örnek alanlar:

```text
QuizOption
- Id
- QuizQuestionId
- MeaningId
- OptionText
- IsCorrect
- Order
- CreatedAt
```

Kullanım amacı:

* Çoktan seçmeli soruların seçeneklerini tutmak
* Hangi seçeneğin doğru olduğunu belirlemek
* Kullanıcının seçtiği cevabı değerlendirmek

İlişkiler:

```text
QuizQuestion 1 - N QuizOption
Meaning 1 - N QuizOption
QuizOption 1 - N QuizAnswer
```

Not:

İlk prototipte doğru cevap `IsCorrect = true` olan option üzerinden hesaplanacaktır.

---

## 9.4 QuizAnswer

`QuizAnswer`, kullanıcının quiz sorusuna verdiği cevabı temsil eder.

Örnek alanlar:

```text
QuizAnswer
- Id
- QuizSessionId
- QuizQuestionId
- SelectedQuizOptionId
- UserProfileId
- AnswerResult
- IsCorrect
- AnsweredAt
- CreatedAt
```

Kullanım amacı:

* Kullanıcının verdiği cevabı kaydetmek
* Doğru/yanlış sonucunu saklamak
* Quiz summary oluşturmak
* Learning progress güncellemek

İlişkiler:

```text
QuizSession 1 - N QuizAnswer
QuizQuestion 1 - N QuizAnswer
QuizOption 1 - N QuizAnswer
UserProfile 1 - N QuizAnswer
```

Önemli business rule:

Kullanıcı aynı quiz question için birden fazla cevap göndermemelidir.

Bu kural ileride unique constraint veya business validation ile korunabilir.

---

## 10. Review / Learning Entityleri

## 10.1 LearningProgressHistory

`LearningProgressHistory`, progress üzerindeki değişiklikleri geçmiş olarak saklar.

Örnek alanlar:

```text
LearningProgressHistory
- Id
- UserLearningProgressId
- QuizAnswerId
- PreviousStatus
- NewStatus
- PreviousConfidenceScore
- NewConfidenceScore
- CorrectCount
- WrongCount
- ChangedAt
```

Kullanım amacı:

* Kullanıcının öğrenme gelişimini takip etmek
* Quiz cevaplarının progress üzerindeki etkisini kaydetmek
* İleride grafik, istatistik ve dashboard verisi üretmek

İlişkiler:

```text
UserLearningProgress 1 - N LearningProgressHistory
QuizAnswer 1 - 1 LearningProgressHistory
```

Bu entity ilk prototipte basit tutulabilir.

Ancak ileride learning analytics için çok önemli olacaktır.

---

## 11. MVP Sonrası Entity Grupları

Aşağıdaki entityler ilk prototipte aktif kodlanmayacaktır.

Ancak mimari bu entityleri ileride eklemeye uygun olacaktır.

---

## 11.1 Content Catalog Future Entityleri

```text
Phrase
Sentence
ExampleSentence
SentenceTranslation
ContentTag
LearningItemTag
```

Açıklama:

Phrase ve Sentence desteği geldikçe `LearningItem` çatısı altında yeni içerik tipleri aktif olacaktır.

---

## 11.2 User Dictionary Future Entityleri

```text
UserLearningNote
UserLearningFlag
UserLearningItemEvent
```

Açıklama:

Kullanıcı ileride kelimelere not ekleyebilir, favori/zor gibi flagler koyabilir ve dictionary aktiviteleri event olarak tutulabilir.

---

## 11.3 Deck Entityleri

```text
Deck
DeckItem
DeckActivityEvent
```

Açıklama:

Deck modülü ile kullanıcı kendi kelime gruplarını oluşturabilir.

DeckItem yine doğrudan Word değil, LearningItem üzerinden tasarlanmalıdır.

---

## 11.4 Motivation Entityleri

```text
MotivationMessageTemplate
UserMotivationEvent
```

Açıklama:

Kullanıcıya çalışma alışkanlığına göre motivasyon mesajları üretmek için kullanılacaktır.

---

## 11.5 Import / Provider Entityleri

```text
DataSource
ImportJob
ImportItem
ProviderRequestLog
ExternalContentCache
```

Açıklama:

Dış dictionary, translation veya sentence kaynaklarından veri almak ve bu işlemleri takip etmek için kullanılacaktır.

---

## 11.6 Admin Entityleri

```text
AdminActionLog
SystemSetting
```

Açıklama:

Admin işlemlerini loglamak ve sistem ayarlarını yönetmek için kullanılacaktır.

---

## 11.7 Teacher-Student Future Module Entityleri

```text
TeacherStudentInvitation
TeacherStudentRelation
Assignment
AssignmentDeck
StudentAssignmentProgress
```

Açıklama:

Teacher-Student modülü ilk prototipte yapılmayacaktır.

Ancak ileride öğretmenlerin öğrencilere assignment vermesi, deck ataması ve öğrenci ilerlemesini takip etmesi için bu entityler eklenebilir.

---

## 12. İlk Prototip Entity İlişki Özeti

İlk prototipte temel ilişki akışı aşağıdaki gibidir:

```text
UserProfile
  -> LookupHistory
  -> UserLearningItem
  -> QuizSession

LearningItem
  -> Word
  -> Meaning
  -> LookupHistory
  -> UserLearningItem
  -> QuizQuestion

UserLearningItem
  -> UserLearningProgress

QuizSession
  -> QuizQuestion
  -> QuizOption
  -> QuizAnswer

UserLearningProgress
  -> LearningProgressHistory
```

---

## 13. Entity Tasarımında Dikkat Edilecek Kurallar

Entityler yazılırken şu kurallara dikkat edilecektir:

1. Domain entityleri EF Core bağımlılığı içermeyecektir.
2. Entityler `Wordix.Domain` katmanında duracaktır.
3. Database mapping ayarları `Wordix.Persistence` katmanında yapılacaktır.
4. Controller içinde entity oluşturma iş kuralları yazılmayacaktır.
5. Entityler mümkün olduğunca gerçek iş kavramlarını temsil edecektir.
6. Dictionary yapısı `LearningItem` üzerinden kurulacaktır.
7. Quiz sistemi ileride yeni question type destekleyecek şekilde tasarlanacaktır.
8. User ownership gerektiren entitylerde `UserProfileId` ilişkisi bulunacaktır.
9. Duplicate kayıtları engellemek için unique constraint kararları configuration dosyalarında uygulanacaktır.

---

## 14. Özet

Wordix entity tasarımının en önemli kararı şudur:

```text
Sistem sadece Word üzerine değil, LearningItem ortak çatısı üzerine kurulacaktır.
```

Bu karar sayesinde:

* Word desteği ilk prototipte çalışır.
* Phrase desteği ileride eklenebilir.
* Sentence desteği ileride eklenebilir.
* Dictionary sistemi bozulmaz.
* Quiz sistemi bozulmaz.
* Learning progress sistemi genişletilebilir kalır.

Bu yaklaşım Clean Architecture ve Open/Closed Principle ile uyumludur.
