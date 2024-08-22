namespace SolitaireSolver
{
    public interface ISolver
    {
        /// <summary>
        /// Calculates the next move in a game of Solitaire.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <returns>The next move, formatted as a string.</returns>
        public static string CalculateNextMove(ISolitaire game) => throw new NotImplementedException();
    }
}
