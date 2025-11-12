namespace TicTacToe.Tests;

public class TicTacToeTests
{
    [Fact]
    public void Test1()
    {
        var gameEngine = new TicTacToeGame();

        var move = new TicTacToeMove(
            Player: Player.Player1,
            X: 0,
            Y: 0
        );
        var result = gameEngine.MakeMove(move);
        Assert.True(result.IsSuccess);
    }
}
