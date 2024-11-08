namespace Lottery.Console;

public class HumanPlayer : Player
{
    private readonly IUserInterface _ui;

    public HumanPlayer(IUserInterface ui, string name, decimal startingBalance)
    {
        _ui = ui;
        Name = name;
        Balance = startingBalance;
    }

    public override void PurchaseTickets(decimal ticketPrice, int maxTickets)
    {
        _ui.Write($"{Name}, your balance is {Balance:C}. Tickets cost {ticketPrice:C}. How many tickets would you like to buy?");

        if (int.TryParse(_ui.Read(), out int ticketsRequested))
        {
            // Calculate the maximum tickets the player can afford
            int maxAffordableTickets = (int)(Balance / ticketPrice);

            // Determine the actual number of tickets to purchase, respecting both maxTickets and available balance
            TicketsPurchased = Math.Min(ticketsRequested, Math.Min(maxTickets, maxAffordableTickets));

            if (TicketsPurchased > 0)
            {
                Balance -= TicketsPurchased * ticketPrice;
                _ui.Write($"{Name} purchased {TicketsPurchased} tickets. Remaining balance: {Balance:C}");
            }
            else
            {
                _ui.Write("Insufficient funds to purchase any tickets.");
            }
        }
        else
        {
            _ui.Write("Invalid purchase amount. Try again.");
        }
    }
}