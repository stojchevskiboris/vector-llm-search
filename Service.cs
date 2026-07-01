using ElBruno.LocalEmbeddings;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;

namespace VectorLLM;

public class Service
{
    public static async Task GenerateAndSaveEmbeddings(AppDbContext db)
    {
        await using var generator = await LocalEmbeddingGenerator.CreateAsync();
        var films = db.Films.Where(f => f.embeddings == null).ToList();

        if (!films.Any()) return;

        Console.WriteLine($"\n[INFO] {films.Count} movies pending embedding...");

        for (int i = 0; i < films.Count; i++)
        {
            var film = films[i];
            var embedding = await generator.GenerateEmbeddingAsync($"{film.title}: {film.description}");
            film.embeddings = MemoryMarshal.AsBytes(embedding.Vector.Span).ToArray();

            Console.Write($"\r[PROCESS] {i + 1}/{films.Count} - {film.title.PadRight(30)}");
        }

        await db.SaveChangesAsync();
        Console.WriteLine("\n[SUCCESS] All embeddings saved.\n");
    }

    public static async Task SearchFilms(AppDbContext db, string query)
    {
        Console.WriteLine($"\nSearching for: \"{query}\"...");
        await using var generator = await LocalEmbeddingGenerator.CreateAsync();

        var queryEmbedding = await generator.GenerateEmbeddingAsync(query);
        float[] queryVector = queryEmbedding.Vector.ToArray();

        var films = db.Films.Where(f => f.embeddings != null).ToList();

        var allScores = films.Select(f => new {
            f.title,
            f.description,
            Score = TensorPrimitives.CosineSimilarity(queryVector, MemoryMarshal.Cast<byte, float>(f.embeddings!).ToArray())
        }).ToList();

        float maxScore = allScores.Any() ? allScores.Max(s => s.Score) : 1.0f;

        var results = allScores.OrderByDescending(r => r.Score).Take(5);

        Console.WriteLine("\n" + new string('-', 45));
        Console.WriteLine($"{"SCORE",-10} | {"TITLE"}");
        Console.WriteLine(new string('-', 45));

        foreach (var res in results)
        {
            float normalized = res.Score / maxScore;

            Console.ForegroundColor = normalized > 0.95 ? ConsoleColor.Green :
                                      normalized > 0.85 ? ConsoleColor.Yellow :
                                                          ConsoleColor.DarkGray;

            Console.WriteLine($"{res.Score} | {res.title} => {res.description}");
        }
        Console.ResetColor();
        Console.WriteLine(new string('-', 45) + "\n");
    }
}