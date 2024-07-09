using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class GameCurrency
{
    public string currencyCode { get; set; }
    public List<int> bets { get; set; }
    public List<int> denominations { get; set; }
}

public class GameDetails
{
    public string gameName { get; set; }
    public bool gamble { get; set; }
    public int maxGambleAttempts { get; set; }
    public int gambleBetLimit { get; set; }
    public List<int> supportedLines { get; set; }
    public int initialLines { get; set; }
    public int initialFactor { get; set; }
    public string version { get; set; }
    public int defaultVariant { get; set; }
    public List<GameCurrency> currencies { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the number of JSON files: ");
        int numberOfFiles = int.Parse(Console.ReadLine());

        List<GameDetails> allGames = MergeCurrenciesPerGame(numberOfFiles);

        // Save merged games to a new JSON file with a datetime stamp
        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string newJsonFileName = $"merged_games_{timeStamp}.json";
        string newJsonFile = Path.Combine(Environment.CurrentDirectory, newJsonFileName);
        string outputJson = JsonConvert.SerializeObject(allGames, Formatting.Indented);
        File.WriteAllText(newJsonFile, outputJson);

        Console.WriteLine($"Merged games saved to: {newJsonFile}");
    }

    static List<GameDetails> MergeCurrenciesPerGame(int numberOfFiles)
    {
        List<GameDetails> allGames = new List<GameDetails>();

        Console.WriteLine("Enter the file paths:");

        for (int i = 0; i < numberOfFiles; i++)
        {
            Console.Write($"File {i + 1}: ");
            string filePath = Console.ReadLine();
            
            string json = File.ReadAllText(filePath);
            List<GameDetails> games = JsonConvert.DeserializeObject<List<GameDetails>>(json);
            allGames.AddRange(games);
        }

        Dictionary<string, GameDetails> gamesByName = new Dictionary<string, GameDetails>();

        foreach (GameDetails game in allGames)
        {
            if (!gamesByName.ContainsKey(game.gameName))
            {
                gamesByName[game.gameName] = game;
            }
            else
            {
                // Merge currencies for existing game
                GameDetails existingGame = gamesByName[game.gameName];
                foreach (var currency in game.currencies)
                {
                    if (!existingGame.currencies.Exists(c => c.currencyCode == currency.currencyCode))
                    {
                        existingGame.currencies.Add(currency);
                    }
                    else
                    {
                        // Update existing currency
                        var existingCurrency = existingGame.currencies.Find(c => c.currencyCode == currency.currencyCode);
                        existingCurrency.bets.AddRange(currency.bets);
                        existingCurrency.denominations.AddRange(currency.denominations);
                    }
                }
            }
        }

        return new List<GameDetails>(gamesByName.Values);
    }
}
