using FakeItEasy;
using Lottery.Console.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery.Console.Tests;

public class CPUPlayerTests
{
    private readonly CPUPlayer _cpuPlayer;
    private readonly IRandomNumberGenerator _fakeRandomGenerator;

    public CPUPlayerTests()
    {
        // Arrange: Create a fake random number generator
        _fakeRandomGenerator = A.Fake<IRandomNumberGenerator>();
        _cpuPlayer = new CPUPlayer("CPUPlayer1", 100m, _fakeRandomGenerator); // Starting balance of 100
    }

    [Fact]
    public void PurchaseTickets_CallsRandomGeneratorWithCorrectRange()
    {
        // Arrange
        decimal ticketPrice = 10m;
        int maxTickets = 5;

        // Act
        _cpuPlayer.PurchaseTickets(ticketPrice, maxTickets);

        // Assert
        A.CallTo(() => _fakeRandomGenerator.Next(1, maxTickets + 1)).MustHaveHappenedOnceExactly(); // Ensure random generator is called with correct range
    }

    [Fact]
    public void PurchaseTickets_ExceedsBalance_BuysMaxPossibleTickets()
    {
        // Arrange: CPUPlayer can only afford 2 tickets with a ticket price of 30
        _cpuPlayer.Balance = 50m; // Set balance to 50, which can only afford a max of 1 ticket at this price
        decimal ticketPrice = 30m;
        int maxTickets = 5;
        A.CallTo(() => _fakeRandomGenerator.Next(1, maxTickets + 1)).Returns(3); // Fake random number as 3

        // Act
        _cpuPlayer.PurchaseTickets(ticketPrice, maxTickets);

        // Assert
        Assert.Equal(1, _cpuPlayer.TicketsPurchased); // Should buy only 1 ticket (max affordable)
        Assert.Equal(20m, _cpuPlayer.Balance); // Balance should be 50 - 1 * 30 = 20
    }

    [Fact]
    public void PurchaseTickets_WithEnoughBalance_BuysTicketsBasedOnRandomNumber()
    {
        // Arrange
        decimal ticketPrice = 10m;
        int maxTickets = 5;
        A.CallTo(() => _fakeRandomGenerator.Next(1, maxTickets + 1)).Returns(3); // Fake random number as 3

        // Act
        _cpuPlayer.PurchaseTickets(ticketPrice, maxTickets);

        // Assert
        Assert.Equal(3, _cpuPlayer.TicketsPurchased); // Verify 3 tickets were purchased
        Assert.Equal(70m, _cpuPlayer.Balance); // Balance should be reduced to 100 - (3 * 10) = 70
    }
}