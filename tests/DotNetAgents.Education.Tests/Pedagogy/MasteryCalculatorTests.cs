using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Education.Tests.Pedagogy;

public class MasteryCalculatorTests
{
    private readonly MasteryCalculator _calculator;

    public MasteryCalculatorTests()
    {
        _calculator = new MasteryCalculator();
    }

    [Fact]
    public void CalculateMastery_WithNoHistory_ShouldReturnNovice()
    {
        // Arrange
        var concept = new ConceptId("test-concept", SubjectArea.Science, GradeLevel.G6_8);
        var history = new List<AssessmentResult>();

        // Act
        var mastery = _calculator.CalculateMastery(concept, history);

        // Assert
        mastery.Should().Be(MasteryLevel.Novice);
    }

    [Fact]
    public void CalculateMastery_WithHighScores_ShouldReturnAdvanced()
    {
        // Arrange
        var concept = new ConceptId("test-concept", SubjectArea.Science, GradeLevel.G6_8);
        var history = new List<AssessmentResult>
        {
            new() { Score = 85, Timestamp = DateTimeOffset.UtcNow.AddDays(-7), AssessmentId = "assess-1" },
            new() { Score = 90, Timestamp = DateTimeOffset.UtcNow.AddDays(-3), AssessmentId = "assess-2" },
            new() { Score = 88, Timestamp = DateTimeOffset.UtcNow.AddDays(-1), AssessmentId = "assess-3" }
        };

        // Act
        var mastery = _calculator.CalculateMastery(concept, history);

        // Assert
        mastery.Should().Be(MasteryLevel.Advanced);
    }

    [Fact]
    public void CalculateMastery_WithLowScores_ShouldReturnNovice()
    {
        // Arrange
        var concept = new ConceptId("test-concept", SubjectArea.Science, GradeLevel.G6_8);
        var history = new List<AssessmentResult>
        {
            new() { Score = 30, Timestamp = DateTimeOffset.UtcNow.AddDays(-7), AssessmentId = "assess-1" },
            new() { Score = 35, Timestamp = DateTimeOffset.UtcNow.AddDays(-3), AssessmentId = "assess-2" }
        };

        // Act
        var mastery = _calculator.CalculateMastery(concept, history);

        // Assert
        mastery.Should().Be(MasteryLevel.Novice);
    }

    [Fact]
    public void MeetsPrerequisites_WithNoPrerequisites_ShouldReturnTrue()
    {
        // Arrange
        var targetConcept = new ConceptId("target", SubjectArea.Science, GradeLevel.G6_8);
        var studentMastery = new Dictionary<ConceptId, MasteryLevel>
        {
            [targetConcept] = MasteryLevel.Proficient
        };

        // Act
        var meetsPrereqs = _calculator.MeetsPrerequisites(targetConcept, studentMastery);

        // Assert
        meetsPrereqs.Should().BeTrue();
    }

    [Fact]
    public void MeetsPrerequisites_WithPrerequisitesMet_ShouldReturnTrue()
    {
        // Arrange
        var prerequisite = new ConceptId("prereq", SubjectArea.Science, GradeLevel.G6_8);
        var targetConcept = new ConceptId("target", SubjectArea.Science, GradeLevel.G6_8);
        var prerequisiteGraph = new Dictionary<ConceptId, IReadOnlyList<ConceptId>>
        {
            [targetConcept] = new[] { prerequisite }
        };
        var calculator = new MasteryCalculator(prerequisiteGraph);
        var studentMastery = new Dictionary<ConceptId, MasteryLevel>
        {
            [prerequisite] = MasteryLevel.Proficient,
            [targetConcept] = MasteryLevel.Novice
        };

        // Act
        var meetsPrereqs = calculator.MeetsPrerequisites(targetConcept, studentMastery);

        // Assert
        meetsPrereqs.Should().BeTrue();
    }

    [Fact]
    public void MeetsPrerequisites_WithPrerequisitesNotMet_ShouldReturnFalse()
    {
        // Arrange
        var prerequisite = new ConceptId("prereq", SubjectArea.Science, GradeLevel.G6_8);
        var targetConcept = new ConceptId("target", SubjectArea.Science, GradeLevel.G6_8);
        var prerequisiteGraph = new Dictionary<ConceptId, IReadOnlyList<ConceptId>>
        {
            [targetConcept] = new[] { prerequisite }
        };
        var calculator = new MasteryCalculator(prerequisiteGraph);
        var studentMastery = new Dictionary<ConceptId, MasteryLevel>
        {
            [prerequisite] = MasteryLevel.Developing, // Not proficient
            [targetConcept] = MasteryLevel.Novice
        };

        // Act
        var meetsPrereqs = calculator.MeetsPrerequisites(targetConcept, studentMastery);

        // Assert
        meetsPrereqs.Should().BeFalse();
    }

    [Fact]
    public void GetReadyConcepts_WithPrerequisitesMet_ShouldReturnConcepts()
    {
        // Arrange
        var prerequisite = new ConceptId("prereq", SubjectArea.Science, GradeLevel.G6_8);
        var targetConcept = new ConceptId("target", SubjectArea.Science, GradeLevel.G6_8);
        var prerequisiteGraph = new Dictionary<ConceptId, IReadOnlyList<ConceptId>>
        {
            [targetConcept] = new[] { prerequisite }
        };
        var calculator = new MasteryCalculator(prerequisiteGraph);
        var availableConcepts = new List<ConceptId> { prerequisite, targetConcept };
        var studentMastery = new Dictionary<ConceptId, MasteryLevel>
        {
            [prerequisite] = MasteryLevel.Proficient
        };

        // Act
        var readyConcepts = calculator.GetReadyConcepts(availableConcepts, studentMastery);

        // Assert
        readyConcepts.Should().Contain(targetConcept);
    }
}
