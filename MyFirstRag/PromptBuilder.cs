using MyFirstRag.Models;

namespace MyFirstRag;

internal class PromptBuilder
{
    public static string BuildPrompt(string context, string question)
    {
        return $@"""
            Based on the following context from documents, please answer the question.

            CONTEXT:
            {context}

            QUESTION: {question}

            INSTRUCTIONS:
            - Answer the question using only the information provided in the context above
            - Be specific and cite relevant parts of the context
            - If the context doesn't contain enough information to answer fully, state this clearly
            - Keep your answer concise and focused

            ANSWER:
            """;
    }

    public static string BuildContext(IEnumerable<VectorChunk> chunks)
    {
        return string.Join("\n\n---\n\n", chunks.Select((chunk, index) =>
            $"[Context {index + 1}] (Relevance: {chunk.SimilarityScore:F2})\n{chunk.Text}"));
    }

    public static string GetSystemMessage()
    {
        var message = """
            You are a helpful assistant that answers questions based on the provided context.
            Always base your answers on the context provided.
            If the context doesn't contain enough information, say so.
            """;

        return message;
    }
}