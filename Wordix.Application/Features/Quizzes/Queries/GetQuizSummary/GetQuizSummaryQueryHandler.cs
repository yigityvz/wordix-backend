using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Dtos.Responses;
using Wordix.Application.Features.Quizzes.Mappers;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Queries.GetQuizSummary;

/// <summary>
/// GetQuizSummaryQuery isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - QuizSession var mı ve current user'a ait mi kontrol eder.
/// - QuizQuestion kayıtlarını çeker.
/// - QuizAnswer kayıtlarını çeker.
/// - Summary istatistiklerini hesaplar.
/// - Soru bazlı summary listesi döner.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Quiz summary ownership kontrolü KeycloakUserId ile yapılır.
/// 
/// Bu handler ne yapmaz?
/// - QuizAnswer oluşturmaz.
/// - Progress güncellemez.
/// - SaveChanges çağırmaz.
/// - DbContext kullanmaz.
/// - Controller logic'i içermez.
/// </summary>
public sealed class GetQuizSummaryQueryHandler
    : IRequestHandler<GetQuizSummaryQuery, QuizSummaryResponse>
{
    private readonly IQuizRepository _quizRepository;
    private readonly IRepository<QuizAnswer> _quizAnswerRepository;

    /// <summary>
    /// Handler ihtiyacı olan servis ve repository abstraction'larını DI üzerinden alır.
    /// 
    /// Burada DbContext yok.
    /// Burada HttpContext yok.
    /// Burada controller yok.
    /// 
    /// Current user bilgisi ICurrentUserService üzerinden alınır.
    /// Quiz session ownership kontrolü IQuizRepository üzerinden yapılır.
    /// </summary>
    public GetQuizSummaryQueryHandler(
        IQuizRepository quizRepository,
        IRepository<QuizAnswer> quizAnswerRepository)
    {
        _quizRepository = quizRepository;
        _quizAnswerRepository = quizAnswerRepository;
    }

    /// <summary>
    /// Quiz summary bilgisini üretir.
    /// </summary>
    public async Task<QuizSummaryResponse> Handle(
        GetQuizSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini request üzerinden alıyoruz.
        //
        // Bu değer client'tan gelmez.
        // CurrentUserBehavior, MediatR pipeline içinde token'dan okuyup request'e yazar.
        // Quiz summary ownership kontrolü bu değerle yapılır.
        var keycloakUserId = request.KeycloakUserId;

        // 2. QuizSession var mı ve current user'a ait mi kontrol ediyoruz.
        //
        // Eski yapı:
        // QuizSession.UserProfileId == userProfile.Id
        //
        // Yeni yapı:
        // QuizSession.KeycloakUserId == keycloakUserId
        //
        // Repository null dönerse iki ihtimal vardır:
        // - Böyle bir QuizSession yoktur.
        // - QuizSession vardır ama current user'a ait değildir.
        //
        // Güvenlik açısından ikisini de NotFound gibi davranmak kabul edilebilir.
        // Böylece kullanıcı başka bir session id tahmin ettiğinde var/yok bilgisini öğrenemez.
        var quizSession = await _quizRepository.GetSessionByIdForUserAsync(
            request.QuizSessionId,
            keycloakUserId,
            cancellationToken);

        if (quizSession is null)
        {
            throw new NotFoundException(
                "Quiz session",
                request.QuizSessionId);
        }

        // 3. Bu session'a ait sorular çekilir.
        // Bu sorgu read-only olduğu için IQuizRepository tarafında AsNoTracking ile çalışır.
        var quizQuestions = await _quizRepository.GetQuestionsBySessionAsync(
            quizSession.Id,
            cancellationToken);

        var orderedQuestions = quizQuestions
            .OrderBy(question => question.DisplayOrder)
            .ToArray();

        if (orderedQuestions.Length == 0)
        {
            return QuizMapper.ToQuizSummaryResponse(
                quizSession: quizSession,
                orderedQuestions: Array.Empty<QuizQuestion>(),
                answersByQuestionId: new Dictionary<Guid, QuizAnswer>());
        }

        var questionIds = orderedQuestions
            .Select(question => question.Id)
            .ToArray();

        // 4. Bu session sorularına current user tarafından verilmiş cevaplar çekilir.
        //
        // QuizAnswer doğrudan QuizSessionId tutmadığı için QuizQuestionId üzerinden ilişki kuruyoruz.
        // Yeni mimaride kullanıcı filtresi UserProfileId ile değil KeycloakUserId ile yapılır.
        var quizAnswers = await _quizAnswerRepository.ListAsync(
            answer => answer.KeycloakUserId == keycloakUserId
                      && questionIds.Contains(answer.QuizQuestionId),
            cancellationToken);

        var answersByQuestionId = quizAnswers
            .GroupBy(answer => answer.QuizQuestionId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(answer => answer.AnsweredAt)
                    .First());

        return QuizMapper.ToQuizSummaryResponse(
            quizSession: quizSession,
            orderedQuestions: orderedQuestions,
            answersByQuestionId: answersByQuestionId);
    }

}
