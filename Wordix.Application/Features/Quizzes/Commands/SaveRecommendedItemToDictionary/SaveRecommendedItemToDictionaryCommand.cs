using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
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
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// - Handler ownership ve dictionary kayıtları için request.KeycloakUserId değerini kullanır.
/// </summary>
public sealed record SaveRecommendedItemToDictionaryCommand(
    Guid QuizRecommendationItemId)
    : IRequest<SaveRecommendedItemToDictionaryResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}