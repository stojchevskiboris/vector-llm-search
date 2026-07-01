using ElBruno.LocalEmbeddings;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;

namespace VectorLLM;

public class Service
{
    public static async Task GenerateAndSaveEmbeddings(AppDbContext db)
    {
        var films = db.Films.Where(f => f.embeddings == null).ToList();

        if (!films.Any()) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n📦 FOUND {films.Count} MOVIES WITHOUT EMBEDDINGS.");
        Console.WriteLine("⚙️  Initializing AI Engine...");
        Console.ResetColor();

        await using var generator = await LocalEmbeddingGenerator.CreateAsync();

        for (int i = 0; i < films.Count; i++)
        {
            var film = films[i];
            var embedding = await generator.GenerateEmbeddingAsync($"{film.title}: {film.description}");
            film.embeddings = MemoryMarshal.AsBytes(embedding.Vector.Span).ToArray();

            double progress = (double)(i + 1) / films.Count;
            Console.Write($"\r🧠 Processing: [{new string('█', (int)(progress * 20)).PadRight(20, '░')}] {progress:P0} | {film.title.PadRight(30)}");
        }

        await db.SaveChangesAsync();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✅ SUCCESS: All embeddings generated and synchronized with database.\n");
        Console.ResetColor();
    }

    public static async Task SearchFilms(AppDbContext db, string query)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n🚀 Deep diving for: \"{query}\"...");
        Console.ResetColor();

        await using var generator = await LocalEmbeddingGenerator.CreateAsync();

        var queryEmbedding = await generator.GenerateEmbeddingAsync(query);
        float[] queryVector = queryEmbedding.Vector.ToArray();

        var films = db.Films.Where(f => f.embeddings != null).ToList();

        var allResults = films.Select(f => new {
            Film = f,
            Score = TensorPrimitives.CosineSimilarity(queryVector, MemoryMarshal.Cast<byte, float>(f.embeddings!).ToArray())
        })
        .OrderByDescending(r => r.Score)
        .Take(5)
        .ToList();

        if (!allResults.Any())
        {
            Console.WriteLine("❌ No matches found.");
            return;
        }

        while (true)
        {
            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine($" {"#",-3} | {"MATCH",-8} | {"MOVIE TITLE"}");
            Console.WriteLine(new string('─', 60));

            for (int i = 0; i < allResults.Count; i++)
            {
                var res = allResults[i];
                Console.ForegroundColor = i == 0 ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine($" [{i + 1}] | {res.Score:P1}  | {res.Film.title}");
            }
            Console.ResetColor();
            Console.WriteLine(new string('═', 60));

            Console.Write("\n💡 Select a number for details, or press Enter to go back: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) break;

            if (int.TryParse(input, out int index) && index >= 1 && index <= allResults.Count)
            {
                DisplayFilmDetails(allResults[index - 1].Film);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️ Invalid selection.");
                Console.ResetColor();
            }
        }
    }

    private static void DisplayFilmDetails(Film film)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n" + new string('█', 60));
        Console.WriteLine($"  {film.title.ToUpper()} ({film.release_year ?? "N/A"})");
        Console.WriteLine(new string('█', 60));
        Console.ResetColor();

        Console.WriteLine($"\n⭐ Rating: {film.rating ?? "N/A"}  |  🕒 Length: {film.length} min");
        Console.WriteLine($"💰 Rental: ${film.rental_rate}  |  🔄 Replacement: ${film.replacement_cost}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n📖 PLOT SUMMARY:");
        Console.ResetColor();
        Console.WriteLine(film.description ?? "No description available.");

        if (!string.IsNullOrEmpty(film.special_features))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n✨ SPECIAL FEATURES:");
            Console.ResetColor();
            Console.WriteLine(film.special_features);
        }

        Console.WriteLine("\n" + new string('─', 60));
        Console.WriteLine("Press any key to return to search results...");
        Console.ReadKey(true);
    }
}