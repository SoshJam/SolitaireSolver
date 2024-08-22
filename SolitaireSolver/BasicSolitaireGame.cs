using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver
{
    public class BasicSolitaireGame : SolitaireHandler
    {
        public bool BoardToFoundations(int column, out char revealed)
        {
            throw new NotImplementedException();
        }

        public char CycleStock()
        {
            throw new NotImplementedException();
        }

        public bool FoundationsToBoard(Suit suit, int target)
        {
            throw new NotImplementedException();
        }

        public List<char>[] GetBoard()
        {
            throw new NotImplementedException();
        }

        public char[] GetFoundationPiles()
        {
            throw new NotImplementedException();
        }

        public bool MovePile(int start, int end, out char revealed, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public char PeekStock()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool StockToBoard(int target, out char revealed)
        {
            throw new NotImplementedException();
        }

        public bool StockToFoundations(out char revealed)
        {
            throw new NotImplementedException();
        }
    }
}
