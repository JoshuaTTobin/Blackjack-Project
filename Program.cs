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
        // Public properties to access private fields
        public Suit Suit { get; private set; }
        public Value Value { get; private set; }

        // Constructor for the Card class
        public Card(Suit suit, Value value)
        {
            Suit = suit;
            Value = value;
        }

        // Override the ToString method to display the card details
        public override string ToString()
        {
            return $"{Value} of {Suit}";
        }
    }

    // Class to represent a deck of cards
    public class Deck
    {
        private List<Card> cards;  // Collection to hold the deck of cards

        // Constructor to initialize the deck with all possible cards
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

        // Method to shuffle the deck using the Fisher-Yates algorithm
        public void Shuffle()
        {
            Random random = new Random();  // Random number generator
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Card temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }

        // Method to draw a card from the deck
        public Card DrawCard()
        {
            Card drawnCard = cards[0];
            cards.RemoveAt(0);
            return drawnCard;
        }
    }

    // Interface to enforce common behavior for players
    public interface IPlayer
    {
        void DrawCard(Deck deck);
        int CalculateScore();
        void ShowHand();
        bool HasBlackjack();
    }

    // Class to represent a player in the game
    public class Player : IPlayer
    {
        public List<Card> Hand { get; private set; }  // Collection to hold the player's hand
        public int ChipCount { get; private set; }  // Property to track the player's chips

        // Constructor for the Player class with an initial chip count
        public Player(int initialChips)
        {
            Hand = new List<Card>();
            ChipCount = initialChips;
        }

        // Method to draw a card from the deck and add it to the player's hand
        public void DrawCard(Deck deck)
        {
            Hand.Add(deck.DrawCard());
        }

        // Method to calculate the player's score based on the hand
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

            // Adjust for aces if necessary (count Ace as 1 instead of 11 if score > 21)
            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        // Method to display the player's hand
        public void ShowHand()
        {
            foreach (Card card in Hand)
            {
                Console.WriteLine(card);
            }
            Console.WriteLine("Score: " + CalculateScore());
        }

        // Method to check if the player has a Blackjack (score of 21)
        public bool HasBlackjack()
        {
            return CalculateScore() == 21;
        }

        // Method to place a bet and update the chip count
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

        // Method to update the chip count when the player wins a bet
        public void WinBet(int betAmount)
        {
            ChipCount += betAmount * 2;
        }

        // Method to handle the case when the player loses a bet
        public void LoseBet()
        {
            // Bet amount is already subtracted when placing a bet
        }
    }

    // Class to represent the dealer, inheriting from Player
    public class Dealer : Player
    {
        // Constructor for the Dealer class, setting the chip count to 0
        public Dealer() : base(0) // Dealer doesn't use chips
        {
        }

        // Method to show only the dealer's first card
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
            const int initialChips = 100;  // Initial chips for the player
            Deck deck = new Deck();  // Create a new deck of cards
            Player player = new Player(initialChips);  // Create a new player with initial chips
            Dealer dealer = new Dealer();  // Create a new dealer

            // Game loop: Continue playing rounds until the player runs out of chips
            while (player.ChipCount > 0)
            {
                Console.WriteLine($"\nYou have {player.ChipCount} chips.");
                Console.Write("Enter your bet amount: ");
                int betAmount;

                // Validate the bet amount
                while (!int.TryParse(Console.ReadLine(), out betAmount) || betAmount <= 0 || !player.PlaceBet(betAmount))
                {
                    Console.WriteLine("Invalid bet. Please enter a valid bet amount.");
                }

                // Draw initial cards for the player and dealer
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

                // Player's turn: Hit or Stand
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

                // Dealer's turn: Continue drawing until the score is 17 or higher
                if (player.CalculateScore() <= 21)
                {
                    Console.WriteLine("\nDealer's Hand:");
                    dealer.ShowHand();

                    while (dealer.CalculateScore() < 17)
                    {
                        dealer.DrawCard(deck);
                        Console.WriteLine("\nDealer draws a card.");
                        dealer.ShowHand();
                    }

                    // Determine the winner and update chip count
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

                // Clear hands for the next round
                player.Hand.Clear();
                dealer.Hand.Clear();
            }

            Console.WriteLine("You have run out of chips. Game over.");
        }

        // Static method to save game results to a file
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
