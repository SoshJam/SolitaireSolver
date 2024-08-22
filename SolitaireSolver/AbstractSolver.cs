namespace SolitaireSolver
{
    public abstract class AbstractSolver
    {
        // The game
        ISolitaire game;

        // A simple list of all cards
        protected HashSet<char> allCards = new HashSet<char>();

        // The cards currently face-up on the board
        protected List<char>[] board = new List<char>[7];

        // The highest cards in the foundation piles
        protected char[] foundationPiles = new char[4];

        // The card currently atop the waste pile (or \0 if empty)
        protected char stock = '\0';

        // The cards we know to be in the stock pile
        protected HashSet<char> cardsInStock = new HashSet<char>();

        // The cards we know to be either on the board or in a foundation pile
        protected HashSet<char> cardsInPlay = new HashSet<char>();

        // The cards we haven't seen yet
        protected HashSet<char> cardsMissing = new HashSet<char>();

        // If this is 24 we have seen every card in the stock pile
        protected int seenStock = 0;

        // If we don't know what move to do next and are just cycling through
        public SolverState state { get; protected set; } = SolverState.Normal;

        // The card in the stockpile when we were last stumped.
        // If we see this card again, we have completed a full cycle while stumped and should reset.
        protected char cycleStart = '#'; 

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
            state = SolverState.Normal;
            cycleStart = '#';
        }

        protected void Update()
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
                    if (c == '#') continue;

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

/// <summary>
/// The state of the solver, i.e. whether or not it knows what to do.
/// </summary>
public enum SolverState
{
    Normal, // everything is going as planned
    Stumped, // just cycling through, not knowing what to do
    GaveUp, // completed a full stock cycle while stumped, reset recommended
}
