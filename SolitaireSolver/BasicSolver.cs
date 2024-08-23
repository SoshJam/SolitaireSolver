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
                checkVictory,
                tryAddToFoundationPilesFromBoard,
                tryAddToFoundationPilesFromStock,
                tryMoveKing,
                tryMoveAndReveal,
                tryKingToBoard,
                tryStockToBoardForMoveNextTurn,
                tryAddPartOfChain,
                trySearchForMoveNextTurn,
                trySearchForChainsInStock,
                seeStockPile,
                tryAddCurrentStock,
                tryAddAnythingToFoundationsFromBoard,
                tryAddAnythingToFoundationsFromStock,
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
                return "reset (Stuck)";
            }

            // Check if we should give up
            if (stock == cycleStart)
            {
                state = SolverState.GaveUp;
                return "reset (Stuck)";
            }

            // Otherwise, we press on.
            if (cycleStart == '#' || cycleStart == '\0')
                cycleStart = stock;

            state = SolverState.Stumped;
            return "cycle (Stumped)";
        }

        // Step Methods
        // These return a command if the move is possible, or "NO" if not.

        // 00: Check to see if we've already one
        protected string checkVictory()
        {
            return getMinimumFoundationValue() == 13 ? "reset" : "NO";
        }

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

        // 02: Try to add to a foundation pile from the stock - O(1)
        protected string tryAddToFoundationPilesFromStock()
        {
            int minimumFoundationValue = getMinimumFoundationValue();

            // We can add if this is 1 above the minimum
            if (Card.GetValue(stock) == minimumFoundationValue + 1)
                return "stf";

            return "NO";
        }

        // 03: Move a king to an empty column, if its has face-down cards in its column
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

        // 04: Move a stack on the board to reveal a face-down card - O(N) I think
        protected string tryMoveAndReveal()
        {
            char[] bottoms = getPileBottoms();
            char[] tops = getPileTops();
            int[] facedown = getFaceDownCardsOnBoard();
            bool[] canMove = new bool[7];

            // Loop through bottoms, see if they have a matching top
            for (int i = 0; i < 7; i++)
                if (bottoms[i] != '\0')
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

        // 05: Move a king from the stock to an empty column on the board
        protected string tryKingToBoard()
        {
            if (Card.GetValue(stock) == 13)
                for (int i = 0; i < 7; i++)
                    if (board[i].Count == 0)
                        return "stb " + i;

            return "NO";
        }

        // 06: Move from waste pile to board that will allow a move next turn
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
                return "stb " + targetPileTops[0] + " (Allows an immediate move)";

            return "NO";
        }

        // 07: Move from waste pile to board that will allow a move after a while
        protected string tryAddPartOfChain()
        {
            // Check if it even can be added
            if (!canBePlacedOnBoard(stock)) return "NO";

            // Find the column it could be placed in
            int targetColumn = 0;
            char[] tops = getPileTops();
            for (int i = 0; i < 7; i++)
                if (isValidCombo((char)stock, tops[i]))
                    targetColumn = i;

            // Find the bottoms of the piles and see if this connects with anything
            char[] bottoms = getPileBottoms();
            for (int i = 0; i < 7; i++)
            {
                if (i == targetColumn) continue;

                // Check if the cards can be chained
                if (checkStockForChain(stock, bottoms[i]))
                    return "stb " + targetColumn + " (Part of a chain)"; // We should take this card if so
            }

            return "NO";
        }

        // 08: Cycle through, knowing there's something that would allow us to move immediately after playing it
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

                    // If either card is in stock, cycle the stock.
                    if (ContainsAny(cardsInStock, fillers)) {
                        // Many returns so we can say the exact card we're looking for
                        if (!cardsInStock.Contains(fillers[0]))
                            return $"cycle (Looking for {Card.ToString(fillers[1])})";
                        if (!cardsInStock.Contains(fillers[1]))
                            return $"cycle (Looking for {Card.ToString(fillers[0])})";
                        return $"cycle (Looking for {Card.ToString(fillers[0])} or {Card.ToString(fillers[1])})";
                    }

                    continue;
                }
            }

            return "NO";
        }

        // 09: Cycle through, knowing there's something that would allow us to move after adding a few cards in a chain
        protected string trySearchForChainsInStock()
        {
            char[] bottoms = getPileBottoms();
            char[] tops = getPileTops();

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (i == j) continue;

                    // Check if the cards can be chained via the stock pile
                    if (!checkStockForChain(tops[i], bottoms[j]))
                        continue;

                    // Return exactly the cards we are looking for
                    char[] nextChains = Card.FromColorAndValue(!Card.IsBlack(tops[i]), Card.GetValue(tops[i]) - 1);
                    if (!cardsInStock.Contains(nextChains[0]))
                        return $"cycle (Looking for {Card.ToString(nextChains[1])}. Chaining {Card.ToString(tops[i])} to {Card.ToString(bottoms[j])}.)";
                    if (!cardsInStock.Contains(nextChains[1]))
                        return $"cycle (Looking for {Card.ToString(nextChains[0])}. Chaining {Card.ToString(tops[i])} to {Card.ToString(bottoms[j])}.)";

                    return $"cycle (Looking for {Card.ToString(nextChains[0])} or {Card.ToString(nextChains[1])}. Chaining {Card.ToString(tops[i])} to {Card.ToString(bottoms[j])})"; // cycle takes no params so we can add stuff
                }
            }

            return "NO";
        }

        // 10: Cycle through just so we can see all the stock
        protected string seeStockPile()
        {
            if (seenStock < 24)
                return "cycle (Tracking the pile)";
            return "NO";
        }

        // 11: Add whatever's currently atop the stock pile to the board
        protected string tryAddCurrentStock()
        {
            // Check if it even can be added
            if (canBePlacedOnBoard(stock))
            {
                // Place it on the first pile it can go on
                char[] tops = getPileTops();
                for (int i = 0; i < 7; i++)
                    if (isValidCombo(stock, tops[i]))
                        return "stb " + i;
            }

            return "NO";
        }

        // 12: Put anything on the board into a foundation pile (prioritizing smallest)
        protected string tryAddAnythingToFoundationsFromBoard()
        {
            char[] tops = getPileTops();
            int minValue = 13;

            for (int i = 0; i < 7; i++)
            {
                int suit = (int)Card.GetSuit(tops[i]);
                int value = Card.GetValue(tops[i]);

                // Check if it can be added to the pile
                if (value != Card.GetValue(foundationPiles[suit]) + 1)
                    continue;

                // Update minValue if so
                if (value < minValue) minValue = value;
            }

            for (int i = 0; i < 7; i++)
            {
                int suit = (int)Card.GetSuit(tops[i]);
                int value = Card.GetValue(tops[i]);

                if (value != Card.GetValue(foundationPiles[suit]) + 1)
                    continue;

                // Find the first card with the min value and add it
                if (value == minValue)
                    return "btf " + i;
            }

            return "NO";
        }

        // 13: Put anything from the stock into a foundation pile (prioritizing smallest)
        protected string tryAddAnythingToFoundationsFromStock()
        {
            int suit = (int)Card.GetSuit(stock);
            int value = Card.GetValue(stock);

            // Check if it can be added to the pile
            if (value == Card.GetValue(foundationPiles[suit]) + 1)
                return "stf";

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
            return (top != '\0' && bottom != '\0') &&
                (Card.IsBlack(top) != Card.IsBlack(bottom)) &&
                (Card.GetValue(top) + 1 == Card.GetValue(bottom));
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
        /// Checks if there exists a chain from the start card to the target card in the stock pile.
        /// 
        /// e.g. start card is 7H, end card is 4C. If either 7H or 7D, either 6S or 6C, and either 5H or 5D are in the stock pile, this returns true.
        /// If any are missing, it returns false.
        /// 
        /// Does not check the stock pile for the first card or last card.
        /// </summary>
        /// <param name="start">The start card of the chain(inclusive)</param>
        /// <param name="target">The end card of the chain (exclusive, assumed to be on board)</param>
        /// <returns>True if the entire chain is present in the stock pile.</returns>
        protected bool checkStockForChain(char start, char target)
        {
            if (start == target) return false;

            // 0 if the colors match, 1 if not
            int colorMatchBit = Card.IsBlack(start) == Card.IsBlack(target) ? 0 : 1;

            // i.e. a 7 of Spades - a 4 of Hearts = a difference of 3
            int difference = Card.GetValue(start) - Card.GetValue(target);

            if (difference < 1) return false;

            // If the difference mod 2 equals the color bit, they can be chained!
            // i.e. 7 of Spades - 4 of Hearts -> different colors (1), difference of 3, 3 mod 2 = 1
            //      7 of Spades - 5 of Clubs  -> same      colors (0), difference of 2, 2 mod 2 = 0
            // Otherwise, they can't be chained.
            if (difference % 2 != colorMatchBit) return false;

            // Iteratively go through and check if each part of this chain are in the stock.
            difference--;
            int targetValue = Card.GetValue(target);
            bool targetColor = !Card.IsBlack(start);

            // Note: We decrement here, which means that we don't check the stock pile for the first card (or its same-rank-and-color sibling.)
            // This is because we use this function in only two spots:
            // a) When checking if the current stock card can be chained, in which case we know we have it.
            // b) When checking if any chain exists between bottoms and tops, in which case the first card definitely wouldn't be in the stock pile.

            char[] possibilities;
            while (difference > 0)
            {
                // Get the cards that match
                possibilities = Card.FromColorAndValue(targetColor, targetValue + difference);

                // If neither is present, there isn't a chain.
                if (!ContainsAny(cardsInStock, possibilities)) return false;

                difference--;
                targetColor = !targetColor;
            }

            // If we didn't return, they can be chained.
            return true;
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