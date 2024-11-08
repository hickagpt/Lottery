namespace Lottery.Console.Interfaces;

public interface IRandomNumberGenerator
{
    int Next(int min, int max);
}