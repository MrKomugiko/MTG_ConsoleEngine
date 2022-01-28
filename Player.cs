using MTG_ConsoleEngine.Card_Category;

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
        public readonly ConsoleColor color;
        
        public Player(){}
        public Player( int _id, int _health) 
        {
            this.ID = _id;
            this.Health = _health;
        
            if(_id == 1) color = ConsoleColor.Blue;
            else color= ConsoleColor.Green;
        }
        public void DealDamage(int value)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.Write($"Player {ID} otrzymał {value} obrażeń.");
            Health -= value;
            Console.WriteLine($"Aktualne HP: {Health}");
            Console.ResetColor();
        }
        internal void AddToDeck(CardBase _card)
        {
            _card.Owner = this;
            Deck.Add(_card);
        }
        public void DrawCard()
        {
            Console.ForegroundColor = color;
            var newCard = Deck.Last();
            Deck.Remove(newCard);
            Hand.Add(newCard);
            Console.WriteLine($"Player {ID} dobrał karte: {newCard.Name}");
            Console.ResetColor();
        }
        internal List<Creature> SelectAttackers()
        {
            Console.ForegroundColor = color;
            List<Creature> _attackersToDeclare = new();
            Console.WriteLine("Wybierz atakujących z dostępnych kreatur na polu: [ indexy oddzielaj przecinkiem: 0,1,3... ]");
            DisplayPlayerField();

            string input = Console.ReadLine()??"";
            Console.ResetColor();
            if(String.IsNullOrEmpty(input)) return new();

            List<string> attackers = input.Trim().Split(",").ToList();
            attackers.ForEach(index=>_attackersToDeclare.Add((Creature)Field[Int32.Parse(index)]));
            return _attackersToDeclare;
        }
        public Dictionary<Creature,Creature> SelectDeffenders()
        {
            Console.ForegroundColor = color;
            Dictionary<Creature, Creature> _deffendersToDeclare = new();
            Console.WriteLine("Nadchodzący Atak:");
            foreach (Creature creature in Engine.DeclaredAttackers)
            {
                Console.WriteLine($"[{Engine.DeclaredAttackers.IndexOf(creature)}] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth})");
            }

            Console.WriteLine("Wybierz obronę ataku / (enter zeby pominąć)\n[ najpierw podaj index przeciwnika potem kolejno blokujacych np. 1 - 1,2,4 ]");
            DisplayPlayerField();

            while (true)
            {
                string input = Console.ReadLine() ?? "".Trim();
                if (String.IsNullOrEmpty(input)) break;

                int input_atk = Int32.Parse(input.Split("-")[0]);                                       // 1
                IEnumerable<int> input_def = input.Split("-")[1].Split(",").Select(x => Int32.Parse(x));  // [1,2,3,4]

                Creature attacker = Engine.DeclaredAttackers[input_atk];
                foreach (var deffender in input_def)
                {
                    _deffendersToDeclare.Add((Creature)Field[deffender], attacker);
                }
            }
            Console.ResetColor();

            return _deffendersToDeclare;
        }
        public void PlayCardFromHand(CardBase card)
        {
            Field.Add(card);
            Hand.Remove(card);
            Console.ForegroundColor = color;
            Console.WriteLine($"Gracz 1 zagrywa kartą {card.Name}");
            Console.ResetColor();
        }
        public void DisplayPlayerField()
        {
            foreach (Creature creature in Field.Where(c => ((Creature)c).isTapped == false))
            {
                Console.WriteLine($"[{Field.IndexOf(creature)}] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth}) <Mana:{creature.getmanastring}>");
            }
            foreach (Creature creature in Field.Where(c => ((Creature)c).isTapped))
            {
                Console.WriteLine($"[ TAPPED ] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth}) <Mana:{creature.getmanastring}>");
            }
        }
    }
}