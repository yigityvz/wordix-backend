using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının kendi dictionary item'ına verdiği kişisel işareti temsil eder.
/// 
/// Bu entity global LearningItem'a bağlı değildir.
/// Çünkü flag kullanıcıya özeldir.
/// 
/// Doğru ilişki:
/// UserLearningItem -> UserLearningFlag
/// 
/// Örnek:
/// - Favorite
/// - Difficult
/// - WantMorePractice
/// 
/// Bir kullanıcı için "achieve" Difficult olabilir,
/// başka bir kullanıcı için olmayabilir.
/// </summary>
public class UserLearningFlag : BaseEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// EF Core database'den kayıt okurken bu constructor'ı kullanabilir.
    /// Dışarıdan boş ve geçersiz flag oluşturulmasını istemediğimiz için protected bırakıyoruz.
    /// </summary>
    protected UserLearningFlag()
    {
    }

    /// <summary>
    /// Yeni kullanıcı öğrenme flag'i oluşturur.
    /// 
    /// userLearningItemId:
    /// - Kullanıcının kişisel dictionary item kaydıdır.
    /// - Global LearningItemId değildir.
    /// 
    /// flagType:
    /// - Favorite, Difficult gibi işaret tipidir.
    /// </summary>
    public UserLearningFlag(
        Guid userLearningItemId,
        UserLearningFlagType flagType)
    {
        if (userLearningItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserLearningItemId boş Guid olamaz.",
                nameof(userLearningItemId));
        }

        if (!Enum.IsDefined(flagType))
        {
            throw new ArgumentException(
                "Geçersiz UserLearningFlagType değeri.",
                nameof(flagType));
        }

        UserLearningItemId = userLearningItemId;
        FlagType = flagType;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Flag'in bağlı olduğu kullanıcı dictionary item id değeridir.
    /// 
    /// Ownership doğrudan burada KeycloakUserId tutularak değil,
    /// UserLearningItem -> KeycloakUserId zinciriyle doğrulanır.
    /// </summary>
    public Guid UserLearningItemId { get; private set; }

    /// <summary>
    /// Kullanıcının verdiği flag tipidir.
    /// 
    /// Örnek:
    /// Favorite
    /// Difficult
    /// WantMorePractice
    /// Ignored
    /// </summary>
    public UserLearningFlagType FlagType { get; private set; }

    /// <summary>
    /// Flag'in oluşturulduğu zamandır.
    /// 
    /// Flag update edilen bir yapı değildir.
    /// Kullanıcı flag'i kaldırmak isterse kayıt silinir.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
}