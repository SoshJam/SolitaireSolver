namespace SolitaireSolver
{
    public abstract class AbstractSolver
    {
        // The game
        ISolitaire game;

        // A simple list of all cards
        private HashSet<char> allCards = new HashSet<char>();

        // The cards currently face-up on the board
        private List<char>[] board = new List<char>[7];

        // The highest cards in the foundation piles
        private char[] foundationPiles = new char[4];

        // The card currently atop the waste pile (or \0 if empty)
        private char stock = '\0';

        // The cards we know to be in the stock pile
        private HashSet<char> cardsInStock = new HashSet<char>();

        // The cards we know to be either on the board or in a foundation pile
        private HashSet<char> cardsInPlay = new HashSet<char>();

        // The cards we haven't seen yet
        private HashSet<char> cardsMissing = new HashSet<char>();

        // If this is 24 we have seen every card in the stock pile
        private int seenStock = 0;

        // Set up variables
        public AbstractSolver(ISolitaire newGame)
        {
            // Add all the cards to the array of all cards
            allCards.Clear();
            for (char x = 'A'; x <= 'Z'; x++)
            {
                allCards.Add(x);
                allCards.Add((char)(x + 32)); // Add the lower-case version as well
            }

            game = newGame;
            Reset(game);
        }
        /// <summary>
        /// Resets the game state with the given new game.
        /// </summary>
        /// <param name="game">The game to transfer the state from.</param>
        public void Reset(ISolitaire newGame)
        {
            game = newGame;

            // Get everything in
            board = game.GetBoard();
            foundationPiles = game.GetFoundationPiles();
            stock = game.PeekStock();

            // Reset other variables
            cardsInStock.Clear();
            cardsInPlay.Clear();
            cardsMissing = new HashSet<char>(allCards);
            seenStock = 0;
        }

        private void Update()
        {
            // Easy updates
            board = game.GetBoard();
            foundationPiles = game.GetFoundationPiles();
            stock = game.PeekStock();

            // Track the stock pile
            if (seenStock < 24 && cardsMissing.Contains(stock))
            {
                cardsInStock.Add(stock);
                seenStock++;
            }

            // Check the board for any missing cards or cards taken from stock
            for (int i = 0; i < 7; i++)
            { 
                foreach (char c in board[i])
                {
                    if (c != '#') continue;

                    if (cardsMissing.Contains(c))
                    {
                        cardsInPlay.Add(c);
                        cardsMissing.Remove(c);
                    }

                    if (cardsInStock.Contains(c))
                    {
                        cardsInPlay.Add(c);
                        cardsInStock.Remove(c);
                    }
                }
            }

            // Check the tops of foundation piles for missing/stock cards
            for (int i = 0; i < 4; i++)
            {
                if (cardsMissing.Contains(foundationPiles[i]))
                {
                    cardsInPlay.Add(foundationPiles[i]);
                    cardsMissing.Remove(foundationPiles[i]);
                }

                if (cardsInStock.Contains(foundationPiles[i]))
                {
                    cardsInPlay.Add(foundationPiles[i]);
                    cardsInStock.Remove(foundationPiles[i]);
                }
            }
        }

        /// <summary>
        /// Calculates the next move in a game of Solitaire.
        /// </summary>
        /// <returns>The next move, formatted as a string.</returns>
        public abstract string CalculateNextMove();
    }
}
