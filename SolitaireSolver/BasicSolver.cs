using System.Runtime.CompilerServices;

namespace SolitaireSolver
{
    public class BasicSolver : AbstractSolver
    {
        public BasicSolver(ISolitaire newGame) : base(newGame) { }

        public override string CalculateNextMove()
        {
            Update();

            // The things we will try, in order from most to least important.
            Func<string>[] checks = {
                tryAddToFoundationPilesFromBoard,
                tryAddToFoundationPilesFromStock,
                tryMoveKing,
                tryMoveAndReveal,
                tryKingToBoard,
                tryStockToBoardForMoveNextTurn,
                trySearchForMoveNextTurn,
            };

            SolverState lastState = state;

            // If we suggested a move last turn
            if (lastState == SolverState.Normal)
            {
                // Reset the stump char
                cycleStart = '#';
            }

            // Try everything
            state = SolverState.Normal;
            foreach (Func<string> check in checks)
            {
                string result = check();
                if (result != "NO")
                    return result;
            }

            // If we haven't returned, we are stumped or should give up.
            // If we already gave up, stay that way.
            if (lastState == SolverState.GaveUp)
            {
                state = SolverState.GaveUp;
                return "reset";
            }

            // Check if we should give up
            if (stock == cycleStart)
            {
                state = SolverState.GaveUp;
                return "reset";
            }

            // Otherwise, we press on.
            if (cycleStart == '#' || cycleStart == '\0')
                cycleStart = stock;

            state = SolverState.Stumped;
            return "cycle";
        }

        // Step Methods
        // These return a command if the move is possible, or "NO" if not.

        // 01: Try to add to a foundation pile from something on the board - O(N)
        protected string tryAddToFoundationPilesFromBoard()
        {
            char[] tops = getPileTops();
            int[] facedown = getFaceDownCardsOnBoard();
            bool[] canAdd = new bool[7];

            // See what piles we can take from
            int minimumFoundationValue = getMinimumFoundationValue();
            for (int i = 0; i < 7; i++)
                if (Card.GetValue(tops[i]) == minimumFoundationValue + 1)
                    canAdd[i] = true;

            // Find the pile with the most face-down cards
            int mostFaceDownCards = 0;
            for (int i = 0; i < 7; i++)
                if (canAdd[i] && facedown[i] > mostFaceDownCards)
                    mostFaceDownCards = facedown[i];

            // Return the rightmost column with the most facedown cards
            for (int i = 6; i >= 0; i--)
                if (canAdd[i] && facedown[i] == mostFaceDownCards)
                    return "btf " + i;

            return "NO";
        }

        // 01: Try to add to a foundation pile from the stock - O(1)
        protected string tryAddToFoundationPilesFromStock()
        {
            int minimumFoundationValue = getMinimumFoundationValue();

            // We can add if this is 1 above the minimum
            if (Card.GetValue(stock) == minimumFoundationValue + 1)
                return "stf";

            return "NO";
        }

        // 02: Move a king to an empty column, if its has face-down cards in its column
        protected string tryMoveKing()
        {
            char[] bottoms = getPileBottoms();
            List<int> needsMoving = new List<int>(); // columns with kings that need moving
            List<int> emptyColumns = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                // Column is empty
                if (board[i].Count == 0)
                {
                    emptyColumns.Add(i);
                    continue;
                }

                // King atop a face-down card
                if (Card.GetValue(bottoms[i]) == 13 && board[i][0] == '#')
                {
                    needsMoving.Add(i);
                    continue;
                }
            }

            // If we can move something, do it
            if (needsMoving.Count > 0 && emptyColumns.Count > 0)
            {
                int targetColumn = emptyColumns.Min();
                int[] facedown = getFaceDownCardsOnBoard();

                // Find the pile with the most face-down cards
                int mostFaceDownCards = 0;
                for (int i = 0; i < 7; i++)
                    if (needsMoving.Contains(i) && facedown[i] > mostFaceDownCards)
                        mostFaceDownCards = facedown[i];

                for (int i = 0; i < 7; i++)
                    if (needsMoving.Contains(i) && facedown[i] == mostFaceDownCards)
                        return "move " + i + " " + targetColumn;
            }

            return "NO";
        }

        // 03: Move a stack on the board to reveal a face-down card - O(N) I think
        protected string tryMoveAndReveal()
        {
            char[] bottoms = getPileBottoms();
            char[] tops = getPileTops();
            int[] facedown = getFaceDownCardsOnBoard();
            bool[] canMove = new bool[7];

            // Loop through bottoms, see if they have a matching top
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                    if (j != i && isValidCombo(bottoms[i], tops[j]))
                        // We can move this stack
                        canMove[i] = true;

            // Find the pile with the most face-down cards
            int mostFaceDownCards = 0;
            for (int i = 0; i < 7; i++)
                if (canMove[i] && facedown[i] > mostFaceDownCards)
                    mostFaceDownCards = facedown[i];

            // Move the rightmost column with the most face-down cards
            for (int i = 6; i >= 0; i--)
                if (canMove[i] && facedown[i] == mostFaceDownCards)
                    for (int j = 0; j < 7; j++)
                        if (j != i && isValidCombo(bottoms[i], tops[j]))
                            return "move " + i + " " + j;

            return "NO";
        }

        // 04: Move a king from the stock to an empty column on the board
        protected string tryKingToBoard()
        {
            if (Card.GetValue(stock) == 13)
                for (int i = 0; i < 7; i++)
                    if (board[i].Count == 0)
                        return "stb " + i;

            return "NO";
        }

