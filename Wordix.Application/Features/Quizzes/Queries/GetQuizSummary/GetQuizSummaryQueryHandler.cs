using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Responses;
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
    private readonly ICurrentUserService _currentUserService;
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
        ICurrentUserService currentUserService,
        IQuizRepository quizRepository,
        IRepository<QuizAnswer> quizAnswerRepository)
    {
        _currentUserService = currentUserService;
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
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Backend burada UserProfile oluşturmaz, UserProfileId üretmez.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

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
            return CreateEmptySummaryResponse(quizSession);
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

        var questionResponses = orderedQuestions
            .Select(question => MapQuestionSummary(
                question,
                answersByQuestionId))
            .ToArray();

        var answeredQuestions = questionResponses
            .Where(question => question.IsAnswered)
            .ToArray();

        var correctAnswerCount = answeredQuestions
            .Count(question => question.IsCorrect == true);

        var wrongAnswerCount = answeredQuestions
            .Count(question => question.IsCorrect == false);

        var measuredResponseTimes = answeredQuestions
            .Where(question => question.QuestionResponseTimeInMilliseconds.HasValue)
            .Select(question => question.QuestionResponseTimeInMilliseconds!.Value)
            .ToArray();

        // 5. Summary response hazırlanır.
        return new QuizSummaryResponse
        {
            QuizSessionId = quizSession.Id,
            QuizType = quizSession.QuizType.ToString(),
            QuizSourceType = quizSession.QuizSourceType.ToString(),
            QuizContentMode = quizSession.QuizContentMode.ToString(),
            Status = quizSession.Status.ToString(),
            StartedAt = quizSession.StartedAt,

            TotalQuestionCount = orderedQuestions.Length,
            AnsweredQuestionCount = answeredQuestions.Length,
            UnansweredQuestionCount = orderedQuestions.Length - answeredQuestions.Length,
            CorrectAnswerCount = correctAnswerCount,
            WrongAnswerCount = wrongAnswerCount,

            AccuracyRate = CalculateRate(
                numerator: correctAnswerCount,
                denominator: answeredQuestions.Length),

            CompletionRate = CalculateRate(
                numerator: answeredQuestions.Length,
                denominator: orderedQuestions.Length),

            AverageQuestionResponseTimeInMilliseconds = CalculateAverageResponseTime(
                measuredResponseTimes),

            FastestQuestionResponseTimeInMilliseconds = measuredResponseTimes.Length == 0
                ? null
                : measuredResponseTimes.Min(),

            SlowestQuestionResponseTimeInMilliseconds = measuredResponseTimes.Length == 0
                ? null
                : measuredResponseTimes.Max(),

            Questions = questionResponses
        };
    }

    /// <summary>
    /// Soru olmayan quiz session için boş summary döner.
    /// Normalde quiz session sorusuz olmamalıdır ama defensive davranıyoruz.
    /// </summary>
    private static QuizSummaryResponse CreateEmptySummaryResponse(
        QuizSession quizSession)
    {
        return new QuizSummaryResponse
        {
            QuizSessionId = quizSession.Id,
            QuizType = quizSession.QuizType.ToString(),
            QuizSourceType = quizSession.QuizSourceType.ToString(),
            QuizContentMode = quizSession.QuizContentMode.ToString(),
            Status = quizSession.Status.ToString(),
            StartedAt = quizSession.StartedAt,
            TotalQuestionCount = 0,
            AnsweredQuestionCount = 0,
            UnansweredQuestionCount = 0,
            CorrectAnswerCount = 0,
            WrongAnswerCount = 0,
            AccuracyRate = 0,
            CompletionRate = 0,
            Questions = Array.Empty<QuizSummaryQuestionResponse>()
        };
    }

    /// <summary>
    /// Tek bir QuizQuestion için summary response üretir.
    /// </summary>
    private static QuizSummaryQuestionResponse MapQuestionSummary(
        QuizQuestion question,
        IReadOnlyDictionary<Guid, QuizAnswer> answersByQuestionId)
    {
        if (!answersByQuestionId.TryGetValue(question.Id, out var answer))
        {
            return new QuizSummaryQuestionResponse
            {
                QuizQuestionId = question.Id,
                QuestionOrder = question.DisplayOrder,
                QuestionText = question.QuestionText,
                LearningItemId = question.LearningItemId,
                IsAnswered = false,
                IsCorrect = null,
                SelectedQuizOptionId = null,
                SelectedAnswerText = null,
                CorrectAnswerText = question.CorrectAnswer,
                QuestionResponseTimeInMilliseconds = null,
                AnsweredAt = null
            };
        }

        return new QuizSummaryQuestionResponse
        {
            QuizQuestionId = question.Id,
            QuestionOrder = question.DisplayOrder,
            QuestionText = question.QuestionText,
            LearningItemId = question.LearningItemId,
            IsAnswered = true,
            IsCorrect = IsCorrectAnswer(answer.AnswerResult),
            SelectedQuizOptionId = answer.SelectedQuizOptionId,
            SelectedAnswerText = answer.UserAnswer,
            CorrectAnswerText = ResolveCorrectAnswerText(
                answer,
                question),
            QuestionResponseTimeInMilliseconds = NormalizeResponseTime(
                answer.ResponseTimeMilliseconds),
            AnsweredAt = answer.AnsweredAt
        };
    }

    /// <summary>
    /// Cevap kaydındaki doğru cevap snapshot değerini çözer.
    /// 
    /// Normalde QuizAnswer.CorrectAnswer dolu olmalıdır.
    /// Defensive davranmak için boşsa QuizQuestion.CorrectAnswer değerine döneriz.
    /// </summary>
    private static string ResolveCorrectAnswerText(
        QuizAnswer answer,
        QuizQuestion question)
    {
        return string.IsNullOrWhiteSpace(answer.CorrectAnswer)
            ? question.CorrectAnswer
            : answer.CorrectAnswer;
    }

    /// <summary>
    /// AnswerResult enum değerini doğru/yanlış bool değerine çevirir.
    /// 
    /// Enum isimlerine aşırı sıkı bağlanmamak için alias kontrolü yapıyoruz.
    /// </summary>
    private static bool IsCorrectAnswer(
        AnswerResult answerResult)
    {
        var answerResultText = answerResult.ToString();

        return string.Equals(answerResultText, "Correct", StringComparison.OrdinalIgnoreCase)
               || string.Equals(answerResultText, "Right", StringComparison.OrdinalIgnoreCase)
               || string.Equals(answerResultText, "Success", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Entity tarafında response time int olarak tutuluyor.
    /// 0 veya negatif değer ölçülmemiş kabul edilir.
    /// </summary>
    private static int? NormalizeResponseTime(
        int responseTimeMilliseconds)
    {
        return responseTimeMilliseconds <= 0
            ? null
            : responseTimeMilliseconds;
    }

    /// <summary>
    /// Yüzdelik oran hesaplar.
    /// 
    /// Örnek:
    /// 3 / 4 = 75.0
    /// </summary>
    private static double CalculateRate(
        int numerator,
        int denominator)
    {
        if (denominator == 0)
        {
            return 0;
        }

        return Math.Round(
            numerator * 100.0 / denominator,
            digits: 2);
    }

    /// <summary>
    /// Ortalama cevap süresini hesaplar.
    /// Ölçülmüş response time yoksa null döner.
    /// </summary>
    private static int? CalculateAverageResponseTime(
        IReadOnlyCollection<int> measuredResponseTimes)
    {
        if (measuredResponseTimes.Count == 0)
        {
            return null;
        }

        return (int)Math.Round(
            measuredResponseTimes.Average(),
            MidpointRounding.AwayFromZero);
    }
}