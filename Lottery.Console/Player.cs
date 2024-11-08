public abstract class Player
{
    public decimal Balance { get; set; }
    public string Name { get; set; }
    public int TicketsPurchased { get; set; }

    public abstract void PurchaseTickets(decimal ticketPrice, int maxTickets);
}