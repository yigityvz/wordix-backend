using MediatR;
using Wordix.Application.Features.Quizzes.Dtos.Responses;

namespace Wordix.Application.Features.Quizzes.Commands.SaveRecommendedItemToDictionary;

/// <summary>
/// Sistem önerisi olarak gelen quiz item'ını kullanıcının dictionary'sine ekleme isteğini temsil eder.
/// 
/// Neden command?
/// - UserLearningItem oluşturabilir.
/// - UserLearningProgress oluşturabilir.
/// - LearningProgressHistory oluşturabilir.
/// - QuizRecommendationItem durumunu günceller.
/// - SearchSuggestionLog durumunu günceller.
/// 
/// Yani sistem durumunu değiştiren bir use-case'tir.
/// </summary>
public sealed record SaveRecommendedItemToDictionaryCommand(
    Guid QuizRecommendationItemId)
    : IRequest<SaveRecommendedItemToDictionaryResponse>;