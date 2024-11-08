namespace Lottery.Console;

public class ConsoleUserInterface : IUserInterface
{
    public string? Read() => System.Console.ReadLine();

    public void Write(string message) => System.Console.WriteLine(message);
}