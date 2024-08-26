namespace SolitaireSolver
{
    public abstract class AbstractSolver
    {
        /// <summary>
        /// The game this solver is currently dealing with.
        /// </summary>
        ISolitaire game;

        /// <summary>
        /// A list of all cards. Used in setup.
        /// </summary>
        protected HashSet<char> allCards = new HashSet<char>();

        /// <summary>
        /// The cards currently face-up on the board 
        /// </summary>
        protected List<char>[] board = new List<char>[7];

        /// <summary>
        /// The highest cards in the foundation piles.
        /// 0 = Spades, 1 = Hearts, 2 = Clubs, 3 = Diamonds
        /// </summary>
        protected char[] foundationPiles = new char[4];

        /// <summary>
        /// The card currently atop the waste pile (or \0 if empty)
        /// </summary>
        protected char stock = '\0';

        /// <summary>
        /// The cards we know to be in the stock pile, in the order that we saw them.
        /// </summary>
        protected List<char> cardsInStock = new List<char>();

        /// <summary> 
        /// The cards we know to be either on the board or in a foundation pile
        /// </summary>
        protected HashSet<char> cardsInPlay = new HashSet<char>();

        /// <summary>
        /// The cards we haven't seen yet
        /// </summary>
        protected HashSet<char> cardsMissing = new HashSet<char>();

        /// <summary>
        /// If this is 24, we have seen every card in the stock pile
        /// </summary>
        protected int seenStock = 0;

        /// <summary>
        /// If we don't know what move to do next and are just cycling through
        /// </summary>
        public SolverState state { get; protected set; } = SolverState.Normal;

        /// <summary>
        /// The card in the stockpile when we were last stumped.
        /// If we see this card again, we have completed a full cycle while stumped and should reset.
        /// </summary>
        protected char cycleStart = '#';

        /// <summary>
        /// If the game uses Turn 3 rules
        /// </summary>
        protected bool isTurn3 = false;

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
        public virtual void Reset(ISolitaire newGame)
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
            isTurn3 = game.IsTurn3;
        }

        /// <summary>
        /// Updates the game state by reading from the current game.
        /// </summary>
        protected virtual void Update()
        {
            // Easy updates
            board = game.GetBoard();
            foundationPiles = game.GetFoundationPiles();
            stock = game.PeekStock();

            // Track the stock pile
            if (seenStock < 24 && cardsMissing.Contains(stock))
            {
                // For Turn 1 mode
                if (!isTurn3)
                {
                    if (!cardsInStock.Contains(stock))
                        cardsInStock.Add(stock);
                    seenStock++;
                }

                // For Turn 3 mode
                else
                {
                    char[] top3 = game.Peek3Stock();

                    // We have to make sure we add it from the bottom up
                    // to make sure they are put in the list in the correct order
                    for (int i = 2; i >= 0; i--)
                    {
                        if (top3[i] == '\0') continue; // Ignore if this card is empty.

                        if (!cardsInStock.Contains(top3[i]))
                            cardsInStock.Add(top3[i]);
                        seenStock++;
                    }
                }
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
