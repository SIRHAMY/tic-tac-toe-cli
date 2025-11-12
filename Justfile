# List available commands
default:
    @just --list

# Run the main project
run *ARGS:
    dotnet run --project TicTacToe/tic-tac-toe-cli.csproj {{ARGS}}

# Run tests
test *ARGS:
    dotnet test {{ARGS}}