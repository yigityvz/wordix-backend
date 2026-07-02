using FluentValidation;

namespace Wordix.Application.Features.UserDictionary.Commands.CreateUserLearningNote;

/// <summary>
/// CreateUserLearningNoteCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - UserLearningItemId boş mu kontrol eder.
/// - NoteText boş mu kontrol eder.
/// - NoteText maksimum uzunluk sınırını kontrol eder.
/// 
/// Bu validator ne yapmaz?
/// - UserLearningItem gerçekten var mı kontrol etmez.
/// - UserLearningItem current user'a mı ait kontrol etmez.
/// - Database'e gitmez.
/// 
/// Bu kontroller handler tarafında yapılır.
/// </summary>
public sealed class CreateUserLearningNoteCommandValidator
    : AbstractValidator<CreateUserLearningNoteCommand>
{
    /// <summary>
    /// UserLearningNoteConfiguration ile uyumlu maksimum not uzunluğu.
    /// </summary>
    private const int NoteTextMaxLength = 2_000;

    public CreateUserLearningNoteCommandValidator()
    {
        RuleFor(command => command.UserLearningItemId)
            .NotEmpty()
            .WithMessage("User learning item id is required.")
            .WithErrorCode("USER_LEARNING_ITEM_ID_REQUIRED");

        RuleFor(command => command.NoteText)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Note text is required.")
            .WithErrorCode("NOTE_TEXT_REQUIRED")
            .MaximumLength(NoteTextMaxLength)
            .WithMessage($"Note text cannot exceed {NoteTextMaxLength} characters.")
            .WithErrorCode("NOTE_TEXT_TOO_LONG");
    }
}