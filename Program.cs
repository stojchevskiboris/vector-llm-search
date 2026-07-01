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


        Console.Title = "🎬 AI Movie Search & Discovery";

        PrintHeader();

        try
        {
            using var db = new AppDbContext();

            // Check for new embeddings
            await Service.GenerateAndSaveEmbeddings(db);

            bool exit = false;
            while (!exit)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n" + new string('═', 50));
                Console.WriteLine(" 🎥 MAIN MENU");
                Console.WriteLine(new string('─', 50));
                Console.WriteLine(" [1] Search for Movies");
                Console.WriteLine(" [Q] Quit Application");
                Console.WriteLine(new string('═', 50));
                Console.ResetColor();

                Console.Write("\n✨ Select an option: ");
                string? choice = Console.ReadLine()?.ToLower();

                switch (choice)
                {
                    case "1":
                        await RunSearch(db);
                        break;
                    case "q":
                    case "quit":
                    case "exit":
                        exit = true;
                        Console.WriteLine("\n👋 Goodbye! Enjoy your movies!\n");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠️ Invalid option, please try again.");
                        Console.ResetColor();
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[!] Error: {ex.Message}");
            Console.ResetColor();
        }
    }

    static void PrintHeader()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
 █████╗ ██╗    ███╗   ███╗ ██████╗ ██╗   ██╗██╗███████╗
██╔══██╗██║    ████╗ ████║██╔═══██╗██║   ██║██║██╔════╝
███████║██║    ██╔████╔██║██║   ██║██║   ██║██║█████╗
██╔══██║██║    ██║╚██╔╝██║██║   ██║╚██╗ ██╔╝██║██╔══╝
██║  ██║██║    ██║ ╚═╝ ██║╚██████╔╝ ╚████╔╝ ██║███████╗
╚═╝  ╚═╝╚═╝    ╚═╝     ╚═╝ ╚═════╝   ╚═══╝  ╚═╝╚══════╝

 ███████╗███████╗ █████╗ ██████╗  ██████╗██╗  ██╗
 ██╔════╝██╔════╝██╔══██╗██╔══██╗██╔════╝██║  ██║
 ███████╗█████╗  ███████║██████╔╝██║     ███████║
 ╚════██║██╔══╝  ██╔══██║██╔══██╗██║     ██╔══██║
 ███████║███████╗██║  ██║██║  ██║╚██████╗██║  ██║
 ╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝
        ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("         --- Vector-Powered Recommendation Engine ---");
        Console.ResetColor();
    }

    static async Task RunSearch(AppDbContext db)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n🔍 AI MOVIE SEARCH");
        Console.WriteLine("   Describe what you want to watch (e.g., 'Action-packed space adventure')");
        Console.ResetColor();

        Console.Write("\n🔎 Search: ");
        string? input = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(input))
        {
            await Service.SearchFilms(db, input);
        }
    }
}