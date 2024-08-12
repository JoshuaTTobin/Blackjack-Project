using System;
using System.Collections.Generic;
using System.IO;

namespace BlackjackGame
{
    // Enum to represent the card suits
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    // Enum to represent the card values
    public enum Value
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack = 10, Queen = 10, King = 10, Ace = 11
    }

    // Class to represent a playing card
    public class Card
    {
        public Suit Suit { get; private set; }
        public Value Value { get; private set; }

        public Card(Suit suit, Value value)
        {
            Suit = suit;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value} of {Suit}";
        }
    }

    // Class to represent a deck of cards
    public class Deck
    {
        private List<Card> cards;

        public Deck()
        {
            cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Value value in Enum.GetValues(typeof(Value)))
                {
                    cards.Add(new Card(suit, value));
                }
            }
            Shuffle();
        }

        public void Shuffle()
        {
            Random random = new Random();
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Card temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }

        public Card DrawCard()
        {
            Card drawnCard = cards[0];
            cards.RemoveAt(0);
            return drawnCard;
        }
    }

    // Interface for the player
    public interface IPlayer
    {
        void DrawCard(Deck deck);
        int CalculateScore();
        void ShowHand();
        bool HasBlackjack();
    }

    // Class to represent a player
    public class Player : IPlayer
    {
        public List<Card> Hand { get; private set; }
        public int ChipCount { get; private set; }

        public Player(int initialChips)
        {
            Hand = new List<Card>();
            ChipCount = initialChips;
        }

        public void DrawCard(Deck deck)
        {
            Hand.Add(deck.DrawCard());
        }

        public int CalculateScore()
        {
            int score = 0;
            int aceCount = 0;

            foreach (Card card in Hand)
            {
                score += (int)card.Value;
                if (card.Value == Value.Ace)
                {
                    aceCount++;
                }
            }

            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        public void ShowHand()
        {
            foreach (Card card in Hand)
            {
                Console.WriteLine(card);
            }
            Console.WriteLine("Score: " + CalculateScore());
        }

        public bool HasBlackjack()
        {
            return CalculateScore() == 21;
        }

        public bool PlaceBet(int betAmount)
        {
            if (betAmount > ChipCount)
            {
                Console.WriteLine("You don't have enough chips to place that bet.");
                return false;
            }

            ChipCount -= betAmount;
            return true;
        }

        public void WinBet(int betAmount)
        {
            ChipCount += betAmount * 2;
        }

        public void LoseBet()
        {
            // Bet amount is already subtracted when placing a bet
        }
    }

    // Class to represent the dealer (inherits from Player)
    public class Dealer : Player
    {
        public Dealer() : base(0) // Dealer doesn't use chips
        {
        }

        public void ShowFirstCard()
        {
            Console.WriteLine(Hand[0]);
        }
    }

    // Main class to run the game
    public class Program
    {
        static void Main(string[] args)
        {
            const int initialChips = 100;
            Deck deck = new Deck();
            Player player = new Player(initialChips);
            Dealer dealer = new Dealer();

            while (player.ChipCount > 0)
            {
                Console.WriteLine($"\nYou have {player.ChipCount} chips.");
                Console.Write("Enter your bet amount: ");
                int betAmount;

                while (!int.TryParse(Console.ReadLine(), out betAmount) || betAmount <= 0 || !player.PlaceBet(betAmount))
                {
                    Console.WriteLine("Invalid bet. Please enter a valid bet amount.");
                }

                // Draw initial cards
                player.DrawCard(deck);
                player.DrawCard(deck);
                dealer.DrawCard(deck);
                dealer.DrawCard(deck);

                // Show player's hand
                Console.WriteLine("Player's Hand:");
                player.ShowHand();

                // Show dealer's first card
                Console.WriteLine("\nDealer's Hand:");
                dealer.ShowFirstCard();

                // Player's turn
                while (true)
                {
                    Console.WriteLine("Do you want to (h)it or (s)tand?");
                    string choice = Console.ReadLine().ToLower();

                    if (choice == "h")
                    {
                        player.DrawCard(deck);
                        Console.WriteLine("\nPlayer's Hand:");
                        player.ShowHand();

                        if (player.CalculateScore() > 21)
                        {
                            Console.WriteLine("Player busts! Dealer wins.");
                            player.LoseBet();
                            break;
                        }
                    }
                    else if (choice == "s")
                    {
                        break;
                    }
                }

                if (player.CalculateScore() <= 21)
                {
                    // Dealer's turn
                    Console.WriteLine("\nDealer's Hand:");
                    dealer.ShowHand();

                    while (dealer.CalculateScore() < 17)
                    {
                        dealer.DrawCard(deck);
                        Console.WriteLine("\nDealer draws a card.");
                        dealer.ShowHand();
                    }

                    if (dealer.CalculateScore() > 21 || player.CalculateScore() > dealer.CalculateScore())
                    {
                        Console.WriteLine("Player wins.");
                        player.WinBet(betAmount);
                    }
                    else if (dealer.CalculateScore() >= player.CalculateScore())
                    {
                        Console.WriteLine("Dealer wins.");
                        player.LoseBet();
                    }
                }

                // Save game result to file
                SaveResult(player.CalculateScore(), dealer.CalculateScore(), player.ChipCount);

                // Clear hands for next round
                player.Hand.Clear();
                dealer.Hand.Clear();
            }

            Console.WriteLine("You have run out of chips. Game over.");
        }

        static void SaveResult(int playerScore, int dealerScore, int playerChips)
        {
            using (StreamWriter writer = new StreamWriter("game_results.txt", true))
            {
                writer.WriteLine($"Player Score: {playerScore}, Dealer Score: {dealerScore}, Player Chips: {playerChips}");
                writer.WriteLine(playerScore > dealerScore ? "Player wins." : "Dealer wins.");
                writer.WriteLine();
            }
        }
    }
}
