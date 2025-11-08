// See https://aka.ms/new-console-template for more information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

Console.WriteLine("Let's play tic-tac-toe");

var gameEngine = new TicTacToeGame();

/*
Outer loop: 
* while true
    * Print current turn
    * Print current game state
    * Await input
    * Make move
    * Based on result
        * If game over -> congrats and exit
        * If next turn -> continue
*/

while (true)
{
    Console.WriteLine("--------------------");
    Console.WriteLine("Current game state: ");
    GameHelpers.PrintGameBoard(gameEngine.GetCurrentBoard());

    var nextTurn = gameEngine.GetNextTurn();

    if (nextTurn is NextTurn.GameOver gameOver)
    {
        Console.WriteLine("Game over!");
        if(gameOver.Winner != null)
        {
            Console.WriteLine($"Winner is: {gameOver.Winner}");
        } else
        {
            Console.WriteLine("It's a draw!");
        }
        
        return;
    }
    else if (nextTurn is NextTurn.PlayerTurn playerTurn)
    {
        Console.WriteLine($"Next turn: {playerTurn.NextPlayer}");
        Console.WriteLine($"Input move as X Y (e.g. 0 1)");
        
        var input = Console.ReadLine();
        var inputCoordinatesResult = GameHelpers.ParseGameInputToCoordinates(input);
        if (inputCoordinatesResult.IsFailure)
        {
            Console.WriteLine(inputCoordinatesResult.Error);
            continue;
        }

        var moveCoordinates = inputCoordinatesResult.Value;
        var playerMove = new TicTacToeMove(
            Player: playerTurn.NextPlayer,
            X: moveCoordinates.X,
            Y: moveCoordinates.Y);
        var moveResult = gameEngine.MakeMove(move: playerMove);
        if(moveResult.IsFailure)
        {
            Console.WriteLine($"Invalid move: {moveResult.Error}");
            continue;
        }
    }
    else
    {
        throw new InvalidOperationException($"Unhandled next turn received! {nextTurn}");
    }
}

public record Coordinates(int X, int Y);

public static class GameHelpers
{
    public static Result<Coordinates, string> ParseGameInputToCoordinates(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result.Failure<Coordinates, string>("No input received.");
        }

        var parts = input.Split(" ");
        if (parts.Length != 2)
        {
            return Result.Failure<Coordinates, string>("Received invalid format. Input move as X Y (e.g. 0 1)");
        }

        var xOkay = int.TryParse(parts[0], out int x);
        var yOkay = int.TryParse(parts[1], out int y);
        if (!xOkay || !yOkay)
        {
            Console.WriteLine("Invalid numbers provided");
        }

        return Result.Success<Coordinates, string>(new Coordinates(
            X: x,

            Y: y
        ));
    }

    public static string PlayerToGameBoardSymbol(
        Player? player
    )
    {
        return player switch
        {
            Player.Player1 => "1",
            Player.Player2 => "2",
            null => "0",
            _ => throw new InvalidOperationException($"Got unhandled player! {player}")
        };
    }
    public static void PrintGameBoard(
        IReadOnlyList<IReadOnlyList<Player?>> board
    )
    {
        // hamytodo - could make this more dynamic but whatevs
        Player?[][] rows = Enumerable.Range(0, 3)
            .Select<int, Player?[]>(y => [board[0][y], board[1][y], board[2][y]])
            .ToArray();

        foreach (var row in rows)
        {
            var rowSymbols = row
                .Select(s => PlayerToGameBoardSymbol(s))
                .ToList();

            Console.WriteLine(string.Join(" | ", rowSymbols));
        }
    }
}