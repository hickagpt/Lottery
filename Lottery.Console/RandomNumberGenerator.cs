using Lottery.Console.Interfaces;

namespace Lottery.Console;

public class RandomNumberGenerator : IRandomNumberGenerator
{
    private static readonly Random _random = new Random();

    public int Next(int min, int max)
    {
        return _random.Next(min, max);
    }

    public int Next()
    {
        return _random.Next();
    }
}