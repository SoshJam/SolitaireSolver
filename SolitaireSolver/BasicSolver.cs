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
            };

            // Try everything
            Stumped = false;
            foreach (Func<string> check in checks)
            {
                string result = check();
                if (result != "NO")
                    return result;
            }

            // If we haven't returned, we are stumped.
            Stumped = true;
            return "cycle";
        }

        // Step Methods
        // These return a command if the move is possible, or "NO" if not.

        // First: Try to add to a foundation pile from something on the board
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

            // Prioritize the pile with the most face-down cards
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
                if (board[i].Count > 0)
                    bottoms[i] = board[i].First();
                else
                    bottoms[i] = '\0';
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
