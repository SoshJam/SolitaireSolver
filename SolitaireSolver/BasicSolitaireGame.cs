using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver
{
    public class BasicSolitaireGame : SolitaireHandler
    {
        // A simple list of all cards
        private HashSet<char> allCards = new HashSet<char>();

        // The cards that are still face-down
        private Stack<char> deck = new Stack<char>();

        // The cards currently face-up on the board
        private List<char>[] board = new List<char>[7];
        
        // The amount of face-down cards in each column
        private int[] faceDownBoardCards = new int[7];

        // The cards in the stock pile, defaulting to 24 cards
        private Stack<char> stockPile = new Stack<char>();
        
        // The cards in the waste pile, defaulting to 0 cards.
        private Stack<char> wastePile = new Stack<char>();

        // The highest cards in the foundation piles
        private char[] foundationPiles = new char[4];

        // Set up variables
        public BasicSolitaireGame()
        {
            // Add all the cards to the array of all cards
            allCards.Clear();
            for (char x = 'A'; x <= 'Z'; x++)
            {
                allCards.Add(x);
                allCards.Add((char)(x + 32)); // Add the lower-case version as well
            }

            // Start a game
            Reset();
        }

        public bool BoardToFoundations(int column, out char revealed)
        {
            // If the target column is empty, this is invalid.
            if (board[column].Count == 0)
            {
                revealed = '\0';
                return false;
            }

            char card = board[column].Last();
            int suit = (int) Card.GetSuit(card);

            // The card can be placed there if:
            // a) It is 1 greater than the card of the same suit, OR
            // b) It is an Ace (value 1) and the pile of that suit is empty.
            if (card - foundationPiles[suit] != 1 && !(foundationPiles[suit] == '\0' && Card.GetValue(card) == 1))
            {
                revealed = '\0';
                return false;
            }

            // Update the pile
            foundationPiles[suit] = card;
            board[column].RemoveAt(board[column].Count - 1);

            // Reveal the top card of this pile if necessary
            revealed = '\0';
            if (board[column].Count == 0)
            {
                revealed = deck.Pop();
                board[column].Add(revealed);
            }

            return true;
        }

        public char CycleStock()
        {
            // Check to see if the stock pile is empty
            if (stockPile.Count == 0)
            {
                // If the waste pile is also empty at the start, there's just no cards left in stock and we should just return \0
                if (wastePile.Count == 0)
                    return '\0';

                // Replenish by stacking from the Waste Pile
                while (wastePile.Count > 0)
                {
                    stockPile.Push(wastePile.Pop());
                }
            }

            // Move a card from the stock pile to the waste pile
            char drawnCard = stockPile.Pop();
            wastePile.Push(drawnCard);

            return drawnCard;
        }

        public bool FoundationsToBoard(Suit suit, int target)
        {
            char card = foundationPiles[(int) suit];

            List<char> targetColumn = board[target];

            // If the target column is empty, the top card must be a king to move it there
            if (targetColumn.Count == 0)
            {
                if (Card.GetValue(card) != 13)
                    return false;

                // Otherwise the move is valid
                targetColumn.Add(card);

                // Decrement the foundation pile by 1, or set it to empty if the moved card is an Ace
                if (Card.GetValue(card) > 1)
                    foundationPiles[(int)suit]--;
                else
                    foundationPiles[(int)suit] = '\0';

                return true;
            }

            // Get the top card of this column
            char targetCard = targetColumn.Last();

            // The move is valid if they are of different colors and topCard's value is one less than that of targetCard
            if (Card.IsBlack(targetCard) != Card.IsBlack(card) && Card.GetValue(targetCard) - 1 == Card.GetValue(card))
            {
                targetColumn.Add(card);

                // Decrement the foundation pile by 1, or set it to empty if the moved card is an Ace
                if (Card.GetValue(card) > 1)
                    foundationPiles[(int)suit]--;
                else
                    foundationPiles[(int)suit] = '\0';

                return true;
            }

            // Otherwise, it's invalid.
            return false;
        }

        public List<char>[] GetBoard()
        {
            List<char>[] outputBoard = new List<char>[7];
            for (int i = 0; i < 7; i++)
            {
                // First add the amount of face-down cards
                for (int f = 0; f < faceDownBoardCards[i]; f++)
                    outputBoard[i].Add('#');

                // Then just add the board
                foreach (char c in board[i])
                    outputBoard[i].Add(c);
            }
            return outputBoard;
        }

        public char[] GetFoundationPiles()
        {
            return foundationPiles;
        }

        public bool MovePile(int start, int end, out char revealed, int offset = 0)
        {
            // Ensure there are enough cards
            if (board[start].Count <= offset)
            {
                revealed = '\0';
                return false;
            }

            char baseCard = board[start][offset];
            List<char> movedCards = new List<char>();
            for (int i = offset; i < board[start].Count; i++)
            {
                movedCards.Add(i);
            }

            // If the target column is empty, the base card must be a king to move it there
            if (board[end].Count == 0)
            {
                if (Card.GetValue(baseCard) != 13)
                {
                    revealed = '\0';
                    return false;
                }

                // Otherwise the move is valid

                // Move cards one at a time
                foreach (char c in movedCards)
                {
                    board[start].Add(c);
                    board[start].Remove(c);
                }

                // Reveal a card if necessary
                revealed = '\0';
                if (offset == 0 || board[start].Count == 0) // Second half of this should be redundant, but just in case
                {
                    revealed = deck.Pop();
                    board[start].Add(revealed);
                }

                return true;
            }

            // Get the top card of this column
            char targetCard = board[end].Last();

            // The move is valid if they are of different colors and topCard's value is one less than that of targetCard
            if (Card.IsBlack(targetCard) != Card.IsBlack(baseCard) && Card.GetValue(targetCard) - 1 == Card.GetValue(baseCard))
            {
                // Move cards one at a time
                foreach (char c in movedCards)
                {
                    board[start].Add(c);
                    board[start].Remove(c);
                }

                // Reveal a card if necessary
                revealed = '\0';
                if (offset == 0 || board[start].Count == 0) // Second half of this should be redundant, but just in case
                {
                    revealed = deck.Pop();
                    board[start].Add(revealed);
                }

                return true;
            }

            // Otherwise, it's invalid.
            revealed = '\0';
            return false;
        }

        public char PeekStock()
        {
            if (wastePile.Count == 0)
                return '\0';
            return wastePile.Peek();
        }

        public void Reset()
        {
            // Reset the deck
            deck.Clear();
            List<char> shuffledCards =allCards.ToList();
            Shuffle<char>(shuffledCards);
            foreach (char c in shuffledCards)
                deck.Push(c);

            Stack<char> tempDeck = new Stack<char>();

            // Draw 7 cards for the board
            for (int i = 0; i < 7; i++)
            {
                board[i].Clear(); // Clear this column
                board[i].Add(deck.Pop()); // Add one card
                faceDownBoardCards[i] = i; // Reset the amount of face-down cards

                // Add face-down cards to this to ensure cards are drawn in the correct order
                for (int j = 0; j < i; j++)
                {
                    tempDeck.Push(deck.Pop());
                }
            }

            // Draw 24 cards for the stock
            wastePile.Clear();
            stockPile.Clear();
            for (int i = 0; i < 24; i++)
                stockPile.Push(deck.Pop());

            // There should be 21 cards left in the deck representing the face-down cards on the board.
            while (tempDeck.Count > 0)
                deck.Push(tempDeck.Pop());

            // Reset the foundation piles.
            for (int i = 0; i < 3; i++)
                foundationPiles[i] = '\0';
        }

        public bool StockToBoard(int target, out char revealed)
        {
            // Check the suit of the card atop the waste pile
            char topCard = PeekStock();

            // If it's a null character the waste pile is empty.
            if (topCard == '\0')
            {
                revealed = '\0';
                return false;
            }

            List<char> targetColumn = board[target];

            // If the target column is empty, the top card must be a king to move it there
            if (targetColumn.Count == 0)
            {
                if (Card.GetValue(topCard) != 13)
                {
                    revealed = '\0';
                    return false;
                }

                // Otherwise the move is valid
                targetColumn.Add(topCard);
                revealed = PeekStock();
                return true;
            }

            // Get the top card of this column
            char targetCard = targetColumn.Last();

            // The move is valid if they are of different colors and topCard's value is one less than that of targetCard
            if (Card.IsBlack(targetCard) != Card.IsBlack(topCard) && Card.GetValue(targetCard) - 1 == Card.GetValue(topCard))
            {
                targetColumn.Add(topCard);
                revealed = PeekStock();
                return true;
            }

            // Otherwise, it's invalid.
            revealed = '\0';
            return false;
        }

        public bool StockToFoundations(out char revealed)
        {
            // Check the suit of the card atop the waste pile
            char topCard = PeekStock();

            // If it's a null character the waste pile is empty.
            if (topCard == '\0')
            {
                revealed = '\0';
                return false;
            }

            int suit = (int) Card.GetSuit(topCard);

            // The card can be placed there if:
            // a) It is 1 greater than the card of the same suit, OR
            // b) It is an Ace (value 1) and the pile of that suit is empty.
            if (topCard - foundationPiles[suit] != 1 && !(foundationPiles[suit] == '\0' && Card.GetValue(topCard) == 1))
            {
                revealed = '\0';
                return false;
            }

            // Update the pile
            foundationPiles[suit] = topCard;
            wastePile.Pop();

            // Reveal the next card in the waste pile
            revealed = PeekStock(); // returns \0 if empty, so we don't have to do any extra logic
            return true;
        }

        // Helper methods

        // List Shuffler
        // From https://stackoverflow.com/questions/273313/randomize-a-listt
        private static Random rng = new Random();
        private static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