        // 05: Move from waste pile to board that will allow a move next turn
        protected string tryStockToBoardForMoveNextTurn()
        {
            char[] bottoms = getPileBottoms();
            char[] tops = getPileTops();

            // Find piles we could put the stock on
            List<int> targetPileTops = new List<int>();
            for (int i = 0; i < 7; i++)
                if (isValidCombo(stock, tops[i]))
                    targetPileTops.Add(i);

            // Find piles that could be placed on this stock
            List<int> targetPileBottoms = new List<int>();
            for (int i = 0; i < 7; i++)
                if (isValidCombo(bottoms[i], stock))
                        targetPileBottoms.Add(i);

            // If there is something in both lists, we should add this to the board.
            if (targetPileTops.Count > 0 && targetPileBottoms.Count > 0)
                return "stb " + targetPileTops[0];

            return "NO";
        }

        // 06: Move from waste pile to board that will allow a move after a while

        // 07: Cycle through, knowing there's something that would allow us to move immediately after playing it
        protected string trySearchForMoveNextTurn()
        {
            char[] bottoms = getPileBottoms();
            char[] tops = getPileTops();

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (i == j) continue;

                    // The bottom of a pile must be the same color and 2 below the top of a different pile.
                    if (Card.IsBlack(bottoms[i]) != Card.IsBlack(tops[j])) continue;

                    if (Card.GetValue(bottoms[i]) + 2 != Card.GetValue(tops[j])) continue;

                    // If we're still here, we found one.

                    // Find the cards that could fill the gaps.
                    char[] fillers = Card.FromColorAndValue(!Card.IsBlack(bottoms[i]), Card.GetValue(bottoms[i]) + 1);

                    /*
                    // If either card is in stock, cycle the stock.
                    if (ContainsAny(cardsInStock, fillers)) {
                        Console.WriteLine("Looking for: " + Card.ToString(fillers[0]) + " or " + Card.ToString(fillers[1]));
                        return "cycle";
                    }
                    */
                    if (cardsInStock.Contains(fillers[0]))
                    {
                        Console.WriteLine("Looking for: " + Card.ToString(fillers[0]));
                        return "cycle";
                    }
                    if (cardsInStock.Contains(fillers[1]))
                    {
                        Console.WriteLine("Looking for: " + Card.ToString(fillers[1]));
                        return "cycle";
                    }

                    continue;
                }
            }

            return "NO";
        }

        // Helper Methods

        /// <summary>
        /// Finds the minimum value of the cards in the foundation piles.
        /// </summary>
        /// <returns>The value of the card atop smallest foundation pile.</returns>
        protected int getMinimumFoundationValue()
        {
            int min = 13;
            for (int i = 0; i < 4; i++)
                if (Card.GetValue(foundationPiles[i]) < min)
                    min = Card.GetValue(foundationPiles[i]);
            return min;
        }

        /// <summary>
        /// Finds the bottom (highest value) card of each column.
        /// </summary>
        /// <returns>An array of 7 bottom cards.</returns>
        protected char[] getPileBottoms()
        {
            char[] bottoms = new char[7];
            for (int i = 0; i < 7; i++)
            {
                if (board[i].Count > 0)
                {
                    foreach (char c in board[i])
                    {
                        if (bottoms[i] == '\0' && c != '#')
                            bottoms[i] = c;
                    }
                }
                else
                {
                    bottoms[i] = '\0';
                }
            }
            return bottoms;
        }

        /// <summary>
        /// Finds the top (lowest value) card of each column.
        /// </summary>
        /// <returns>An array of 7 top cards.</returns>
        protected char[] getPileTops()
        {
            char[] tops = new char[7];
            for (int i = 0; i < 7; i++)
                if (board[i].Count > 0)
                    tops[i] = board[i].Last();
                else
                    tops[i] = '\0';
            return tops;
        }

        /// <summary>
        /// Finds the number of face-down cards on the board.
        /// </summary>
        /// <returns>An array of 7 integers representing the amount of face-down cards in that column.</returns>
        protected int[] getFaceDownCardsOnBoard()
        {
            int[] facedown = new int[7];
            for (int i = 0; i < 7; i++)
                foreach (char c in board[i])
                    if (c == '#')
                        facedown[i]++;
                    else
                        break;
            return facedown;
        }

        /// <summary>
        /// Checks if a card can be played on top of another card. The top card must be a different color and have 1 less value.
        /// </summary>
        /// <param name="top">The card to be placed on top.</param>
        /// <param name="bottom">The bottom card.</param>
        /// <returns>True if the top card can be placed on the bottom card.</returns>
        protected bool isValidCombo(char top, char bottom)
        {
            return (Card.IsBlack(top) != Card.IsBlack(bottom)) && (Card.GetValue(top) + 1 == Card.GetValue(bottom));
        }

        /// <summary>
        /// Checks if the card can be placed somewhere on the board.
        /// </summary>
        /// <param name="card">The card to place.</param>
        /// <returns>True if the card fits somewhere.</returns>
        protected bool canBePlacedOnBoard(char card)
        {
            char[] tops = getPileTops();
            foreach(char t in tops)
                if(isValidCombo(card, t))
                    return true;
            return false;
        }

        /// <summary>
        /// Extension method for checking if a collection contains anything from a collection.
        /// </summary>
        /// <typeparam name="T">Ideally only primitives, but can be anything that IEnumerable.Contains() works on.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="inputs">The collection to check for.</param>
        /// <returns>True if any elements in inputs are in source</returns>
        protected static bool ContainsAny(IEnumerable<char> source, IEnumerable<char> inputs)
        {
            foreach (char i in inputs)
                if (source.Contains(i))
                    return true;
            return false;
        }
    }
}