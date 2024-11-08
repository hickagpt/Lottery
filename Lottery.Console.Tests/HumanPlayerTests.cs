using FakeItEasy;
using Lottery.Console;
using Xunit;

namespace Lottery.Console.Tests;

public class HumanPlayerTests
{
    private readonly IUserInterface _fakeUserInterface;
    private readonly HumanPlayer _humanPlayer;

    public HumanPlayerTests()
    {
        _fakeUserInterface = A.Fake<IUserInterface>();

        _humanPlayer = new HumanPlayer(_fakeUserInterface, "Player1", 10m);
    }

    [Fact]
    public void PurchaseTickets_ExceedsBalance_BuysMaxPossible()
    {
        // Arrange
        var humanPlayer = new HumanPlayer(_fakeUserInterface, "Player1", 1m);

        decimal ticketPrice = 1m;
        int maxTickets = 10;
        int ticketsRequested = 3;
        A.CallTo(() => _fakeUserInterface.Read()).Returns(ticketsRequested.ToString());
        A.CallTo(() => _fakeUserInterface.Write(A<string>.Ignored)).DoesNothing();

        // Act
        humanPlayer.PurchaseTickets(ticketPrice, maxTickets);

        // Assert
        Assert.Equal(1, humanPlayer.TicketsPurchased);
        Assert.Equal(0m, humanPlayer.Balance);
        A.CallTo(() => _fakeUserInterface.Write(A<string>.That.Contains("purchased 1 tickets"))).MustHaveHappened();
    }

    [Fact]
    public void PurchaseTickets_InvalidInput_DoesNotUpdateBalance()
    {
        // Arrange
        decimal ticketPrice = 1m;
        int maxTickets = 10;
        string invalidInput = "invalid";
        A.CallTo(() => _fakeUserInterface.Read()).Returns(invalidInput);
        A.CallTo(() => _fakeUserInterface.Write(A<string>.Ignored)).DoesNothing();

        // Act
        _humanPlayer.PurchaseTickets(ticketPrice, maxTickets);

        // Assert
        Assert.Equal(0, _humanPlayer.TicketsPurchased);
        Assert.Equal(10m, _humanPlayer.Balance);
        A.CallTo(() => _fakeUserInterface.Write(A<string>.That.Contains("Invalid purchase amount"))).MustHaveHappened();
    }

    [Fact]
    public void PurchaseTickets_RequestsMoreThanMaxTickets_BuysMaxTickets()
    {
        // Arrange
        decimal ticketPrice = 1m;
        int maxTickets = 10;
        int ticketsRequested = 11;
        A.CallTo(() => _fakeUserInterface.Read()).Returns(ticketsRequested.ToString());
        A.CallTo(() => _fakeUserInterface.Write(A<string>.Ignored)).DoesNothing();

        // Act
        _humanPlayer.PurchaseTickets(ticketPrice, maxTickets);

        // Assert
        Assert.Equal(10, _humanPlayer.TicketsPurchased);
        Assert.Equal(0m, _humanPlayer.Balance);
        A.CallTo(() => _fakeUserInterface.Write(A<string>.That.Contains("purchased 10 tickets"))).MustHaveHappened();
    }

    [Fact]
    public void PurchaseTickets_ValidInput_UpdatesBalanceAndTickets()
    {
        // Arrange
        decimal ticketPrice = 1m;
        int maxTickets = 10;
        int ticketsRequested = 5;
        A.CallTo(() => _fakeUserInterface.Read()).Returns(ticketsRequested.ToString());
        A.CallTo(() => _fakeUserInterface.Write(A<string>.Ignored)).DoesNothing();

        // Act
        _humanPlayer.PurchaseTickets(ticketPrice, maxTickets);

        // Assert
        Assert.Equal(5, _humanPlayer.TicketsPurchased);
        Assert.Equal(5m, _humanPlayer.Balance);
        A.CallTo(() => _fakeUserInterface.Write(A<string>.That.Contains("purchased 5 tickets"))).MustHaveHappened();
    }
}