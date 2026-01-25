using DotNetAgents.Agents.BehaviorTrees;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.BehaviorTrees;

/// <summary>
/// Context for Socratic dialogue behavior tree.
/// </summary>
public class SocraticDialogueContext
{
    public ConceptContext Concept { get; set; } = null!;
    public StudentUnderstanding CurrentLevel { get; set; }
    public SocraticQuestion? CurrentQuestion { get; set; }
    public string? LastResponse { get; set; }
    public UnderstandingAssessment? LastAssessment { get; set; }
    public int QuestionCount { get; set; }
    public int HintLevel { get; set; }
    public bool MasteryAchieved { get; set; }
}

/// <summary>
/// Behavior tree for adaptive Socratic dialogue strategies.
/// </summary>
public class SocraticDialogueBehaviorTree
{
    private readonly ISocraticDialogueEngine _dialogueEngine;
    private readonly ILogger<SocraticDialogueBehaviorTree>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocraticDialogueBehaviorTree"/> class.
    /// </summary>
    public SocraticDialogueBehaviorTree(
        ISocraticDialogueEngine dialogueEngine,
        ILogger<SocraticDialogueBehaviorTree>? logger = null)
    {
        _dialogueEngine = dialogueEngine ?? throw new ArgumentNullException(nameof(dialogueEngine));
        _logger = logger;
    }

    /// <summary>
    /// Builds the Socratic dialogue behavior tree.
    /// </summary>
    public IBehaviorTreeNode<SocraticDialogueContext> Build()
    {
        // Root selector: Try different strategies
        return new SelectorNode<SocraticDialogueContext>("SocraticDialogueRoot")
            .AddChild(new SequenceNode<SocraticDialogueContext>("MasteryCheck")
                .AddChild(new ConditionNode<SocraticDialogueContext>("CheckMastery", ctx => ctx.MasteryAchieved))
                .AddChild(new ActionNode<SocraticDialogueContext>("CelebrateMastery", CelebrateMastery)))

            .AddChild(new SequenceNode<SocraticDialogueContext>("StrugglingStudentPath")
                .AddChild(new ConditionNode<SocraticDialogueContext>("IsStruggling", IsStruggling))
                .AddChild(new ActionNode<SocraticDialogueContext>("ProvideScaffolding", ProvideScaffolding))
                .AddChild(new ActionNode<SocraticDialogueContext>("GenerateScaffoldedQuestion", GenerateScaffoldedQuestion)))

            .AddChild(new SequenceNode<SocraticDialogueContext>("NeedsHintPath")
                .AddChild(new ConditionNode<SocraticDialogueContext>("NeedsHint", NeedsHint))
                .AddChild(new ActionNode<SocraticDialogueContext>("GenerateHint", GenerateHint))
                .AddChild(new ActionNode<SocraticDialogueContext>("GenerateFollowUpQuestion", GenerateFollowUpQuestion)))

            .AddChild(new SequenceNode<SocraticDialogueContext>("StandardDialoguePath")
                .AddChild(new ConditionNode<SocraticDialogueContext>("HasQuestion", ctx => ctx.CurrentQuestion == null))
                .AddChild(new ActionNode<SocraticDialogueContext>("GenerateQuestion", GenerateQuestion)))

            .AddChild(new SequenceNode<SocraticDialogueContext>("EvaluateResponsePath")
                .AddChild(new ConditionNode<SocraticDialogueContext>("HasResponse", ctx => !string.IsNullOrWhiteSpace(ctx.LastResponse)))
                .AddChild(new ActionNode<SocraticDialogueContext>("EvaluateResponse", EvaluateResponse))
                .AddChild(new ActionNode<SocraticDialogueContext>("DetermineNextStep", DetermineNextStep)));
    }

    private async Task<BehaviorTreeNodeStatus> CelebrateMastery(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Student achieved mastery for concept {ConceptId}", context.Concept.ConceptId.Value);
        return BehaviorTreeNodeStatus.Success;
    }

    private bool IsStruggling(SocraticDialogueContext context)
    {
        return context.LastAssessment != null &&
               (context.LastAssessment.AssessedLevel == StudentUnderstanding.Beginner ||
                context.LastAssessment.AssessedLevel == StudentUnderstanding.None ||
                context.LastAssessment.NeedsMoreHelp) &&
               context.QuestionCount >= 3;
    }

    private async Task<BehaviorTreeNodeStatus> ProvideScaffolding(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Providing scaffolding for struggling student");
        // Scaffolding logic would go here
        return BehaviorTreeNodeStatus.Success;
    }

    private async Task<BehaviorTreeNodeStatus> GenerateScaffoldedQuestion(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        var question = await _dialogueEngine.GenerateQuestionAsync(
            context.Concept,
            StudentUnderstanding.Beginner, // Use beginner level for scaffolding
            cancellationToken: cancellationToken);

        context.CurrentQuestion = question;
        context.QuestionCount++;
        return BehaviorTreeNodeStatus.Success;
    }

    private bool NeedsHint(SocraticDialogueContext context)
    {
        return context.LastAssessment != null &&
               context.LastAssessment.NeedsMoreHelp &&
               context.HintLevel < 5 &&
               !string.IsNullOrWhiteSpace(context.LastResponse);
    }

    private async Task<BehaviorTreeNodeStatus> GenerateHint(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        if (context.CurrentQuestion == null)
            return BehaviorTreeNodeStatus.Failure;

        context.HintLevel++;
        var hint = await _dialogueEngine.GenerateHintAsync(
            context.CurrentQuestion,
            context.HintLevel,
            cancellationToken);

        _logger?.LogInformation("Generated hint level {Level}", context.HintLevel);
        return BehaviorTreeNodeStatus.Success;
    }

    private async Task<BehaviorTreeNodeStatus> GenerateFollowUpQuestion(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        var question = await _dialogueEngine.GenerateQuestionAsync(
            context.Concept,
            context.CurrentLevel,
            cancellationToken: cancellationToken);

        context.CurrentQuestion = question;
        context.QuestionCount++;
        return BehaviorTreeNodeStatus.Success;
    }

    private async Task<BehaviorTreeNodeStatus> GenerateQuestion(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        var question = await _dialogueEngine.GenerateQuestionAsync(
            context.Concept,
            context.CurrentLevel,
            cancellationToken: cancellationToken);

        context.CurrentQuestion = question;
        context.QuestionCount++;
        return BehaviorTreeNodeStatus.Success;
    }

    private async Task<BehaviorTreeNodeStatus> EvaluateResponse(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        if (context.CurrentQuestion == null || string.IsNullOrWhiteSpace(context.LastResponse))
            return BehaviorTreeNodeStatus.Failure;

        var assessment = await _dialogueEngine.EvaluateResponseAsync(
            context.LastResponse,
            context.CurrentQuestion,
            cancellationToken);

        context.LastAssessment = assessment;
        context.MasteryAchieved = assessment.HasMastery;

        return BehaviorTreeNodeStatus.Success;
    }

    private Task<BehaviorTreeNodeStatus> DetermineNextStep(
        SocraticDialogueContext context,
        CancellationToken cancellationToken)
    {
        if (context.LastAssessment == null)
            return Task.FromResult(BehaviorTreeNodeStatus.Failure);

        // Clear current question to trigger new question generation
        if (context.LastAssessment.HasMastery)
        {
            context.MasteryAchieved = true;
        }
        else
        {
            context.CurrentQuestion = null; // Will trigger new question
        }

        return Task.FromResult(BehaviorTreeNodeStatus.Success);
    }
}
