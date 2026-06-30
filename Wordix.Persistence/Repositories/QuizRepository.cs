using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// Quiz akışları için özel repository implementasyonudur.
/// 
/// QuizSession, QuizQuestion, QuizOption ve QuizAnswer entity'leriyle ilgili
/// özel sorgular burada toplanır.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Quiz sahiplik kontrolleri KeycloakUserId üzerinden yapılır.
/// </summary>
public class QuizRepository : IQuizRepository
{
    private readonly WordixDbContext _dbContext;

    public QuizRepository(WordixDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Belirli bir quiz session'ı kullanıcıya aitlik kontrolüyle getirir.
    /// 
    /// Bu method ownership kontrolü için önemlidir.
    /// Kullanıcı başkasına ait quiz session'a erişmemelidir.
    /// 
    /// Eski yapı:
    /// QuizSession.UserProfileId == userProfileId
    /// 
    /// Yeni yapı:
    /// QuizSession.KeycloakUserId == keycloakUserId
    /// </summary>
    public async Task<QuizSession?> GetSessionByIdForUserAsync(
        Guid quizSessionId,
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        if (quizSessionId == Guid.Empty || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return null;
        }

        var normalizedKeycloakUserId = keycloakUserId.Trim();

        return await _dbContext.QuizSessions
            .FirstOrDefaultAsync(session =>
                session.Id == quizSessionId &&
                session.KeycloakUserId == normalizedKeycloakUserId,
                cancellationToken);
    }

    /// <summary>
    /// Bir quiz session'a ait soruları display order'a göre getirir.
    /// 
    /// AsNoTracking kullanıyoruz çünkü bu method genelde quiz görüntüleme/soru listeleme
    /// gibi read-only senaryolarda kullanılacaktır.
    /// </summary>
    public async Task<IReadOnlyList<QuizQuestion>> GetQuestionsBySessionAsync(
        Guid quizSessionId,
        CancellationToken cancellationToken = default)
    {
        if (quizSessionId == Guid.Empty)
        {
            return Array.Empty<QuizQuestion>();
        }

        return await _dbContext.QuizQuestions
            .AsNoTracking()
            .Where(question => question.QuizSessionId == quizSessionId)
            .OrderBy(question => question.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Bir quiz sorusuna ait seçenekleri display order'a göre getirir.
    /// 
    /// Test quizlerde kullanıcıya gösterilecek seçenekler bu methodla alınabilir.
    /// </summary>
    public async Task<IReadOnlyList<QuizOption>> GetOptionsByQuestionAsync(
        Guid quizQuestionId,
        CancellationToken cancellationToken = default)
    {
        if (quizQuestionId == Guid.Empty)
        {
            return Array.Empty<QuizOption>();
        }

        return await _dbContext.QuizOptions
            .AsNoTracking()
            .Where(option => option.QuizQuestionId == quizQuestionId)
            .OrderBy(option => option.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Id'ye göre quiz option getirir.
    /// 
    /// Tracking açık bırakıldı.
    /// Şu an option üzerinde değişiklik yapmayacağız ama cevap değerlendirme sırasında
    /// seçilen option'ın IsCorrect ve OptionText bilgilerini kullanacağız.
    /// </summary>
    public async Task<QuizOption?> GetOptionByIdAsync(
        Guid quizOptionId,
        CancellationToken cancellationToken = default)
    {
        if (quizOptionId == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.QuizOptions
            .FirstOrDefaultAsync(
                option => option.Id == quizOptionId,
                cancellationToken);
    }

    /// <summary>
    /// Kullanıcı belirli bir soruya daha önce cevap vermiş mi kontrol eder.
    /// 
    /// Bu method aynı soruya ikinci kez cevap gönderilmesini engellemek için kullanılabilir.
    /// 
    /// Eski yapı:
    /// QuizAnswer.UserProfileId == userProfileId
    /// 
    /// Yeni yapı:
    /// QuizAnswer.KeycloakUserId == keycloakUserId
    /// </summary>
    public async Task<bool> HasAnswerForQuestionAsync(
        Guid quizQuestionId,
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        if (quizQuestionId == Guid.Empty || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return false;
        }

        var normalizedKeycloakUserId = keycloakUserId.Trim();

        return await _dbContext.QuizAnswers
            .AsNoTracking()
            .AnyAsync(answer =>
                answer.QuizQuestionId == quizQuestionId &&
                answer.KeycloakUserId == normalizedKeycloakUserId,
                cancellationToken);
    }
}