using FakeItEasy;
using Lottery.Console.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery.Console.Tests;

public class LotteryGameTests
{
    private readonly IRandomNumberGenerator _fakeRandomNumberGenerator;
    private readonly IUserInterface _fakeUI;
    private readonly IOptions<GameSettings> _gameSettings;
    private readonly LotteryGame _lotteryGame;

    public LotteryGameTests()
    {
        // Arrange: Set up fakes and game settings
        _fakeUI = A.Fake<IUserInterface>();
        _fakeRandomNumberGenerator = A.Fake<IRandomNumberGenerator>();

        _gameSettings = Options.Create(new GameSettings
        {
            StartingBalance = 10m,
            TicketPrice = 1m,
            MinCPUPlayers = 1,
            MaxCPUPlayers = 3,
            PrizeDistribution = new PrizeDistribution
            {
                GrandPrizePercentage = 0.5f,
                SecondTierPercentage = 0.3f,
                ThirdTierPercentage = 0.1f
            }
        });

        _lotteryGame = new LotteryGame(_fakeUI, _gameSettings, _fakeRandomNumberGenerator);
    }

    [Fact]
    public void DrawPrizes_DistributesPrizesCorrectly_WithRandomizedWinners()
    {
        // Arrange: Create players with ticket counts
        var player1 = new HumanPlayer(_fakeUI, "Player1", 10m) { TicketsPurchased = 5 };
        var player2 = new CPUPlayer("Player2", 10m, _fakeRandomNumberGenerator, _fakeUI) { TicketsPurchased = 3 };
        var player3 = new CPUPlayer("Player3", 10m, _fakeRandomNumberGenerator, _fakeUI) { TicketsPurchased = 2 };

        _lotteryGame.Players.AddRange(new Player[] { player1, player2, player3 });

        // Calculate total ticket revenue and expected prize amounts
        int totalTickets = player1.TicketsPurchased + player2.TicketsPurchased + player3.TicketsPurchased;
        decimal totalRevenue = totalTickets * _gameSettings.Value.TicketPrice;

        // Define expected prize values based on distribution settings
        decimal grandPrize = totalRevenue * 0.5m;          // 50% for grand prize
        decimal secondTierPool = totalRevenue * 0.3m;      // 30% for second tier
        decimal thirdTierPool = totalRevenue * 0.1m;       // 10% for third tier

        // Define number of winners for each tier
        int secondTierWinnersCount = (int)Math.Round(totalTickets * 0.1m);
        int thirdTierWinnersCount = (int)Math.Round(totalTickets * 0.2m);

        // Calculate per-winner amounts for second and third tier
        decimal secondPrizePerWinner = secondTierWinnersCount > 0 ? secondTierPool / secondTierWinnersCount : 0;
        decimal thirdPrizePerWinner = thirdTierWinnersCount > 0 ? thirdTierPool / thirdTierWinnersCount : 0;

        // Mock the sequence of "random" numbers for predictable testing
        // This sequence should match how DrawPrizes selects winners for grand, second, and third tiers
        A.CallTo(() => _fakeRandomNumberGenerator.Next(A<int>.Ignored, A<int>.Ignored))
            .ReturnsNextFromSequence(0, 1, 2, 0, 1);

        // Act: Run the prize draw
        _lotteryGame.DrawPrizes();

        // Capture player balances after drawing prizes
        var playerBalances = new Dictionary<Player, decimal>
    {
        { player1, player1.Balance - 10m },
        { player2, player2.Balance - 10m },
        { player3, player3.Balance - 10m }
    };

        // Assert: Check prize distribution for correctness

        // Grand Prize: Only one player should have received the grand prize
        Assert.Equal(1, playerBalances.Values.Count(balance => balance == grandPrize));

        // Second Tier: `secondTierWinnersCount` players should have received the second-tier prize
        int actualSecondTierWinners = playerBalances.Values.Count(balance => balance == secondPrizePerWinner);
        Assert.Equal(secondTierWinnersCount, actualSecondTierWinners);

        // Third Tier: `thirdTierWinnersCount` players should have received the third-tier prize
        int actualThirdTierWinners = playerBalances.Values.Count(balance => balance == thirdPrizePerWinner);
        Assert.Equal(thirdTierWinnersCount, actualThirdTierWinners);

        // Total Distribution: Ensure that the sum of distributed prizes matches expected amounts
        decimal totalDistributed = playerBalances.Values.Sum();
        decimal expectedTotalDistributed = grandPrize + secondTierPool + thirdTierPool;
        Assert.Equal(expectedTotalDistributed, totalDistributed);

        // Check if the UI output confirms the prize distribution
        A.CallTo(() => _fakeUI.Write(A<string>.That.Contains("Prizes distributed"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DrawPrizes_NoTicketsPurchased_ShowsNoPurchaseMessage()
    {
        // Arrange: Initialize the game without ticket purchases
        _lotteryGame.InitializeGame();

        // Act
        _lotteryGame.DrawPrizes();

        // Assert: Verify "No tickets purchased" message is displayed
        A.CallTo(() => _fakeUI.Write("No tickets purchased this round.")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void InitializeGame_AddsCorrectNumberOfPlayers()
    {
        // Arrange
        A.CallTo(() => _fakeRandomNumberGenerator.Next(1, 4)).Returns(2); // Expected number of CPU players to add

        // Act
        _lotteryGame.InitializeGame();

        // Assert: Check the expected count (1 human + 2 CPU players)
        Assert.Equal(3, _lotteryGame.Players.Count);
        Assert.IsType<HumanPlayer>(_lotteryGame.Players[0]);
        Assert.All(_lotteryGame.Players.GetRange(1, 2), player => Assert.IsType<CPUPlayer>(player));
    }
}