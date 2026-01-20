using DotNetAgents.Core.Chains;
using DotNetAgents.Core.Documents;
using DotNetAgents.Core.Documents.Loaders;
using DotNetAgents.Core.Memory.Implementations;
using DotNetAgents.Core.Models;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Core.Retrieval;
using DotNetAgents.Core.VectorStores;
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
        var chunks = new List<IDocument>();
        
        foreach (var doc in documents)
        {
            var docChunks = await textSplitter.SplitDocumentsAsync(new[] { doc }, cancellationToken: default).ConfigureAwait(false);
            chunks.AddRange(docChunks);
        }
        
        Console.WriteLine($"Created {chunks.Count} chunks from {documents.Count} documents.\n");

        // Step 3: Create embeddings and store in vector store
        Console.WriteLine("Step 3: Creating embeddings and storing in vector store...");
        var vectorStore = new InMemoryVectorStore();
        
        foreach (var chunk in chunks)
        {
            var embedding = await embeddingModel.GenerateEmbeddingAsync(
                chunk.Content,
                cancellationToken: default).ConfigureAwait(false);
            
            await vectorStore.AddAsync(
                chunk.Id,
                embedding,
                chunk,
                cancellationToken: default).ConfigureAwait(false);
        }
        
        Console.WriteLine($"Stored {chunks.Count} document chunks with embeddings.\n");

        // Step 4: Create retrieval chain
        Console.WriteLine("Step 4: Creating retrieval chain...");
        var retrievalChain = new RetrievalChain(
            vectorStore,
            embeddingModel,
            topK: 3);

        // Step 5: Create RAG chain that combines retrieval with LLM
        Console.WriteLine("Step 5: Creating RAG chain...");
        var ragChain = CreateRAGChain(llm, retrievalChain);
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
                var answer = await ragChain.InvokeAsync(question, cancellationToken: default).ConfigureAwait(false);
                Console.WriteLine($"Answer: {answer}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        Console.WriteLine("RAG example completed!");
    }

    private static List<IDocument> CreateSampleDocuments()
    {
        return new List<IDocument>
        {
            new Document(
                id: "doc1",
                content: @"Artificial Intelligence (AI) is a branch of computer science that aims to create 
intelligent machines capable of performing tasks that typically require human intelligence. 
These tasks include learning, reasoning, problem-solving, perception, and language understanding. 
AI systems can be categorized into narrow AI, which is designed for specific tasks, and general AI, 
which would have human-like cognitive abilities across a wide range of tasks.",
                metadata: new Dictionary<string, object> { ["title"] = "Introduction to AI", ["source"] = "sample" }),

            new Document(
                id: "doc2",
                content: @"Machine Learning is a subset of AI that enables systems to learn and improve 
from experience without being explicitly programmed. It uses algorithms to analyze data, identify patterns, 
and make predictions or decisions. Common applications include image recognition, natural language processing, 
recommendation systems, and autonomous vehicles.",
                metadata: new Dictionary<string, object> { ["title"] = "Machine Learning Basics", ["source"] = "sample" }),

            new Document(
                id: "doc3",
                content: @"Deep Learning is a specialized form of machine learning that uses neural networks 
with multiple layers to model and understand complex patterns. It has revolutionized fields such as computer vision, 
speech recognition, and natural language processing. Deep learning models require large amounts of data and 
computational resources but can achieve remarkable accuracy.",
                metadata: new Dictionary<string, object> { ["title"] = "Deep Learning Overview", ["source"] = "sample" }),

            new Document(
                id: "doc4",
                content: @"AI applications are widespread across industries. In healthcare, AI assists in 
medical diagnosis and drug discovery. In finance, it's used for fraud detection and algorithmic trading. 
In transportation, AI powers autonomous vehicles and traffic management systems. In customer service, 
AI chatbots provide 24/7 support. The potential applications continue to grow as technology advances.",
                metadata: new Dictionary<string, object> { ["title"] = "AI Applications", ["source"] = "sample" }),

            new Document(
                id: "doc5",
                content: @"Challenges in AI development include data quality and availability, algorithmic bias, 
explainability and transparency, computational requirements, and ethical concerns. Ensuring AI systems are fair, 
transparent, and beneficial to society requires ongoing research and careful consideration of these challenges. 
Regulation and standards are also evolving to address these concerns.",
                metadata: new Dictionary<string, object> { ["title"] = "AI Challenges", ["source"] = "sample" })
        };
    }

    private static IRunnable<string, string> CreateRAGChain(
        ILLMModel<string, string> llm,
        RetrievalChain retrievalChain)
    {
        // Create a chain that:
        // 1. Retrieves relevant documents based on the query
        // 2. Formats them into a context
        // 3. Uses the LLM to generate an answer based on the context

        return new Runnable<string, string>(async (query, ct) =>
        {
            // Retrieve relevant documents
            var retrievedDocs = await retrievalChain.RetrieveAsync(query, cancellationToken: ct).ConfigureAwait(false);

            // Format context from retrieved documents
            var context = string.Join("\n\n", retrievedDocs.Select((doc, idx) => 
                $"[Document {idx + 1}]\n{doc.Content}"));

            // Create prompt with context
            var prompt = $@"Use the following context to answer the question. If the context doesn't contain 
enough information to answer the question, say so.

Context:
{context}

Question: {query}

Answer:";

            // Generate answer using LLM
            var answer = await llm.GenerateAsync(prompt, cancellationToken: ct).ConfigureAwait(false);
            return answer;
        });
    }
}