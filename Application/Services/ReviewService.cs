using Application.Common;
using Application.DTOs.Reviews;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ReviewService : IReviewService
{
    private static readonly List<ReviewDto> Reviews = new();
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ILogger<ReviewService> logger)
    {
        _logger = logger;
    }

    public Task<Result<IReadOnlyList<ReviewDto>>> GetByCourseAsync(Guid CourseId)
    {
        if (CourseId == Guid.Empty)
            return Task.FromResult(Result<IReadOnlyList<ReviewDto>>.Failure("CourseId is required.", ErrorType.Validation));

        IReadOnlyList<ReviewDto> reviews = Reviews
            .Where(r => r.CourseId == CourseId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<ReviewDto>>.Success(reviews));
    }

    public Task<Result<ReviewDto>> CreateAsync(string studentId, CreateReviewDto dto)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Task.FromResult(Result<ReviewDto>.Failure("StudentId is required.", ErrorType.Validation));

        if (dto.CourseId == Guid.Empty)
            return Task.FromResult(Result<ReviewDto>.Failure("CourseId is required.", ErrorType.Validation));

        if (dto.Rating < 1 || dto.Rating > 5)
            return Task.FromResult(Result<ReviewDto>.Failure("Review rating must be between 1 and 5.", ErrorType.Validation));

        var exists = Reviews.Any(r => r.StudentId == studentId && r.CourseId == dto.CourseId);

        if (exists)
            return Task.FromResult(Result<ReviewDto>.Failure("Student can leave only one review per course.", ErrorType.Validation));

        var review = new ReviewDto
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            StudentName = "",
            CourseId = dto.CourseId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        Reviews.Add(review);
        _logger.LogInformation("Student {StudentId} reviewed course {CourseId}", studentId, dto.CourseId);

        return Task.FromResult(Result<ReviewDto>.Success(review));
    }

    public Task<Result<ReviewDto>> UpdateAsync(Guid Id, string studentId, UpdateReviewDto dto)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Task.FromResult(Result<ReviewDto>.Failure("StudentId is required.", ErrorType.Validation));

        if (dto.Rating < 1 || dto.Rating > 5)
            return Task.FromResult(Result<ReviewDto>.Failure("Review rating must be between 1 and 5.", ErrorType.Validation));

        var review = Reviews.FirstOrDefault(r => r.Id == Id);

        if (review is null)
            return Task.FromResult(Result<ReviewDto>.Failure("Review not found.", ErrorType.NotFound));

        if (review.StudentId != studentId)
            return Task.FromResult(Result<ReviewDto>.Failure("You can update only your own review.", ErrorType.Forbidden));

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;

        return Task.FromResult(Result<ReviewDto>.Success(review));
    }

    public Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(Result.Failure("UserId is required.", ErrorType.Validation));

        var review = Reviews.FirstOrDefault(r => r.Id == Id);

        if (review is null)
            return Task.FromResult(Result.Failure("Review not found.", ErrorType.NotFound));

        if (!isAdmin && review.StudentId != userId)
            return Task.FromResult(Result.Failure("You can delete only your own review.", ErrorType.Forbidden));

        Reviews.Remove(review);

        return Task.FromResult(Result.Success("Review deleted successfully."));
    }
}
