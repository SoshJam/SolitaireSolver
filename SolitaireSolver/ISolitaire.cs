namespace SolitaireSolver
{
    public interface ISolitaire
    {
        /// <summary>
        /// Whether or not the game is using Turn-3 rules:
        /// 
        /// Whenever the stock pile is cycled, (up to) 3 cards are drawn instead of 1. The only way to access cards X away
        /// from the top of the pile (where X mod 3 != 0) is to play extra cards ahead of them, which may lock future opportunities.
        /// </summary>
        public bool IsTurn3 { get; protected set; }

        /// <summary>
        /// Get a collection of cards currently on the main game board.
        /// </summary>
        /// <returns>An array of length 7 containing lists of cards in each column. Face down cards are represented with a pound sign (#).</returns>
        public List<char>[] GetBoard();

        /// <summary>
        /// The foundation piles are the main objective of Solitaire.
        /// </summary>
        /// <returns>The cards at the top of each foundation pile, ordered SHCD.</returns>
        public char[] GetFoundationPiles();

        /// <summary>
        /// Draws a card from the Stock pile and places it atop the Waste pile, first refreshing it if it is depleted.
        /// </summary>
        /// <returns>The card atop the waste pile.</returns>
        public char CycleStock();

        /// <summary>
        /// Peek at the card atop the waste pile.
        /// </summary>
        /// <returns>The card atop the waste pile, or \0 if that pile is empty.</returns>
        public char PeekStock();

        /// <summary>
        /// The three visible cards atop the waste pile, from top to bottom. Only the top card, element 0, is currently accessible.
        /// </summary>
        /// <returns>An array of 3 cards, from top to bottom, or \0 if any spot is empty.</returns>
        public char[] Peek3Stock();

        /// <summary>
        /// Attempts to move a pile on the board from one column to another.
        /// </summary>
        /// <param name="start">The column containing the pile.</param>
        /// <param name="end">The column to place this pile in.</param>
        /// <param name="revealed">A face-down card that may be revealed from the move. '\0' otherwise.</param>
        /// <param name="offset">How far from the bottom to start moving this pile (default 0).</param>
        /// <returns>True if the move is valid and successful.</returns>
        public bool MovePile(int start, int end, out char revealed, int offset = 0);

        /// <summary>
        /// Attempts to move the top card from the specified column to its associated foundation pile.
        /// </summary>
        /// <param name="column">The column number of the card to move.</param>
        /// <param name="revealed">A face-down card that may be revealed by the move.</param>
        /// <returns>True if the move was successful.</returns>
        public bool BoardToFoundations(int column, out char revealed);

        /// <summary>
        /// Attempts to move a card from a Foundation pile back onto the board.
        /// </summary>
        /// <param name="suit">The suit of the pile to remove from.</param>
        /// <param name="target">The column to attempt to move to.</param>
        /// <returns>True if the move was successful.</returns>
        public bool FoundationsToBoard(Suit suit, int target);

        /// <summary>
        /// Attempts to move a card from the stock pile to its associated foundation pile. If the waste pile would then be empty, draws a new card.
        /// </summary>
        /// <param name="revealed">The card that is now atop the stock pile.</param>
        /// <returns></returns>
        public bool StockToFoundations(out char revealed);

        /// <summary>
        /// Attempts to move a card from the waste pile to the target column on the board. If the waste pile would then be empty, draws a new card.
        /// </summary>
        /// <param name="target">The column to attempt to removed to.</param>
        /// <param name="revealed">The card that is now atop the waste pile.</param>
        /// <returns></returns>
        public bool StockToBoard(int target, out char revealed);

        /// <summary>
        /// Resets the game state.
        /// </summary>
        /// <param name="turn3">If the game should use Turn 3 rules.</param>
        public void Reset(bool turn3 = false);
    }
}
