using Microsoft.Extensions.Hosting;

public class LotteryGameService : IHostedService
{
    private readonly LotteryGame _lotteryGame;

    public LotteryGameService(LotteryGame lotteryGame)
    {
        _lotteryGame = lotteryGame;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lotteryGame.InitializeGame();
        _lotteryGame.RunGameLoop();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}