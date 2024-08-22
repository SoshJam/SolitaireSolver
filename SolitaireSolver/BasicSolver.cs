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
            {
                for (int j = 0; j < 7; j++)
                {
                    // Skip the same column
                    if (j == i) continue;

                    // Skip matching colors
                    if (Card.IsBlack(bottoms[i]) == Card.IsBlack(tops[j])) continue;

                    // Skip mismatched values
                    if (Card.GetValue(bottoms[i]) != Card.GetValue(tops[j]) - 1) continue;

                    // We can move this stack
                    canMove[i] = true;
                }
            }

            // Find the pile with the most face-down cards
            int mostFaceDownCards = 0;
            for (int i = 0; i < 7; i++)
                if (canMove[i] && facedown[i] > mostFaceDownCards)
                    mostFaceDownCards = facedown[i];

            // Move the rightmost column with the most face-down cards
            for (int i = 6; i >= 0; i--)
            {
                if (canMove[i] && facedown[i] == mostFaceDownCards)
                {
                    // Loop through to find the right column again
                    for (int j = 0; j < 7; j++)
                    {
                        // Skip the same column
                        if (j == i) continue;

                        // Skip matching colors
                        if (Card.IsBlack(bottoms[i]) == Card.IsBlack(tops[j])) continue;

                        // Skip mismatched values
                        if (Card.GetValue(bottoms[i]) != Card.GetValue(tops[j]) - 1) continue;

                        return "move " + i + " " + j;
                    }
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
        /// <returns>An array of 7 integers</returns>
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
    }
}
