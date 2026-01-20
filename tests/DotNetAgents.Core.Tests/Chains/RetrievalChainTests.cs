using DotNetAgents.Core.Chains;
using DotNetAgents.Core.Models;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Core.Retrieval;
using DotNetAgents.Core.Retrieval.Implementations;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Core.Tests.Chains;

public class RetrievalChainTests
{
    [Fact]
    public async Task InvokeAsync_WithValidInput_RetrievesAndGenerates()
    {
        // Arrange
        var promptTemplate = new PromptTemplate("Context: {context}\n\nQuestion: {query}\n\nAnswer:");
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Answer based on context");

        var vectorStore = new InMemoryVectorStore();
        var mockEmbedding = new Mock<IEmbeddingModel>();
        mockEmbedding.Setup(m => m.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[] { 1.0f, 0.0f, 0.0f });

        // Add a document to the vector store
        var docVector = new float[] { 1.0f, 0.0f, 0.0f };
        var metadata = new Dictionary<string, object> { ["content"] = "Test document content" };
        await vectorStore.UpsertAsync("doc1", docVector, metadata);

        var chain = new RetrievalChain<IDictionary<string, object>, string>(
            promptTemplate,
            mockLLM.Object,
            vectorStore,
            mockEmbedding.Object,
            topK: 5);

        var input = new Dictionary<string, object> { ["query"] = "test query" };

        // Act
        var result = await chain.InvokeAsync(input);

        // Assert
        result.Should().Be("Answer based on context");
        mockLLM.Verify(m => m.GenerateAsync(
            It.Is<string>(s => s.Contains("Context:") && s.Contains("test query")),
            It.IsAny<LLMOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullPromptTemplate_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        var vectorStore = new InMemoryVectorStore();
        var mockEmbedding = new Mock<IEmbeddingModel>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RetrievalChain<IDictionary<string, object>, string>(
                null!,
                mockLLM.Object,
                vectorStore,
                mockEmbedding.Object));
    }

    [Fact]
    public void Constructor_WithInvalidTopK_ThrowsArgumentException()
    {
        // Arrange
        var promptTemplate = new PromptTemplate("Test");
        var mockLLM = new Mock<ILLMModel<string, string>>();
        var vectorStore = new InMemoryVectorStore();
        var mockEmbedding = new Mock<IEmbeddingModel>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new RetrievalChain<IDictionary<string, object>, string>(
                promptTemplate,
                mockLLM.Object,
                vectorStore,
                mockEmbedding.Object,
                topK: 0));
    }
}