namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Subscription tier for families.
/// </summary>
public enum SubscriptionTier
{
    Free,
    Basic,
    Premium,
    Enterprise
}

/// <summary>
/// Role of a guardian within a family.
/// </summary>
public enum GuardianRole
{
    Primary,
    Secondary,
    Teacher
}

/// <summary>
/// Science subject areas.
/// </summary>
public enum Subject
{
    Biology,
    Chemistry,
    Physics,
    EarthScience,
    Astronomy,
    EnvironmentalScience,
    Mathematics
}

/// <summary>
/// Grade band levels for K-12 education.
/// </summary>
public enum GradeBand
{
    K2,
    G3_5,
    G6_8,
    G9_10,
    G11_12
}

/// <summary>
/// Type of content unit.
/// </summary>
public enum ContentType
{
    Concept,
    Lesson,
    Assessment,
    Lab,
    Vocabulary,
    Review
}

/// <summary>
/// Bloom's taxonomy levels.
/// </summary>
public enum BloomLevel
{
    Remember,
    Understand,
    Apply,
    Analyze,
    Evaluate,
    Create
}

/// <summary>
/// Mastery level for student content understanding.
/// </summary>
public enum MasteryLevel
{
    NotStarted,
    Introduced,
    Developing,
    Proficient,
    Mastered
}

/// <summary>
/// Type of assessment.
/// </summary>
public enum AssessmentType
{
    Diagnostic,
    Formative,
    Summative,
    Practice
}

/// <summary>
/// Type of assessment question.
/// </summary>
public enum QuestionType
{
    MultipleChoice,
    MultipleSelect,
    FillBlank,
    ShortAnswer,
    Matching,
    Ordering,
    TrueFalse,
    ConstructedResponse
}
