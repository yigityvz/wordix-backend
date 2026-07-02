using FluentValidation;

namespace Wordix.Application.Features.UserDictionary.Commands.UpdateUserLearningNote;

/// <summary>
/// UpdateUserLearningNoteCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - UserLearningNoteId boş mu kontrol eder.
/// - NoteText boş mu kontrol eder.
/// - NoteText maksimum uzunluk sınırını kontrol eder.
/// 
/// Bu validator ne yapmaz?
/// - Not gerçekten var mı kontrol etmez.
/// - Not current user'a mı ait kontrol etmez.
/// - Database'e gitmez.
/// 
/// Bu kontroller handler tarafında yapılır.
/// </summary>
public sealed class UpdateUserLearningNoteCommandValidator
    : AbstractValidator<UpdateUserLearningNoteCommand>
{
    /// <summary>
    /// UserLearningNoteConfiguration ile uyumlu maksimum not uzunluğu.
    /// </summary>
    private const int NoteTextMaxLength = 2_000;

    public UpdateUserLearningNoteCommandValidator()
    {
        RuleFor(command => command.UserLearningNoteId)
            .NotEmpty()
            .WithMessage("User learning note id is required.")
            .WithErrorCode("USER_LEARNING_NOTE_ID_REQUIRED");

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