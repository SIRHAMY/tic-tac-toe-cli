
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using CSharpFunctionalExtensions;

public enum Player
{
    Player1,
    Player2
}

public record TicTacToeBoard(IReadOnlyList<IReadOnlyList<Player?>> Board)
{
    public IReadOnlyList<IReadOnlyList<Player?>> GetBoardState()
    {
        return Board;
    }

    public Result<TicTacToeBoard, string> MakeMove(TicTacToeMove move)
    {
        if (move.X < 0 || move.X > 2)
        {
            return Result.Failure<TicTacToeBoard, string>(
                "X must be between 0 and 2");
        }

        if (move.Y < 0 || move.Y > 2)
        {
            return Result.Failure<TicTacToeBoard, string>(
                "Y must be between 0 and 2");
        }

        var currentBoardState = Board[move.X][move.Y];
        if (currentBoardState != null)
        {
            return Result.Failure<TicTacToeBoard, string>(
                $"Must choose an open space! Space already taken by {currentBoardState}");
        }

        var mutableBoard = Board
            .Select(row => row.ToList())
            .ToList();

        mutableBoard[move.X][move.Y] = move.Player;
        return Result.Success<TicTacToeBoard, string>(
            this with { Board = mutableBoard }
        );
    }

    // hamytodo - could have a helper where you pass in coords to check
    // and it just checks all are the same and not null?
    public NextTurn.GameOver? IsGameWon()
    {
        // Check rows
        for (var y = 0; y <= 2; y++)
        {
            var win = Board[0][y] == Board[1][y]
                && Board[1][y] == Board[2][y]
                && Board[1][y] != null;
            if (win)
            {
                return new NextTurn.GameOver(Winner: Board[0][y]);
            }
        }

        // Check columns
        for (var x = 0; x <= 2; x++)
        {
            var win = Board[x][0] == Board[x][1]
                && Board[x][1] == Board[x][2]
                && Board[x][1] != null;
            if (win)
            {
                return new NextTurn.GameOver(Winner: Board[x][0]);
            }
        }

        // Check diagonals
        var diagonalDownRightWin = Board[0][0] == Board[1][1]
            && Board[1][1] == Board[2][2]
            && Board[0][0] != null;
        if (diagonalDownRightWin)
        {
            return new NextTurn.GameOver(Winner: Board[0][0]);
        }

        var diagonalUpRightWin = Board[0][2] == Board[1][1]
            && Board[1][1] == Board[2][0]
            && Board[0][2] != null;
        if (diagonalUpRightWin)
        {
            return new NextTurn.GameOver(Winner: Board[0][2]);
        }

        // Check if any moves left
        var emptySpaces = Board.SelectMany(s => s).Count(s => s == null);
        if(emptySpaces == 0)
        {
            return new NextTurn.GameOver(Winner: null);
        }

        return null;
    }
}

public abstract record NextTurn
{
    public sealed record PlayerTurn(
        Player NextPlayer
    ) : NextTurn;
    public sealed record GameOver(
        Player? Winner
    ) : NextTurn;
}

public record TicTacToeState(
    NextTurn NextTurn,
    TicTacToeBoard Board
);

public record TicTacToeMove(
    Player Player,
    int X,
    int Y
);

public class TicTacToeGame
{
    private TicTacToeState State;

    public TicTacToeGame()
    {
        var gameBoard = Enumerable.Range(0, 3)
            .Select<int, Player?[]>(i => [null, null, null])
            .ToArray();
        this.State = new TicTacToeState(
            NextTurn: new NextTurn.PlayerTurn(Player.Player1),
            // hamytodo - may make sense for tic tac board to create this tbh! 
            Board: new TicTacToeBoard(gameBoard)
        );
    }

    // hamytodo - Could make sense to have a validate outside the makeMove as well
    public Result<NextTurn, string> MakeMove(TicTacToeMove move)
    {
        var isValidMoveForState = this.State.NextTurn switch
        {
            NextTurn.PlayerTurn playerTurn => Result.Success(),
            NextTurn.GameOver gameOver => Result.Failure($"Game is already over - Winner: {gameOver.Winner}"),
            _ => throw new InvalidOperationException($"Got impossible condition: {this.State.NextTurn}")
        };
        if (!isValidMoveForState.IsSuccess)
        {
            return Result.Failure<NextTurn, string>(isValidMoveForState.Error);
        }

        var isValidMoveOnBoard = this.State.Board.MakeMove(move);
        if (!isValidMoveOnBoard.IsSuccess)
        {
            return Result.Failure<NextTurn, string>(isValidMoveOnBoard.Error);
        }

        var newBoard = isValidMoveOnBoard.Value;
        var isGameWon = newBoard.IsGameWon();
        NextTurn nextTurn = isGameWon != null
                ? isGameWon
                : new NextTurn.PlayerTurn(
                    NextPlayer: move.Player == Player.Player1
                        ? Player.Player2
                        : Player.Player1);
        this.State = this.State with
        {
            NextTurn = nextTurn,
            Board = newBoard
        };
        return Result.Success<NextTurn, string>(nextTurn);
    }

    public IReadOnlyList<IReadOnlyList<Player?>> GetCurrentBoard()
    {
        return this.State.Board.GetBoardState();
    }
    
    public NextTurn GetNextTurn()
    {
        return this.State.NextTurn;
    }
}