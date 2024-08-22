namespace SolitaireSolver
{
    /// <summary>
    /// There are 52 cards in a deck, which is equivalent to the number of letters in two alphabets.
    /// Because of this, we can represent a card as a single character.
    /// A-M are spades, N-Z are clubs, a-m are hearts, and n-z are diamonds.
    /// (Normally it goes Spades - Hearts - Clubs - Diamonds, but this order makes certain operations easier.)
    /// This static class provides methods for interpreting characters as cards.
    /// </summary>
    public static class Card
    {
        /// <summary>
        /// Returns the Suit of the card.
        /// </summary>
        /// <param name="input">The character representing a card.</param>
        /// <returns>The suit of the card.</returns>
        public static Suit GetSuit(char input)
        {
            if (input >= 'n')
                return Suit.Diamonds;
            if (input >= 'a')
                return Suit.Hearts;
            if (input >= 'N')
                return Suit.Clubs;
            return Suit.Spades;
        }

        /// <summary>
        /// Returns the value of the card as a number from 1 to 13.
        /// </summary>
        /// <param name="input">The character representing the card.</param>
        /// <returns>The value of the card.</returns>
        public static int GetValue(char input)
        {
            if (input == '\0')
                return 0;
            if (input >= 'n')
                return input - 'n' + 1;
            if (input >= 'a')
                return input - 'a' + 1;
            if (input >= 'N')
                return input - 'N' + 1;
            return input - 'A' + 1;
        }

        /// <summary>
        /// Creates a char representing a card from a suit and value.
        /// </summary>
        /// <param name="suit">The suit of a card.</param>
        /// <param name="value">The value of a card.</param>
        /// <returns>The char representing this char.</returns>
        /// <exception cref="ArgumentException">If the value is outisde the valid range.</exception>
        public static char FromSuitAndValue(Suit suit, int value)
        {
            // Make sure value is between 1 and 13
            if (value < 1 || value > 13)
                throw new ArgumentException("Value must be between 1 and 13.");

            // Find the char to start the suit
            char suitStart = '\0';
            switch (suit)
            {
                case Suit.Spades:
                default:
                    suitStart = 'A';
                    break;
                case Suit.Clubs:
                    suitStart = 'N';
                    break;
                case Suit.Hearts:
                    suitStart = 'a';
                    break;
                case Suit.Diamonds:
                    suitStart = 'n';
                    break;
            }

            return (char)(suitStart + value - 1);
        }

        /// <summary>
        /// Converts an input string into a char representing a card.
        /// </summary>
        /// <param name="input">A two-character string representing a card.</param>
        /// <returns>The char that represents that card.</returns>
        /// <exception cref="ArgumentException">If the string is not formatted properly.</exception>
        public static char FromString(string input)
        {
            // Throw an error if the length is not 2.
            if (input.Length != 2)
                throw new ArgumentException("Input string must have length 2.");

            char rankChar = input[0];
            char suitChar = input[1];

            // Throw an error if the string is not correct
            char[] validRankChars = "A23456789TJQK".ToCharArray();
            char[] validSuitChars = "♠♥♣♦SHCD".ToCharArray();

            if (!validRankChars.Contains(rankChar) || !validSuitChars.Contains(suitChar))
                throw new ArgumentException("Input string was not formatted correctly.");

            // Find the char to start the suit
            char suitStart = '\0';
            switch (suitChar)
            {
                case '♠':
                case 'S':
                default:
                    suitStart = 'A';
                    break;
                case '♣':
                case 'C':
                    suitStart = 'N';
                    break;
                case '♥':
                case 'H':
                    suitStart = 'a';
                    break;
                case '♦':
                case 'D':
                    suitStart = 'n';
                    break;
            }

            // Find the value of the card
            int value = Array.IndexOf(validRankChars, rankChar); // 0 indexed so we don't need to subtract 1

            // Get the actual char
            char card = (char)(suitStart + value);

            return card;
        }

        /// <summary>
        /// Converts the card to a string.
        /// </summary>
        /// <param name="input">The card to check.</param>
        /// <returns>The name of the card.</returns>
        /// <exception cref="ArgumentException">If the char is not a letter</exception>
        public static string ToString(char input)
        {
            // Make sure the card is in the valid range
            if (!(input >= 'A' && input <= 'Z') && !(input >= 'a' && input <= 'z') && input != '\0')
                throw new ArgumentException("Input char must be a letter");

            // Return empty spaces if necessary
            if (input == '\0')
                return "  ";

            // Get the suit and value of the card
            Suit suit = GetSuit(input);
            int value = GetValue(input);

            // Convert value into rank
            string card = "";
            switch (value)
            {
                case 1:
                    card = "A";
                    break;
                case 10:
                    card = "T"; // must be T because 10 is two characters
                    break;
                case 11:
                    card = "J";
                    break;
                case 12:
                    card = "Q";
                    break;
                case 13:
                    card = "K";
                    break;
                default:
                    card = value.ToString();
                    break;
            }

            // Add the suit
            card += "SHCD"[(int)suit];

            return card;
        }

        /// <summary>
        /// Returns true if the card is black.
        /// </summary>
        /// <param name="input">The card to check.</param>
        /// <returns>True if the card is black.</returns>
        public static bool IsBlack(char input)
        {
            return input <= 'Z';
        }
    }

    public enum Suit
    {
        Spades,
        Hearts,
        Clubs,
        Diamonds
    }
}