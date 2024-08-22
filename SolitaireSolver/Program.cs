﻿using SolitaireSolver;

Console.WriteLine("Starting Game");

string FormatBoard(ISolitaire game)
{
    List<char>[] board = game.GetBoard();
    char[] foundations = game.GetFoundationPiles();
    char stock = game.PeekStock();

    // Display the stock pile and foundation piles
    string firstLine = "\n## " + Card.ToString(stock) + "    ";
    foreach (char f in foundations)
    {
        firstLine += Card.ToString(f) + " ";
    }

    // Divider line
    string secondLine = "-0--1--2--3--4--5--6\n";

    // Display the board

    // Find the tallest pile
    int tallestPileSize = 0;
    for (int i = 0; i < 7; i++)
        if (board[i].Count > tallestPileSize)
            tallestPileSize = board[i].Count;

    // Fill in rows
    string outputString = firstLine + "\n" + secondLine;

    // Loop through down first
    for (int y = 0; y < tallestPileSize; y++)
    {
        // Then across
        for (int x = 0; x < 7; x++)
        {
            // If there's no card here leave a space
            if (board[x].Count <= y)
            {
                outputString += "   ";
            }
            // If the card is face down
            else if (board[x][y] == '#')
            {
                outputString += "## ";
            }
            // Otherwise just display the card.
            else
            {
                outputString += Card.ToString(board[x][y]) + " ";
            }
        }
        outputString += "\n";
    }

    return outputString;
}

// Setup Game
ISolitaire game = new BasicSolitaire();

Console.WriteLine(FormatBoard(game));
Console.WriteLine("Solver recommends: " + BasicSolver.CalculateNextMove(game));

// Main Loop
string? query;
while (true)
{
    query = Console.ReadLine();
    if (query == null)
        continue;

    string command = query.Split(' ')[0];
    string[] parameters = query.Split(' ').Skip(1).ToArray();

    if (command == "exit")
        break;

    // View the board
    if (command == "board")
    {
        // do nothing, because it will print later.
    }

    // Cycle the stock pile
    else if (command == "cycle")
    {
        game.CycleStock();
    }

    // Send the stock pile to a foundation pile
    else if (command == "stf")
    {
        char revealed;
        bool success = game.StockToFoundations(out revealed);

        if (!success)
        {
            Console.WriteLine("This action is not valid.");
            continue;
        }
        else if (revealed != '\0')
        {
            Console.WriteLine("The card " + Card.ToString(revealed) + " was revealed.");
        }
    }

    // Send a board card to a foundation pile
    else if (command == "btf")
    {
        if (parameters.Length != 1)
        {
            Console.WriteLine("Usage: btf <column>");
            continue;
        }

        int? col = int.Parse(parameters[0]);
        if (col == null || col < 0 || col > 6)
        {
            Console.WriteLine("Column must be an integer between 0 and 6.");
            continue;
        }

        char revealed;
        bool success = game.BoardToFoundations((int)col, out revealed);
        if (!success)
        {
            Console.WriteLine("This action is not valid.");
            continue;
        }
        else if (revealed != '\0')
        {
            Console.WriteLine("The card " + Card.ToString(revealed) + " was revealed.");
        }
    }

    // Send a foundation pile card to the board
    else if (command == "ftb")
    {
        if (parameters.Length != 2)
        {
            Console.WriteLine("Usage: ftb <suit> <column>");
            continue;
        }

        int? suit = int.Parse(parameters[0]);
        if (suit == null || suit < 0 || suit > 3)
        {
            Console.WriteLine("Suit must be an integer between 0 and 3.");
            continue;
        }

        int? col = int.Parse(parameters[1]);
        if (col == null || col < 0 || col > 6)
        {
            Console.WriteLine("Column must be an integer between 0 and 6.");
            continue;
        }

        bool success = game.FoundationsToBoard((Suit)suit, (int)col);
        if (!success)
        {
            Console.WriteLine("This action is not valid.");
            continue;
        }
    }

    // Move a card from the stock pile to the board
    else if (command == "stb")
    {
        if (parameters.Length != 1)
        {
            Console.WriteLine("Usage: stb <column>");
            continue;
        }

        int? col = int.Parse(parameters[0]);
        if (col == null || col < 0 || col > 6)
        {
            Console.WriteLine("Column must be an integer between 0 and 6.");
            continue;
        }
        char revealed;
        bool success = game.StockToBoard((int)col, out revealed);

        if (!success)
        {
            Console.WriteLine("This action is not valid.");
            continue;
        }
        else if (revealed != '\0')
        {
            Console.WriteLine("The card " + Card.ToString(revealed) + " was revealed.");
        }
    }

    // Move a card
    else if (command == "move")
    {
        if (parameters.Length != 2 && parameters.Length != 3)
        {
            Console.WriteLine("Usage: move <start> <end> [offset=0]");
            continue;
        }


        int? start = int.Parse(parameters[0]);
        if (start == null || start < 0 || start > 6)
        {
            Console.WriteLine("Start column must be an integer between 0 and 6.");
            continue;
        }

        int? end = int.Parse(parameters[1]);
        if (end == null || end < 0 || end > 6)
        {
            Console.WriteLine("End column must be an integer between 0 and 6.");
            continue;
        }

        int? offset = 0;
        if (parameters.Length > 2)
        {
            offset = int.Parse(parameters[2]);
            if (offset == null)
            {
                offset = 0;
            }
            if (offset < 0)
            {
                Console.WriteLine("Offset must be positive.");
                continue;
            }
        }

        char revealed;
        bool success = game.MovePile((int)start, (int)end, out revealed, (int)offset);


        if (!success)
        {
            Console.WriteLine("This action is not valid.");
            continue;
        }
        else if (revealed != '\0')
        {
            Console.WriteLine("The card " + Card.ToString(revealed) + " was revealed.");
        }
    }

    // Reset the game.
    else if (command == "reset")
    {
        game.Reset();
    }

    else
    {
        Console.WriteLine("Unknown command.");
        continue;
    }

    Console.WriteLine(FormatBoard(game));
    Console.WriteLine("Solver recommends: " + BasicSolver.CalculateNextMove(game));
}