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
        int totalTickets = 0;
        foreach (var player in _players)
        {
            totalTickets += player.TicketsPurchased;
        }

        if (totalTickets == 0)
        {
            _ui.Write("No tickets purchased this round.");
            return;
        }

        decimal revenue = totalTickets * _settings.TicketPrice;
        decimal grandPrize = revenue * (decimal)_settings.PrizeDistribution.GrandPrizePercentage;
        decimal secondPrize = revenue * (decimal)_settings.PrizeDistribution.SecondTierPercentage;
        decimal thirdPrize = revenue * (decimal)_settings.PrizeDistribution.ThirdTierPercentage;

        decimal roundProfit = revenue * 0.1m;
        LotteryProfit += roundProfit;

        var winners = new List<Player>(_players);
        winners.Sort((a, b) => b.TicketsPurchased.CompareTo(a.TicketsPurchased));

        if (winners.Count > 0) winners[0].Balance += grandPrize;
        if (winners.Count > 1) winners[1].Balance += secondPrize;
        if (winners.Count > 2) winners[2].Balance += thirdPrize;

        _ui.Write($"Prizes distributed: Grand - {grandPrize:C}, Second - {secondPrize:C}, Third - {thirdPrize:C}");
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
            _players.Add(new CPUPlayer($"CPU Player {i + 1}", _settings.StartingBalance, _randomNumberGenerator));
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