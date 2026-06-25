namespace Wordix.Domain.Enums;

/// <summary>
/// Kullanıcının lookup/search sırasında girdiği metnin tipini temsil eder.
/// 
/// Örneğin kullanıcı "achieve" yazarsa Word,
/// "give up" yazarsa Phrase,
/// uzun bir metin yazarsa Sentence olarak değerlendirilebilir.
/// </summary>
public enum InputType
{
    /// <summary>
    /// Henüz analiz edilmemiş veya tipi belirlenememiş input.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Tek kelimelik input.
    /// </summary>
    Word = 1,

    /// <summary>
    /// Birden fazla kelimeden oluşan kısa ifade.
    /// </summary>
    Phrase = 2,

    /// <summary>
    /// Tam cümle veya uzun metin.
    /// </summary>
    Sentence = 3
}