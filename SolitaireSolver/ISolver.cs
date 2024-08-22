namespace SolitaireSolver
{
    public interface ISolver
    {
        /// <summary>
        /// Calculates the next move in a game of Solitaire.
        /// </summary>
        /// <returns>The next move, formatted as a string.</returns>
        public string CalculateNextMove();

        /// <summary>
        /// Resets the game state with the given game.
        /// </summary>
        /// <param name="game">The game to transfer the state from.</param>
        public void Reset(ISolitaire game);
    }
}
