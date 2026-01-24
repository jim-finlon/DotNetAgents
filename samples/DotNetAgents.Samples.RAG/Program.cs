using DotNetAgents.Abstractions.Chains;
using DotNetAgents.Core.Chains;
using DotNetAgents.Abstractions.Documents;
using DotNetAgents.Core.Documents;
using DotNetAgents.Documents.Loaders;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Prompts;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Abstractions.Retrieval;
using DotNetAgents.Core.Retrieval.Implementations;
using DotNetAgents.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.RAG;

/// <summary>
/// Example demonstrating a RAG (Retrieval-Augmented Generation) pipeline.
/// This example shows how to:
/// 1. Load documents
/// 2. Split them into chunks
/// 3. Create embeddings and store in a vector store
/// 4. Retrieve relevant chunks based on a query
/// 5. Use retrieved context to answer questions
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - RAG (Retrieval-Augmented Generation) Example");
        Console.WriteLine("===========================================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Add OpenAI provider (requires OPENAI_API_KEY environment variable)
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERROR: OPENAI_API_KEY environment variable is not set.");
            Console.WriteLine("Please set it to your OpenAI API key to run this example.");
            return;
        }

        services.AddOpenAI(apiKey, "gpt-3.5-turbo");

        var serviceProvider = services.BuildServiceProvider();
        var llm = serviceProvider.GetRequiredService<ILLMModel<string, string>>();
        var embeddingModel = serviceProvider.GetRequiredService<IEmbeddingModel>();

        // Step 1: Create sample documents
        Console.WriteLine("Step 1: Creating sample documents...");
        var documents = CreateSampleDocuments();
        Console.WriteLine($"Created {documents.Count} documents.\n");

        // Step 2: Split documents into chunks
        Console.WriteLine("Step 2: Splitting documents into chunks...");
        var textSplitter = new CharacterTextSplitter(chunkSize: 200, chunkOverlap: 50);
        var chunks = new List<Document>();
        
        foreach (var doc in documents)
        {
            var docChunks = await textSplitter.SplitDocumentsAsync(new[] { doc }, cancellationToken: default).ConfigureAwait(false);
            chunks.AddRange(docChunks);
        }
        
        Console.WriteLine($"Created {chunks.Count} chunks from {documents.Count} documents.\n");

        // Step 3: Create embeddings and store in vector store
        Console.WriteLine("Step 3: Creating embeddings and storing in vector store...");
        var vectorStore = new InMemoryVectorStore();
        
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = await embeddingModel.EmbedAsync(
                chunk.Content,
                cancellationToken: default).ConfigureAwait(false);
            
            var metadata = new Dictionary<string, object>(chunk.Metadata)
            {
                ["content"] = chunk.Content
            };
            
            await vectorStore.UpsertAsync(
                $"chunk_{i}",
                embedding,
                metadata,
                cancellationToken: default).ConfigureAwait(false);
        }
        
        Console.WriteLine($"Stored {chunks.Count} document chunks with embeddings.\n");

        // Step 4: Create prompt template for RAG
        Console.WriteLine("Step 4: Creating RAG prompt template...");
        var promptTemplate = new PromptTemplate(
            @"Use the following context to answer the question. If the context doesn't contain 
enough information to answer the question, say so.

Context:
{context}

Question: {query}

Answer:");

        // Step 5: Create RAG chain using RetrievalChain
        Console.WriteLine("Step 5: Creating RAG chain...");
        var ragChain = new RetrievalChain<Dictionary<string, object>, string>(
            promptTemplate,
            llm,
            vectorStore,
            embeddingModel,
            topK: 3);
        Console.WriteLine("RAG chain ready!\n");

        // Step 6: Ask questions
        var questions = new[]
        {
            "What is artificial intelligence?",
            "What are the main applications of AI?",
            "What are the challenges in AI development?"
        };

        foreach (var question in questions)
        {
            Console.WriteLine($"Question: {question}");
            Console.WriteLine("Retrieving relevant context...");
            
            try
            {
                var input = new Dictionary<string, object> { ["query"] = question };
                var answer = await ragChain.InvokeAsync(input, cancellationToken: default).ConfigureAwait(false);
                Console.WriteLine($"Answer: {answer}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        Console.WriteLine("RAG example completed!");
    }

    private static List<Document> CreateSampleDocuments()
    {
        return new List<Document>
        {
            new Document
            {
                Content = @"Artificial Intelligence (AI) is a branch of computer science that aims to create 
intelligent machines capable of performing tasks that typically require human intelligence. 
These tasks include learning, reasoning, problem-solving, perception, and language understanding. 
AI systems can be categorized into narrow AI, which is designed for specific tasks, and general AI, 
which would have human-like cognitive abilities across a wide range of tasks.",
                Metadata = new Dictionary<string, object> { ["id"] = "doc1", ["title"] = "Introduction to AI", ["source"] = "sample" }
            },

            new Document
            {
                Content = @"Machine Learning is a subset of AI that enables systems to learn and improve 
from experience without being explicitly programmed. It uses algorithms to analyze data, identify patterns, 
and make predictions or decisions. Common applications include image recognition, natural language processing, 
recommendation systems, and autonomous vehicles.",
                Metadata = new Dictionary<string, object> { ["id"] = "doc2", ["title"] = "Machine Learning Basics", ["source"] = "sample" }
            },

            new Document
            {
                Content = @"Deep Learning is a specialized form of machine learning that uses neural networks 
with multiple layers to model and understand complex patterns. It has revolutionized fields such as computer vision, 
speech recognition, and natural language processing. Deep learning models require large amounts of data and 
computational resources but can achieve remarkable accuracy.",
                Metadata = new Dictionary<string, object> { ["id"] = "doc3", ["title"] = "Deep Learning Overview", ["source"] = "sample" }
            },

            new Document
            {
                Content = @"AI applications are widespread across industries. In healthcare, AI assists in 
medical diagnosis and drug discovery. In finance, it's used for fraud detection and algorithmic trading. 
In transportation, AI powers autonomous vehicles and traffic management systems. In customer service, 
AI chatbots provide 24/7 support. The potential applications continue to grow as technology advances.",
                Metadata = new Dictionary<string, object> { ["id"] = "doc4", ["title"] = "AI Applications", ["source"] = "sample" }
            },

            new Document
            {
                Content = @"Challenges in AI development include data quality and availability, algorithmic bias, 
explainability and transparency, computational requirements, and ethical concerns. Ensuring AI systems are fair, 
transparent, and beneficial to society requires ongoing research and careful consideration of these challenges. 
Regulation and standards are also evolving to address these concerns.",
                Metadata = new Dictionary<string, object> { ["id"] = "doc5", ["title"] = "AI Challenges", ["source"] = "sample" }
            }
        };
    }

}