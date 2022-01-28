namespace MTG_ConsoleEngine
{
    public class Player
    {
        public int ID { get;  internal set; } 
        public int Health { get; internal set; } = 20;
        public Dictionary<string,int> ManaPool {get; internal set; } = new();

        public List<CardBase> Field {get; private set; } = new();
        public List<CardBase> Hand {get; private set; } = new();
        public List<CardBase> Deck { get; private set; } = new();
        public List<CardBase> Graveyard { get; private set; } = new();
        public List<CardBase> Exiled { get; private set; } = new();

        public Player( int _id, int _health)
        {
            this.ID = _id;
            this.Health = _health;
        }

        public void DealDamage(int value)
        {
            Console.Write($"Player {ID} otrzymał {value} obrażeń.");
            Health -= value;
            Console.WriteLine($"Aktualne HP: {Health}");
        }

        internal void AddToDeck(CardBase _card)
        {
            _card.owner = this;
            Deck.Add(_card);
        }
    }
}