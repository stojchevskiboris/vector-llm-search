using Microsoft.Extensions.Configuration;

namespace VectorLLM;

class Program
{
    public static IConfiguration Config { get; private set; }

    static async Task Main()
    {
        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();


        Console.Title = "Vector Search";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== AI Movie Search ===");
        Console.ResetColor();

        try
        {
            using var db = new AppDbContext();

            // Check for new embeddings
            await Service.GenerateAndSaveEmbeddings(db);

            Console.WriteLine("\n-------------------------------------------");
            Console.Write("🔍 Enter a search term (or 'exit' to quit): ");
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input) && input.ToLower() != "exit")
            {
                await Service.SearchFilms(db, input);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[!] Error: {ex.Message}");
            Console.ResetColor();
        }
    }
}