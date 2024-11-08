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
    public void DrawPrizes_DistributesPrizesCorrectly()
    {
        // Arrange
        var player1 = new HumanPlayer(_fakeUI, "Player1", 10m) { TicketsPurchased = 5 };
        var player2 = new CPUPlayer("Player2", 10m, _fakeRandomNumberGenerator) { TicketsPurchased = 3 };
        var player3 = new CPUPlayer("Player3", 10m, _fakeRandomNumberGenerator) { TicketsPurchased = 2 };

        _lotteryGame.Players.AddRange(new Player[] { player1, player2, player3 });

        // Act
        _lotteryGame.DrawPrizes();

        // Assert prize distribution
        decimal totalRevenue = (player1.TicketsPurchased + player2.TicketsPurchased + player3.TicketsPurchased) * _gameSettings.Value.TicketPrice;
        Assert.Equal(10m + (totalRevenue * 0.5m), player1.Balance); // Grand prize
        Assert.Equal(10m + (totalRevenue * 0.3m), player2.Balance); // Second prize
        Assert.Equal(10m + (totalRevenue * 0.1m), player3.Balance); // Third prize

        // Check the correct message was displayed
        A.CallTo(() => _fakeUI.Write(A<string>.That.Contains("Prizes distributed"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DrawPrizes_HouseEarns10PercentProfit()
    {
        // Arrange
        var player1 = new HumanPlayer(_fakeUI, "Player1", 10m) { TicketsPurchased = 4 };
        var player2 = new CPUPlayer("Player2", 10m, _fakeRandomNumberGenerator) { TicketsPurchased = 6 };

        _lotteryGame.Players.AddRange(new Player[] { player1, player2 });

        // Act
        _lotteryGame.DrawPrizes();

        // Calculate expected profit (10% of total revenue)
        decimal totalRevenue = (player1.TicketsPurchased + player2.TicketsPurchased) * _gameSettings.Value.TicketPrice;
        decimal expectedProfit = totalRevenue * 0.1m;

        // Assert: Check that LotteryProfit reflects the 10% profit
        Assert.Equal(expectedProfit, _lotteryGame.LotteryProfit);

        // Verify the profit message was displayed
        A.CallTo(() => _fakeUI.Write(A<string>.That.Contains($"Lottery profit for this round: {expectedProfit:C}"))).MustHaveHappened();
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