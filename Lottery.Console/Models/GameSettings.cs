public class GameSettings
{
    public int MaxCPUPlayers { get; set; }
    public int MaxTicketsPerRound { get; set; }
    public int MinCPUPlayers { get; set; }
    public int MinTicketsPerRound { get; set; }
    public PrizeDistribution PrizeDistribution { get; set; }
    public decimal StartingBalance { get; set; }
    public decimal TicketPrice { get; set; }
}