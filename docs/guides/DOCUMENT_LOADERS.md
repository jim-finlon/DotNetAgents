# Document Loaders Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents provides comprehensive document loading capabilities with support for 10 different document formats. All loaders implement the `IDocumentLoader` interface and can be used with text splitters for chunking.

## Table of Contents

1. [Available Loaders](#available-loaders)
2. [Basic Usage](#basic-usage)
3. [Text Splitters](#text-splitters)
4. [Loader-Specific Features](#loader-specific-features)
5. [Comparison](#comparison)
6. [Best Practices](#best-practices)

## Available Loaders

| Loader | Format | Features |
|--------|--------|----------|
| **PdfDocumentLoader** | PDF | Page splitting, text extraction |
| **CsvDocumentLoader** | CSV | Header mapping, row splitting |
| **ExcelDocumentLoader** | Excel (.xlsx, .xls) | Worksheet/row splitting |
| **EpubDocumentLoader** | EPUB | Chapter splitting |
| **MarkdownDocumentLoader** | Markdown | Preserves structure |
| **TextDocumentLoader** | Plain Text | Simple text loading |
| **DocxDocumentLoader** | Word (.docx) | Paragraph extraction |
| **HtmlDocumentLoader** | HTML | Text extraction, tag removal |
| **JsonDocumentLoader** | JSON | Flattening, path-based extraction |
| **XmlDocumentLoader** | XML | Text extraction, element-based |

## Basic Usage

### Simple Loading

```csharp
using DotNetAgents.Documents.Loaders;

// Load a PDF
var pdfLoader = new PdfDocumentLoader();
var documents = await pdfLoader.LoadAsync("document.pdf");

foreach (var doc in documents)
{
    Console.WriteLine($"Page {doc.Metadata["page"]}: {doc.Content.Substring(0, 100)}...");
}
```

### With Text Splitter

```csharp
using DotNetAgents.Documents.Splitters;

var loader = new PdfDocumentLoader();
var splitter = new RecursiveCharacterTextSplitter(
    chunkSize: 1000,
    chunkOverlap: 200
);

var documents = await loader.LoadAsync("document.pdf");
var chunks = await splitter.SplitAsync(documents);

Console.WriteLine($"Created {chunks.Count} chunks from {documents.Count} pages");
```

## Text Splitters

### RecursiveCharacterTextSplitter

Splits text recursively by characters, trying to keep paragraphs/sentences together:

```csharp
var splitter = new RecursiveCharacterTextSplitter(
    chunkSize: 1000,
    chunkOverlap: 200,
    separators: new[] { "\n\n", "\n", ". ", " ", "" }
);

var chunks = await splitter.SplitAsync(documents);
```

### CharacterTextSplitter

Simple character-based splitting:

```csharp
var splitter = new CharacterTextSplitter(
    chunkSize: 1000,
    chunkOverlap: 200
);

var chunks = await splitter.SplitAsync(documents);
```

### SemanticTextSplitter

Splits based on semantic similarity (requires embeddings):

```csharp
var splitter = new SemanticTextSplitter(
    embeddingService: embeddingService,
    chunkSize: 1000,
    similarityThreshold: 0.8
);

var chunks = await splitter.SplitAsync(documents);
```

## Loader-Specific Features

### PDF Loader

```csharp
var pdfLoader = new PdfDocumentLoader();

// Load with page splitting
var documents = await pdfLoader.LoadAsync("document.pdf");

// Access page metadata
foreach (var doc in documents)
{
    var pageNumber = doc.Metadata["page"];
    var totalPages = doc.Metadata["total_pages"];
    Console.WriteLine($"Page {pageNumber}/{totalPages}");
}
```

### CSV Loader

```csharp
var csvLoader = new CsvDocumentLoader();

// Load with header mapping
var documents = await csvLoader.LoadAsync("data.csv", options: new CsvLoadOptions
{
    HasHeader = true,
    Delimiter = ",",
    MapHeaders = true
});

// Each row becomes a document
foreach (var doc in documents)
{
    Console.WriteLine($"Row: {doc.Content}");
    Console.WriteLine($"Headers: {string.Join(", ", doc.Metadata.Keys)}");
}
```

### Excel Loader

```csharp
var excelLoader = new ExcelDocumentLoader();

// Load specific worksheet
var documents = await excelLoader.LoadAsync("data.xlsx", options: new ExcelLoadOptions
{
    WorksheetName = "Sheet1",
    IncludeHeaders = true,
    SplitByRow = true
});

// Or load all worksheets
var allSheets = await excelLoader.LoadAllWorksheetsAsync("data.xlsx");
```

### EPUB Loader

```csharp
var epubLoader = new EpubDocumentLoader();

// Load with chapter splitting
var documents = await epubLoader.LoadAsync("book.epub");

// Access chapter metadata
foreach (var doc in documents)
{
    var chapter = doc.Metadata["chapter"];
    var title = doc.Metadata["title"];
    Console.WriteLine($"Chapter {chapter}: {title}");
}
```

### JSON Loader

```csharp
var jsonLoader = new JsonDocumentLoader();

// Load with flattening
var documents = await jsonLoader.LoadAsync("data.json", options: new JsonLoadOptions
{
    Flatten = true,
    MaxDepth = 5,
    IncludePaths = new[] { "items", "products" }
});

// Access flattened paths
foreach (var doc in documents)
{
    var path = doc.Metadata["json_path"];
    Console.WriteLine($"Path: {path}, Content: {doc.Content}");
}
```

### HTML Loader

```csharp
var htmlLoader = new HtmlDocumentLoader();

// Load with text extraction
var documents = await htmlLoader.LoadAsync("page.html", options: new HtmlLoadOptions
{
    RemoveScripts = true,
    RemoveStyles = true,
    PreserveLinks = false
});

// Clean HTML text
foreach (var doc in documents)
{
    Console.WriteLine(doc.Content); // Clean text, no HTML tags
}
```

### XML Loader

```csharp
var xmlLoader = new XmlDocumentLoader();

// Load with element-based extraction
var documents = await xmlLoader.LoadAsync("data.xml", options: new XmlLoadOptions
{
    RootElement = "items",
    ChildElement = "item",
    IncludeAttributes = true
});

// Access element metadata
foreach (var doc in documents)
{
    var elementName = doc.Metadata["element"];
    var attributes = doc.Metadata["attributes"];
    Console.WriteLine($"Element: {elementName}, Attributes: {attributes}");
}
```

## Comparison

### Feature Matrix

| Loader | Binary Support | Metadata | Splitting | Complexity |
|--------|---------------|----------|-----------|------------|
| **PDF** | ✅ | Page numbers | Page-based | Medium |
| **CSV** | ❌ | Headers, row numbers | Row-based | Low |
| **Excel** | ✅ | Worksheet, headers | Row-based | Medium |
| **EPUB** | ✅ | Chapters, titles | Chapter-based | Medium |
| **Markdown** | ❌ | Structure preserved | Paragraph-based | Low |
| **Text** | ❌ | Minimal | Character-based | Very Low |
| **DOCX** | ✅ | Paragraphs, styles | Paragraph-based | Medium |
| **HTML** | ❌ | Tags, structure | Element-based | Medium |
| **JSON** | ❌ | Paths, structure | Path-based | Medium |
| **XML** | ❌ | Elements, attributes | Element-based | Medium |

### Performance

| Loader | Speed | Memory Usage | Best For |
|--------|-------|-------------|----------|
| **Text** | Very Fast | Low | Simple text files |
| **Markdown** | Very Fast | Low | Documentation |
| **CSV** | Fast | Low | Tabular data |
| **JSON** | Fast | Medium | Structured data |
| **HTML** | Medium | Medium | Web content |
| **XML** | Medium | Medium | Structured documents |
| **PDF** | Medium | High | Documents |
| **DOCX** | Medium | High | Word documents |
| **Excel** | Slow | High | Spreadsheets |
| **EPUB** | Slow | High | E-books |

## Best Practices

### 1. Chunking Strategy

```csharp
// Use appropriate chunk size based on content
var splitter = new RecursiveCharacterTextSplitter(
    chunkSize: 1000,  // For general text
    chunkOverlap: 200
);

// For code/documentation, use larger chunks
var codeSplitter = new RecursiveCharacterTextSplitter(
    chunkSize: 2000,
    chunkOverlap: 400,
    separators: new[] { "\n\n", "\n", " ", "" }
);
```

### 2. Metadata Preservation

```csharp
// Preserve important metadata
var documents = await loader.LoadAsync("file.pdf");

foreach (var doc in documents)
{
    // Add custom metadata
    doc.Metadata["source"] = "file.pdf";
    doc.Metadata["loaded_at"] = DateTime.UtcNow;
    doc.Metadata["loader"] = "PdfDocumentLoader";
}
```

### 3. Error Handling

```csharp
try
{
    var documents = await loader.LoadAsync("file.pdf");
}
catch (DocumentException ex)
{
    _logger.LogError(ex, "Failed to load document");
    // Handle error appropriately
}
```

### 4. Batch Processing

```csharp
var files = Directory.GetFiles("documents", "*.pdf");

var allDocuments = new List<Document>();
foreach (var file in files)
{
    var documents = await pdfLoader.LoadAsync(file);
    allDocuments.AddRange(documents);
}

// Process all documents together
var chunks = await splitter.SplitAsync(allDocuments);
```

### 5. Memory Management

```csharp
// For large files, process in batches
await foreach (var document in loader.LoadStreamAsync("large-file.pdf"))
{
    var chunks = await splitter.SplitAsync(new[] { document });
    await ProcessChunksAsync(chunks);
}
```

## Related Documentation

- [RAG Guide](./RAG_GUIDE.md)
- [Document Interfaces](../../src/DotNetAgents.Abstractions/Documents/IDocumentLoader.cs)
- [Text Splitters](../../src/DotNetAgents.Documents/Documents/Splitters/)
