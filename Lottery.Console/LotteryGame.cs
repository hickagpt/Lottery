using Lottery.Console;
using Lottery.Console.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public class LotteryGame
{
    private readonly List<Player> _players = new List<Player>();
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly GameSettings _settings;
    private readonly IUserInterface _ui;
    private bool _isPlaying;

    public LotteryGame(IUserInterface ui, IOptions<GameSettings> settings, IRandomNumberGenerator randomNumberGenerator)
    {
        _ui = ui;
        _settings = settings.Value;
        _randomNumberGenerator = randomNumberGenerator;
    }

    public decimal LotteryProfit { get; private set; } = 0m;
    public List<Player> Players => _players;

    public void DrawPrizes()
    {
        // Calculate total tickets purchased
        int totalTickets = _players.Sum(player => player.TicketsPurchased);

        if (totalTickets == 0)
        {
            _ui.Write("No tickets purchased this round.");
            return;
        }

        // Calculate total revenue from ticket sales
        decimal revenue = totalTickets * _settings.TicketPrice;

        // Determine prize amounts based on prize distribution settings
        decimal grandPrize = revenue * (decimal)_settings.PrizeDistribution.GrandPrizePercentage;
        decimal secondPrizePool = revenue * (decimal)_settings.PrizeDistribution.SecondTierPercentage;
        decimal thirdPrizePool = revenue * (decimal)_settings.PrizeDistribution.ThirdTierPercentage;

        // Calculate lottery profit
        decimal roundProfit = revenue * 0.1m;
        LotteryProfit += roundProfit;

        // Sort players by tickets purchased in descending order
        var winners = new List<Player>(_players);
        winners.Sort((a, b) => b.TicketsPurchased.CompareTo(a.TicketsPurchased));

        // Grand Prize: Awarded to the player with the most tickets
        if (winners.Count > 0)
        {
            winners[0].Balance += grandPrize;
        }

        // Calculate the number of winners for second and third tiers
        int secondTierWinnersCount = (int)Math.Round(totalTickets * 0.1m);
        int thirdTierWinnersCount = (int)Math.Round(totalTickets * 0.2m);

        // Distribute second-tier prize equally among the top `secondTierWinnersCount` players
        decimal secondPrizePerWinner = secondTierWinnersCount > 0 ? secondPrizePool / secondTierWinnersCount : 0;
        for (int i = 1; i <= secondTierWinnersCount && i < winners.Count; i++)
        {
            winners[i].Balance += secondPrizePerWinner;
        }

        // Distribute third-tier prize equally among the next `thirdTierWinnersCount` players
        decimal thirdPrizePerWinner = thirdTierWinnersCount > 0 ? thirdPrizePool / thirdTierWinnersCount : 0;
        for (int i = secondTierWinnersCount + 1; i <= secondTierWinnersCount + thirdTierWinnersCount && i < winners.Count; i++)
        {
            winners[i].Balance += thirdPrizePerWinner;
        }

        // Display results
        _ui.Write($"Prizes distributed: Grand - {grandPrize:C}, " +
                  $"Second - {secondPrizePool:C} shared by {secondTierWinnersCount} winners, " +
                  $"Third - {thirdPrizePool:C} shared by {thirdTierWinnersCount} winners");
        _ui.Write($"Lottery profit for this round: {roundProfit:C}");
    }

    public void InitializeGame()
    {
        // Add a human player
        _players.Add(new HumanPlayer(_ui, "Human Player", _settings.StartingBalance));

        // Add CPU players within specified limits
        int cpuPlayersCount = _randomNumberGenerator.Next(_settings.MinCPUPlayers, _settings.MaxCPUPlayers + 1);
        for (int i = 0; i < cpuPlayersCount; i++)
        {
            _players.Add(new CPUPlayer($"CPU Player {i + 1}", _settings.StartingBalance, _randomNumberGenerator, _ui));
        }
    }

    public void RunCycle()
    {
        foreach (var player in _players)
        {
            player.PurchaseTickets(_settings.TicketPrice, 10); // Limit ticket purchase to 10 per round
        }

        DrawPrizes();

        // Display player balances
        foreach (var player in _players)
        {
            _ui.Write($"{player.Name} has a balance of {player.Balance:C}");
        }

        _ui.Write($"Total House Profit? {LotteryProfit}");

        // Check if any players want to continue
        _ui.Write("Do you want to play another round? (y/n)");
        if (_ui.Read()?.ToLower() != "y") _isPlaying = false;
    }

    public void RunGameLoop()
    {
        _ui.Write("Welcome to the Lottery Game!");

        _isPlaying = true;
        while (_isPlaying)
        {
            RunCycle();
        }
    }
}