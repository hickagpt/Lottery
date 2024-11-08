using Lottery.Console.Interfaces;
using System.Security.Cryptography;

namespace Lottery.Console;

public class CPUPlayer : Player
{
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IUserInterface _ui;

    public CPUPlayer(string name, decimal startingBalance, IRandomNumberGenerator randomNumberGenerator, IUserInterface ui)
    {
        Name = name;
        Balance = startingBalance;
        _ui = ui;
        _randomNumberGenerator = randomNumberGenerator;
    }

    public override void PurchaseTickets(decimal ticketPrice, int maxTickets)
    {
        TicketsPurchased = Math.Min(_randomNumberGenerator.Next(1, maxTickets + 1), (int)(Balance / ticketPrice));
        Balance -= TicketsPurchased * ticketPrice;
        _ui.Write($"{Name} purchased {TicketsPurchased} tickets. Remaining balance: {Balance:C}");
    }
}