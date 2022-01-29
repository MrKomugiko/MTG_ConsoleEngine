using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Player
    {
        public int ID { get;  internal set; } 
        public int Health { get; internal set; } = 20;
        public List<CardBase> ManaField {get; internal set; } = new();
        public List<CardBase> CombatField {get; private set; } = new();
        public List<CardBase> Hand {get; private set; } = new();
        public List<CardBase> Deck { get; private set; } = new();
        public List<CardBase> Graveyard { get; private set; } = new();
        public List<CardBase> Exiled { get; private set; } = new();
        public bool IsLandPlayedThisTurn { get; internal set; }

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

            if(value > 0)
            {
                Console.Write($"Player {ID} otrzymał {value} obrażeń.");
            }  
            Health -= value;
            Console.WriteLine($"Aktualne HP: {Health}");
            Console.ResetColor();
        }
        public void Heal(int value)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            if(value > 0)
            {
                Console.Write($"Player {ID} został uleczony o {value} hp.");
            }  
            Health += value;
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
            attackers.ForEach(index=>_attackersToDeclare.Add((Creature)CombatField[Int32.Parse(index)]));
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
                    _deffendersToDeclare.Add((Creature)CombatField[deffender], attacker);
                }
            }
            Console.ResetColor();

            return _deffendersToDeclare;
        }
        public void PlayCardFromHand(CardBase card, List<CardBase> landToPayCost)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"Gracz 1 zagrywa kartą {card.Name}");
            Hand.Remove(card);
            switch(card)
            {
                case Creature c:  
                    if(! c.CardSpecialActions.Any(x=>x.description == "Haste"))
                    {
                        c.isTapped = true;
                    }
                    CombatField.Add(card);
                    card.UseSpecialAction(ActionType.Play);
                    break;

                case Land:
                    ManaField.Add(card);
                    card.UseSpecialAction(ActionType.Play);
                    break;
            }
            landToPayCost.ForEach(land=>((Land)land).isTapped = true);
            Console.ResetColor();
        }
        public void DisplayPlayerField()
        {
            Console.ForegroundColor = color;
            foreach (Creature creature in CombatField.Where(c => (c is Creature) && ((Creature)c).isTapped == false))
                Console.WriteLine($"[{CombatField.IndexOf(creature)}] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth}) <Mana:{creature.ManaCostString}>");
           
            foreach (Creature creature in CombatField.Where(c => (c is Creature) && ((Creature)c).isTapped))
                Console.WriteLine($"[ TAPPED ] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth}) <Mana:{creature.ManaCostString}>");
            Console.ResetColor();
        }
        public Dictionary<int,(bool result,List<CardBase> landsToTap)> DisplayPlayerHand()
        {
            string myavailableMana = String.Join(",",SumAvailableManaFromManaField().Select(x=>$"{x.Key} = {x.Value}")).ToString();
            Console.ForegroundColor = color;
            Console.WriteLine($"Player {ID} Hand:");
            Console.WriteLine($"Current available Mana: {myavailableMana}");
            Dictionary<int,(bool result,List<CardBase> landsToTap)> validpickwithcostcast = new();
            for(int index = 0;index < Hand.Count; index++)
            {   validpickwithcostcast.Add(index,Hand[index].CheckAvailability());
                Console.WriteLine($"[{validpickwithcostcast[index].result.ToString().PadLeft(5)}] [{index}] {Hand[index].GetCardString()}");
            }
            Console.ResetColor();
            return validpickwithcostcast;
        }
        public Dictionary<string, int> SumAvailableManaFromManaField()
        {
            Dictionary<string, int> SumManaOwnedAndAvailable = new();
            //sumowanie dostepnej many
            foreach (Land manaCard in ManaField.Where(c => c is Land && c.isTapped == false))
            {
                foreach (var mana in manaCard.manaValue)
                {
                    if (SumManaOwnedAndAvailable.ContainsKey(mana.Key))
                    {
                        SumManaOwnedAndAvailable[mana.Key] += mana.Value;
                    }
                    else
                    {
                        SumManaOwnedAndAvailable.Add(mana.Key, mana.Value);
                    }
                }
            }

            return SumManaOwnedAndAvailable;
        }
    }
}