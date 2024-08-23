# Solitaire Solver
By Sam Hill

---

I've been getting a whole lot of ads recently for two mobile games, Solitaire Cash and Solitaire Clash. Both of them offer real money for participating in Solitaire Tournaments. However, these tournaments cost real money (well, freemium currency) to enter, and the payouts are a fraction of how much actually goes in.

To make matters worse, both of these games are advertised not as something you should play for fun, but as a way to pay off debt, make rent, or even afford a house. That infuriates me, as it's manipulating people into spending time and money on the slight hope of financial security. However, while I was doomscrolling about these games, I came across some interviews that said that the high-paying tournaments were just filled with bots, and that made gameplay pretty discouraging.

So I thought, why not build my own bot? This project has 3 goals:

1. Take a bunch of money from these shitty companies.
2. Fill up tournament slots with super efficient bot players to discourage real victims from spending money on the game
3. Maybe supplement my income until I can find a real job, and then just provide funds to run more bots.

## Development Roadmap
```
[x] Plan out the basic project structure.
[x] Simulate a game of solitaire via the command line.
[x] Create an algorithm to find the optimal* move.
[ ] Figure out how to handle Turn 3 solitaire, which is MUCH harder to deal with.
[ ] Set up a system to read a screen and convert that into game information, and
    convert movecommands into mouse actions.
[ ] Set up this system for the free website Solitaired** and ensure it works well.
[ ] Add some randomness to the mouse movements, in case Cash or Clash have some
    way to account for that.
[ ] Set up the system for those games.
[ ] Set up something to automatically start new games as well in those apps.
```
\* Klondike Solitaire is not a solved game, so there isn't a known way to find the true best move. The goal is to simply outperform all but the very best human players.  
\*\* [https://solitaired.com/](Solitaired) is an actually good website that has got me through many a boring lecture.

## Statistics

Tested over 1,000,000 games, the current algorithm has the following stats:

### Turn 1 Klondike Solitaire

Win Rate: 42.27% (according to Solitaired, the average win rate is about 33.0%)  
Average Moves per game: ~118.72  
Average Moves per winning game: ~145.01  