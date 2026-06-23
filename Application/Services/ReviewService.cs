using Application.Common;
using Application.DTOs.Reviews;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IReviewRepository reviewRepository,
        ICacheService cache,
        ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByCourseAsync(Guid CourseId)
    {
        if (CourseId == Guid.Empty)
            return Result<IReadOnlyList<ReviewDto>>.Failure("CourseId is required.", ErrorType.Validation);

        var courseExists = await _reviewRepository.CourseExistsAsync(CourseId);
        if (!courseExists)
            return Result<IReadOnlyList<ReviewDto>>.Failure("Course not found.", ErrorType.NotFound);

        var reviews = await _reviewRepository.GetByCourseAsync(CourseId);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(MapToDto).ToList());
    }

    public async Task<Result<ReviewDto>> CreateAsync(string studentId, CreateReviewDto dto)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Result<ReviewDto>.Failure("StudentId is required.", ErrorType.Validation);

        if (dto.CourseId == Guid.Empty)
            return Result<ReviewDto>.Failure("CourseId is required.", ErrorType.Validation);

        if (dto.Rating < 1 || dto.Rating > 5)
            return Result<ReviewDto>.Failure("Review rating must be between 1 and 5.", ErrorType.Validation);

        var courseExists = await _reviewRepository.CourseExistsAsync(dto.CourseId);
        if (!courseExists)
            return Result<ReviewDto>.Failure("Course not found.", ErrorType.NotFound);

        var enrolled = await _reviewRepository.IsStudentEnrolledAsync(studentId, dto.CourseId);
        if (!enrolled)
            return Result<ReviewDto>.Failure("Student can review only enrolled course.", ErrorType.Forbidden);

        var exists = await _reviewRepository.ExistsAsync(studentId, dto.CourseId);
        if (exists)
            return Result<ReviewDto>.Failure("Student can leave only one review per course.", ErrorType.Validation);

        var review = new Review
        {
            StudentId = studentId,
            CourseId = dto.CourseId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        await _reviewRepository.CreateAsync(review);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        _logger.LogInformation("Student {StudentId} reviewed course {CourseId}", studentId, dto.CourseId);

        return Result<ReviewDto>.Success(MapToDto(review));
    }

    public async Task<Result<ReviewDto>> UpdateAsync(Guid courseId, Guid Id, string studentId, UpdateReviewDto dto)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Result<ReviewDto>.Failure("StudentId is required.", ErrorType.Validation);

        if (dto.Rating < 1 || dto.Rating > 5)
            return Result<ReviewDto>.Failure("Review rating must be between 1 and 5.", ErrorType.Validation);

        var review = await _reviewRepository.GetByCourseAndIdAsync(courseId, Id);

        if (review is null)
            return Result<ReviewDto>.Failure("Review not found.", ErrorType.NotFound);

        if (review.StudentId != studentId)
            return Result<ReviewDto>.Failure("You can update only your own review.", ErrorType.Forbidden);

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result<ReviewDto>.Success(MapToDto(review));
    }

    public async Task<Result> DeleteAsync(Guid courseId, Guid Id, string userId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure("UserId is required.", ErrorType.Validation);

        var review = await _reviewRepository.GetByCourseAndIdAsync(courseId, Id);

        if (review is null)
            return Result.Failure("Review not found.", ErrorType.NotFound);

        if (!isAdmin && review.StudentId != userId)
            return Result.Failure("You can delete only your own review.", ErrorType.Forbidden);

        await _reviewRepository.DeleteAsync(review);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result.Success("Review deleted successfully.");
    }

    private static ReviewDto MapToDto(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            StudentId = review.StudentId,
            StudentName = review.Student?.FullName ?? "",
            CourseId = review.CourseId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}
