using FluentValidation;

namespace Wordix.Application.Features.UserDictionary.Commands.DeleteUserLearningNote;

/// <summary>
/// DeleteUserLearningNoteCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator sadece NoteId boş mu diye kontrol eder.
/// 
/// Şu kontrolleri yapmaz:
/// - Note gerçekten var mı?
/// - Note current user'a mı ait?
/// 
/// Bu kontroller handler tarafında repository ile yapılır.
/// </summary>
public sealed class DeleteUserLearningNoteCommandValidator
    : AbstractValidator<DeleteUserLearningNoteCommand>
{
    public DeleteUserLearningNoteCommandValidator()
    {
        RuleFor(command => command.UserLearningNoteId)
            .NotEmpty()
            .WithMessage("User learning note id is required.")
            .WithErrorCode("USER_LEARNING_NOTE_ID_REQUIRED");
    }
}