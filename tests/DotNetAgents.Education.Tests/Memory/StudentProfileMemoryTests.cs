using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Education.Tests.Memory;

public class StudentProfileMemoryTests
{
    private readonly StudentProfileMemory _memory;

    public StudentProfileMemoryTests()
    {
        _memory = new StudentProfileMemory();
    }

    [Fact]
    public async Task SaveProfileAsync_WithValidProfile_ShouldSave()
    {
        // Arrange
        var profile = new StudentProfile
        {
            StudentId = "student-1",
            Name = "Test Student",
            GradeLevel = GradeLevel.G6_8,
            LearningStyle = "Visual",
            Interests = new[] { "Science", "Math" }
        };

        // Act
        await _memory.SaveProfileAsync(profile);

        // Assert
        var retrieved = await _memory.GetProfileAsync("student-1");
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Student");
    }

    [Fact]
    public async Task GetProfileAsync_WithNonExistentStudent_ShouldReturnNull()
    {
        // Act
        var profile = await _memory.GetProfileAsync("non-existent");

        // Assert
        profile.Should().BeNull();
    }

    [Fact]
    public async Task SaveProfileAsync_WithUpdatedProfile_ShouldUpdate()
    {
        // Arrange
        var profile = new StudentProfile
        {
            StudentId = "student-1",
            Name = "Original Name",
            GradeLevel = GradeLevel.G6_8
        };
        await _memory.SaveProfileAsync(profile);

        var updatedProfile = profile with { Name = "Updated Name" };

        // Act
        await _memory.SaveProfileAsync(updatedProfile);

        // Assert
        var retrieved = await _memory.GetProfileAsync("student-1");
        retrieved!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteProfileAsync_WithExistingProfile_ShouldDelete()
    {
        // Arrange
        var profile = new StudentProfile
        {
            StudentId = "student-1",
            Name = "Test Student",
            GradeLevel = GradeLevel.G6_8
        };
        await _memory.SaveProfileAsync(profile);

        // Act
        await _memory.DeleteProfileAsync("student-1");

        // Assert
        var retrieved = await _memory.GetProfileAsync("student-1");
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProfileAsync_WithNonExistentProfile_ShouldNotThrow()
    {
        // Act & Assert
        await _memory.Invoking(m => m.DeleteProfileAsync("non-existent"))
            .Should().NotThrowAsync();
    }
}
