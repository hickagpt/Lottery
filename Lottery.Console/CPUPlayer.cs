using Lottery.Console.Interfaces;
using System.Security.Cryptography;

namespace Lottery.Console;

public class CPUPlayer : Player
{
    private readonly IRandomNumberGenerator randomNumberGenerator;

    public CPUPlayer(string name, decimal startingBalance, IRandomNumberGenerator randomNumberGenerator)
    {
        Name = name;
        Balance = startingBalance;
        this.randomNumberGenerator = randomNumberGenerator;
    }

    public override void PurchaseTickets(decimal ticketPrice, int maxTickets)
    {
        TicketsPurchased = Math.Min(randomNumberGenerator.Next(1, maxTickets + 1), (int)(Balance / ticketPrice));
        Balance -= TicketsPurchased * ticketPrice;
    }
}