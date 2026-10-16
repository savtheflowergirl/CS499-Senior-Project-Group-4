using System.ComponentModel.DataAnnotations;

namespace HorizonLunaris.Contracts;

// ----- Feature 2: progress comments -----

public record AddCommentRequest([property: Required, StringLength(2000)] string Text);

public record CommentDto(
    int Id,
    int TaskItemId,
    int AuthorId,
    string AuthorName,
    string Text,
    DateTime CreatedAtUtc);
