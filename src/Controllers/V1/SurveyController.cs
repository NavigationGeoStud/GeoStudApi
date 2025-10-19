using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Survey;
using GeoStud.Api.Models;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SurveyController : ControllerBase
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<SurveyController> _logger;

    public SurveyController(GeoStudDbContext context, ILogger<SurveyController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Submit survey data for a student
    /// </summary>
    /// <param name="request">Survey data</param>
    /// <returns>Created survey response</returns>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(SurveyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitSurvey([FromBody] SurveyRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current user ID from token
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user token");
            }

            // Check if student already exists
            var username = User.Identity?.Name ?? "anonymous";
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Username == username);

            Student student;
            if (existingStudent != null)
            {
                // Update existing student
                existingStudent.AgeRange = request.AgeRange;
                existingStudent.IsStudent = request.IsStudent;
                existingStudent.Gender = request.Gender;
                existingStudent.IsLocal = request.IsLocal;
                existingStudent.Interests = JsonSerializer.Serialize(request.Interests);
                existingStudent.Budget = request.Budget;
                existingStudent.ActivityTime = request.ActivityTime;
                existingStudent.SocialPreference = request.SocialPreference;
                existingStudent.UpdatedAt = DateTime.UtcNow;
                
                student = existingStudent;
            }
            else
            {
                // Create new student
                student = new Student
                {
                    Username = User.Identity?.Name ?? "anonymous",
                    Email = User.FindFirst("email")?.Value ?? "unknown@example.com",
                    PasswordHash = "survey_user", // Not used for survey users
                    AgeRange = request.AgeRange,
                    IsStudent = request.IsStudent,
                    Gender = request.Gender,
                    IsLocal = request.IsLocal,
                    Interests = JsonSerializer.Serialize(request.Interests),
                    Budget = request.Budget,
                    ActivityTime = request.ActivityTime,
                    SocialPreference = request.SocialPreference,
                    IsActive = true
                };

                _context.Students.Add(student);
            }

            await _context.SaveChangesAsync();

            // Create individual responses for analytics
            await CreateStudentResponses(student.Id, request);

            var response = new SurveyResponse
            {
                StudentId = student.Id,
                Username = student.Username,
                Email = student.Email,
                AgeRange = student.AgeRange,
                IsStudent = student.IsStudent,
                Gender = student.Gender,
                IsLocal = student.IsLocal,
                Interests = request.Interests,
                Budget = student.Budget,
                ActivityTime = student.ActivityTime,
                SocialPreference = student.SocialPreference,
                CreatedAt = student.CreatedAt
            };

            return CreatedAtAction(nameof(GetSurvey), new { id = student.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting survey");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get survey data by student ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Survey response</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SurveyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSurvey(int id)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (student == null)
            {
                return NotFound();
            }

            var interests = string.IsNullOrEmpty(student.Interests) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(student.Interests) ?? new List<string>();

            var response = new SurveyResponse
            {
                StudentId = student.Id,
                Username = student.Username,
                Email = student.Email,
                AgeRange = student.AgeRange,
                IsStudent = student.IsStudent,
                Gender = student.Gender,
                IsLocal = student.IsLocal,
                Interests = interests,
                Budget = student.Budget,
                ActivityTime = student.ActivityTime,
                SocialPreference = student.SocialPreference,
                CreatedAt = student.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving survey {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all survey responses (for analytics)
    /// </summary>
    /// <returns>List of survey responses</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SurveyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllSurveys()
    {
        try
        {
            var students = await _context.Students
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var responses = students.Select(s => new SurveyResponse
            {
                StudentId = s.Id,
                Username = s.Username,
                Email = s.Email,
                AgeRange = s.AgeRange,
                IsStudent = s.IsStudent,
                Gender = s.Gender,
                IsLocal = s.IsLocal,
                Interests = string.IsNullOrEmpty(s.Interests) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(s.Interests) ?? new List<string>(),
                Budget = s.Budget,
                ActivityTime = s.ActivityTime,
                SocialPreference = s.SocialPreference,
                CreatedAt = s.CreatedAt
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all surveys");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task CreateStudentResponses(int studentId, SurveyRequest request)
    {
        var responses = new List<StudentResponse>
        {
            new() { StudentId = studentId, Question = "Age Range", Answer = request.AgeRange, Category = "Demographics" },
            new() { StudentId = studentId, Question = "Is Student", Answer = request.IsStudent.ToString(), Category = "Demographics" },
            new() { StudentId = studentId, Question = "Gender", Answer = request.Gender, Category = "Demographics" },
            new() { StudentId = studentId, Question = "Is Local", Answer = request.IsLocal.ToString(), Category = "Demographics" },
            new() { StudentId = studentId, Question = "Interests", Answer = JsonSerializer.Serialize(request.Interests), Category = "Preferences" },
            new() { StudentId = studentId, Question = "Budget", Answer = request.Budget, Category = "Behavior" },
            new() { StudentId = studentId, Question = "Activity Time", Answer = request.ActivityTime, Category = "Behavior" },
            new() { StudentId = studentId, Question = "Social Preference", Answer = request.SocialPreference, Category = "Behavior" }
        };

        _context.StudentResponses.AddRange(responses);
        await _context.SaveChangesAsync();
    }
}
