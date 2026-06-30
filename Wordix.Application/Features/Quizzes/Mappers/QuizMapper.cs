using Wordix.Application.Common.Exceptions;
using Wordix.Application.Features.Quizzes.Commands.StartQuiz;
using Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;
using Wordix.Application.Features.Quizzes.Dtos.Requests;
using Wordix.Application.Features.Quizzes.Dtos.Responses;
using Wordix.Application.Features.Quizzes.Models;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Mappers;

/// <summary>
/// Quizzes feature'ına ait explicit mapping işlemlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Bu sınıf neden var?
/// - Controller içinde Request DTO → Command dönüşümü yapmak istemiyoruz.
/// - Handler içinde Response DTO propertylerini tek tek dizmek istemiyoruz.
/// - AutoMapper/Mapster gibi otomatik mapping kütüphanesi kullanmak istemiyoruz.
/// - Mapping kurallarını feature seviyesinde, açık ve kontrollü biçimde topluyoruz.
/// 
/// Bu yaklaşım:
/// - Magic mapping değildir.
/// - Hangi alanın nereye gittiği açıkça görülür.
/// - Controller'ı sadeleştirir.
/// - Handler'ı use-case akışına odaklı tutar.
/// - Mapping kodunu feature seviyesinde tek yerde toplar.
/// </summary>
public static class QuizMapper
{
    /// <summary>
    /// API request DTO'sunu StartQuizCommand modeline dönüştürür.
    /// 
    /// Dikkat:
    /// Controller içinde null body kontrolü yapmıyoruz.
    /// Eğer request null gelirse string alanlar boş kalır, QuestionCount 0 olur.
    /// StartQuizCommandValidator bu değerleri ValidationBehavior üzerinden yakalar.
    /// </summary>
    public static StartQuizCommand ToStartQuizCommand(
        StartQuizRequest? request)
    {
        return new StartQuizCommand
        {
            QuizType = request?.QuizType ?? string.Empty,
            QuizSourceType = request?.QuizSourceType ?? string.Empty,
            QuizContentMode = request?.QuizContentMode ?? string.Empty,
            QuestionCount = request?.QuestionCount ?? 0
        };
    }

    /// <summary>
    /// Route'tan gelen quizSessionId ve API request DTO'sunu
    /// SubmitQuizAnswerCommand modeline dönüştürür.
    /// </summary>
    public static SubmitQuizAnswerCommand ToSubmitQuizAnswerCommand(
        Guid quizSessionId,
        SubmitQuizAnswerRequest? request)
    {
        return new SubmitQuizAnswerCommand(
            quizSessionId,
            request?.SelectedQuizOptionId ?? Guid.Empty,
            request?.QuestionResponseTimeInMilliseconds);
    }

    /// <summary>
    /// QuizOption entity'sini API response DTO'suna dönüştürür.
    /// 
    /// Dikkat:
    /// Response'a IsCorrect koymuyoruz.
    /// Doğru cevap bilgisi sadece backend/database tarafında kalır.
    /// </summary>
    public static QuizOptionResponse ToQuizOptionResponse(
        QuizOption quizOption)
    {
        ArgumentNullException.ThrowIfNull(quizOption);

        return new QuizOptionResponse
        {
            QuizOptionId = quizOption.Id,
            DisplayOrder = quizOption.DisplayOrder,
            OptionText = quizOption.OptionText
        };
    }

    /// <summary>
    /// QuizQuestion entity'si ve generator ara modelinden API question response üretir.
    /// 
    /// Neden generatedQuestion da alıyoruz?
    /// - WordId şu an QuizQuestion entity üzerinde yok.
    /// - Generator'ın daha açıklayıcı QuestionType bilgisi response'ta gösteriliyor.
    /// </summary>
    public static QuizQuestionResponse ToQuizQuestionResponse(
        QuizQuestion quizQuestion,
        GeneratedQuizQuestion generatedQuestion,
        IReadOnlyCollection<QuizOptionResponse> optionResponses)
    {
        ArgumentNullException.ThrowIfNull(quizQuestion);
        ArgumentNullException.ThrowIfNull(generatedQuestion);
        ArgumentNullException.ThrowIfNull(optionResponses);

        return new QuizQuestionResponse
        {
            QuizQuestionId = quizQuestion.Id,
            QuestionOrder = quizQuestion.DisplayOrder,
            QuestionText = quizQuestion.QuestionText,
            LearningItemId = quizQuestion.LearningItemId,
            WordId = generatedQuestion.WordId,
            ItemType = LearningItemType.Word.ToString(),

            // API response'ta generator'ın daha açıklayıcı question type değerini döndürüyoruz.
            // Database tarafında ise QuizQuestion.QuestionType domain enum olarak saklanır.
            QuestionType = generatedQuestion.QuestionType,

            Options = optionResponses
                .OrderBy(option => option.DisplayOrder)
                .ToArray()
        };
    }

    /// <summary>
    /// Quiz başlatma use-case'i sonunda API'ye dönecek response modelini üretir.
    /// </summary>
    public static StartQuizResponse ToStartQuizResponse(
        StartQuizCommand request,
        QuizSession quizSession,
        IReadOnlyCollection<QuizQuestionResponse> questionResponses)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(quizSession);
        ArgumentNullException.ThrowIfNull(questionResponses);

        return new StartQuizResponse
        {
            QuizSessionId = quizSession.Id,
            QuizType = request.QuizType.Trim(),
            QuizSourceType = request.QuizSourceType.Trim(),
            QuizContentMode = request.QuizContentMode.Trim(),
            QuestionCount = questionResponses.Count,
            StartedAt = quizSession.StartedAt,
            Status = quizSession.Status.ToString(),
            Questions = questionResponses
        };
    }

    /// <summary>
    /// GeneratedQuizQuestion içindeki doğru seçeneğin metnini bulur.
    /// 
    /// QuizQuestion entity, doğru cevabı string olarak saklıyor.
    /// Bu yüzden doğru option'ın OptionText değerini CorrectAnswer alanına yazıyoruz.
    /// </summary>
    public static string ResolveCorrectAnswerText(
        GeneratedQuizQuestion generatedQuestion)
    {
        ArgumentNullException.ThrowIfNull(generatedQuestion);

        var correctOption = generatedQuestion.Options
            .FirstOrDefault(option => option.IsCorrect);

        if (correctOption is null)
        {
            throw new BusinessRuleException(
                "Generated quiz question does not contain a correct option.",
                "GENERATED_QUESTION_HAS_NO_CORRECT_OPTION");
        }

        return correctOption.OptionText;
    }

    /// <summary>
    /// Generator'dan gelen question type değerini domain QuestionType enum'una çevirir.
    /// 
    /// Bu mapping neden burada?
    /// - GeneratedQuizQuestion application ara modelidir.
    /// - QuizQuestion domain entity'si QuestionType enum bekler.
    /// - Bu iki model arasındaki dönüşüm handler içinde dağılmamalıdır.
    /// </summary>
    public static QuestionType ToDomainQuestionType(
        string questionType)
    {
        if (string.IsNullOrWhiteSpace(questionType))
        {
            throw new BusinessRuleException(
                "Generated question type is required.",
                "QUESTION_TYPE_REQUIRED");
        }

        if (Enum.TryParse<QuestionType>(
                questionType.Trim(),
                ignoreCase: true,
                out var parsedQuestionType))
        {
            return parsedQuestionType;
        }

        if (string.Equals(
                questionType.Trim(),
                "MultipleChoiceTranslation",
                StringComparison.OrdinalIgnoreCase))
        {
            var multipleChoiceAliases = new[]
            {
                "MultipleChoice",
                "Test",
                "Translation",
                "WordTranslation",
                "WordToMeaning",
                "MeaningSelection",
                "MultipleChoiceMeaning",
                "MultipleChoiceWordTranslation"
            };

            foreach (var alias in multipleChoiceAliases)
            {
                if (Enum.TryParse<QuestionType>(
                        alias,
                        ignoreCase: true,
                        out var aliasQuestionType))
                {
                    return aliasQuestionType;
                }
            }
        }

        var supportedQuestionTypes = string.Join(
            ", ",
            Enum.GetNames<QuestionType>());

        throw new BusinessRuleException(
            $"Question type '{questionType}' is not supported. Supported domain question types: {supportedQuestionTypes}.",
            "QUESTION_TYPE_NOT_SUPPORTED");
    }

    /// <summary>
    /// Quiz cevaplama use-case'i sonunda API'ye dönecek response modelini üretir.
    /// </summary>
    public static SubmitQuizAnswerResponse ToSubmitQuizAnswerResponse(
        QuizAnswer quizAnswer,
        QuizSession quizSession,
        QuizQuestion quizQuestion,
        QuizOption selectedOption,
        QuizAnswerEvaluationResult evaluationResult,
        LearningProgressUpdateResult progressUpdateResult)
    {
        ArgumentNullException.ThrowIfNull(quizAnswer);
        ArgumentNullException.ThrowIfNull(quizSession);
        ArgumentNullException.ThrowIfNull(quizQuestion);
        ArgumentNullException.ThrowIfNull(selectedOption);
        ArgumentNullException.ThrowIfNull(evaluationResult);
        ArgumentNullException.ThrowIfNull(progressUpdateResult);

        return new SubmitQuizAnswerResponse
        {
            QuizAnswerId = quizAnswer.Id,
            QuizSessionId = quizSession.Id,
            QuizQuestionId = quizQuestion.Id,
            SelectedQuizOptionId = selectedOption.Id,
            IsCorrect = evaluationResult.IsCorrect,
            SelectedOptionText = evaluationResult.SelectedOptionText,
            CorrectAnswerText = evaluationResult.CorrectAnswerText,
            QuestionResponseTimeInMilliseconds = evaluationResult.QuestionResponseTimeInMilliseconds,
            AnsweredAt = quizAnswer.AnsweredAt,

            CorrectCount = progressUpdateResult.CorrectCount,
            WrongCount = progressUpdateResult.WrongCount,
            ConsecutiveCorrectCount = progressUpdateResult.ConsecutiveCorrectCount,
            ConsecutiveWrongCount = progressUpdateResult.ConsecutiveWrongCount,

            PreviousLearningStatus = progressUpdateResult.PreviousLearningStatus.ToString(),
            CurrentLearningStatus = progressUpdateResult.NewLearningStatus.ToString(),

            PreviousConfidenceScore = progressUpdateResult.PreviousConfidenceScore,
            CurrentConfidenceScore = progressUpdateResult.NewConfidenceScore,

            NextReviewDate = progressUpdateResult.NextReviewDate
        };
    }

    /// <summary>
    /// Quiz session ve soru/cevap bilgilerinden summary response üretir.
    /// 
    /// Handler sadece verileri toplar.
    /// Summary istatistiklerini ve response DTO mapping'ini bu method yapar.
    /// </summary>
    public static QuizSummaryResponse ToQuizSummaryResponse(
        QuizSession quizSession,
        IReadOnlyCollection<QuizQuestion> orderedQuestions,
        IReadOnlyDictionary<Guid, QuizAnswer> answersByQuestionId)
    {
        ArgumentNullException.ThrowIfNull(quizSession);
        ArgumentNullException.ThrowIfNull(orderedQuestions);
        ArgumentNullException.ThrowIfNull(answersByQuestionId);

        var questionResponses = orderedQuestions
            .OrderBy(question => question.DisplayOrder)
            .Select(question => ToQuizSummaryQuestionResponse(
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

        return new QuizSummaryResponse
        {
            QuizSessionId = quizSession.Id,
            QuizType = quizSession.QuizType.ToString(),
            QuizSourceType = quizSession.QuizSourceType.ToString(),
            QuizContentMode = quizSession.QuizContentMode.ToString(),
            Status = quizSession.Status.ToString(),
            StartedAt = quizSession.StartedAt,

            TotalQuestionCount = orderedQuestions.Count,
            AnsweredQuestionCount = answeredQuestions.Length,
            UnansweredQuestionCount = orderedQuestions.Count - answeredQuestions.Length,
            CorrectAnswerCount = correctAnswerCount,
            WrongAnswerCount = wrongAnswerCount,

            AccuracyRate = CalculateRate(
                numerator: correctAnswerCount,
                denominator: answeredQuestions.Length),

            CompletionRate = CalculateRate(
                numerator: answeredQuestions.Length,
                denominator: orderedQuestions.Count),

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
    /// Tek bir QuizQuestion için summary question response üretir.
    /// </summary>
    public static QuizSummaryQuestionResponse ToQuizSummaryQuestionResponse(
        QuizQuestion question,
        IReadOnlyDictionary<Guid, QuizAnswer> answersByQuestionId)
    {
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(answersByQuestionId);

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