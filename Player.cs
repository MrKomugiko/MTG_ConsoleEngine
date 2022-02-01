using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Player
    {
        public int PlayerNumberID { get;  internal set; } 
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
            this.PlayerNumberID = _id;
            this.Health = _health;
        
            if(_id == 1) color = ConsoleColor.Blue;
            else color= ConsoleColor.Green;
        }
        public void DealDamage(int value)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;

            if(value > 0)
            {
                Console.Write($"Player {PlayerNumberID} otrzymał {value} obrażeń. ");
            }  
            Health -= value;
            Console.WriteLine($"Aktualne HP: {Health}");
            Console.ResetColor();
        }
        public void Heal(int value)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;

            if(value > 0)
            {
                Console.Write($"Player {PlayerNumberID} został uleczony o {value} hp.");
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
            var newCard = Deck.First();
            Deck.Remove(newCard);
            Hand.Add(newCard);
            Console.WriteLine($"Player {PlayerNumberID} dobrał karte: {newCard.Name}");
            Console.ResetColor();
        }
        internal List<Creature> Get_AvailableAttackers() {
            Console.WriteLine("dostępne kreatury do ataku: "); 

            var possibleAttackers = CombatField.Where(x=> x.isTapped == false && x is Creature ).Select(x=>(Creature)x).ToList();
            possibleAttackers.ForEach(x => 
                    Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
                );
            
            return possibleAttackers;
        } 
        internal List<Creature> Get_AvailableDeffenders() {
            var possibleDefenders = CombatField.Where(x=>x.isTapped == false && x is Creature ).Select(x=>(Creature)x).ToList();
           
            if(possibleDefenders.Count == 0) return new();

            Console.WriteLine("Dostępne jednostki do obrony");
            possibleDefenders.ForEach(x => 
                Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
            );
            return possibleDefenders;

        } 
        internal List<Creature> SelectAttackers()
        {
            TryAgain:
            Console.ForegroundColor = color;
            DisplayPlayerField();
            Console.WriteLine("Wybierz atakujących z dostępnych kreatur na polu: \n[ indexy oddzielaj przecinkiem: 0,1,3... ]");
List<Creature> availableTargets = Get_AvailableAttackers();

            string input = Console.ReadLine()??"";
            Console.ResetColor();
            if(String.IsNullOrEmpty(input)) return new();

            List<string> attackers = input.Trim().Split(",").ToList();
            
            List<Creature> _attackersToDeclare = new();
            
            int attackIndex;
            foreach(var attacker in attackers)
            {
                if(Int32.TryParse(attacker, out attackIndex))
                {
                    if(attackIndex > CombatField.Count-1 || attackIndex < 0 ) {
                        Console.WriteLine("niepoprawny index potworka => "+attackIndex+", pominięto.");
                        continue;
                        }

                    Creature attackingCreature = (Creature)CombatField[attackIndex];
                    if(availableTargets.Contains(attackingCreature))
                    {
                        _attackersToDeclare.Add(attackingCreature);
                        Console.WriteLine("dodano do atakujacych: "+attackingCreature.Name);
                    }
                    else
                    {
                        if(_attackersToDeclare.Contains(attackingCreature)) _attackersToDeclare.Remove(attackingCreature); // TODO: raczej nie potrzebne w tym miejscu

                        Console.WriteLine($"pominieto, {attackingCreature.Name} nie moze teraz walczyć");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"błędna wartość => {attacker}, wprwadz sekwencje np.: 0,1,2");
                    Console.WriteLine("Wprwadz poprawnie cały ciąg, ponownie. Enter to skip.");
                    _attackersToDeclare.Clear();
                    goto TryAgain;
                }
            }
        
            return _attackersToDeclare;
        }
        public Dictionary<Creature,Creature> SelectDeffenders()
        {
            Console.WriteLine("Nadchodzący Atak:");
            foreach (Creature creature in Engine.DeclaredAttackers)
            {
                Console.WriteLine($"[{Engine.DeclaredAttackers.IndexOf(creature)}] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth})");
            }

            Console.ForegroundColor = color;

            Console.WriteLine($"---------- Enemy Attackers ID ----------");
            Engine.Players[PlayerNumberID==1?1:0].DisplayPlayerField();                
            Console.WriteLine($"[{(PlayerNumberID==1?0:1)}] => Your Combat field Cards:");
            this.DisplayPlayerField();
            
            var availableDeffenders = Get_AvailableDeffenders();
            
            if(availableDeffenders.Count == 0){
                Console.WriteLine("twoje pole jest puste, nie masz żadnej kreatury do wystawienia na obronie.");
                return new();
            }
            
            return InputHelper.Input_DefendersDeclaration(Engine.DeclaredAttackers, availableDeffenders, PlayerNumberID-1 /* index */);
//            Console.ResetColor();
        }
        public void PlayCardFromHand(CardBase card, List<CardBase> landToPayCost)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"Gracz 1 zagrywa kartą {card.Name}");
            (bool status, int playerIndex, int creatureIndex) playerResponse = (false,-1,-1);
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

                case Enchantment e:
                    if(e.UseOn == "Creature")   
                    {
                        List<object> availableTargets2 = e.GetValidTargets();
                        if(availableTargets2.Count == 0) return;

                        Console.WriteLine($"[{(PlayerNumberID==1?0:1)}] => Your Combat field Cards:");
                        this.DisplayPlayerField();
                        Console.WriteLine($"[{(PlayerNumberID==1?1:0)}] => Enemies Combat field Cards:");
                        Engine.Players[PlayerNumberID==1?1:0].DisplayPlayerField();                
                        playerResponse = InputHelper.Input_SinglePairPlayerMonster();
                        
                        if(playerResponse.status == true) {
                            e.AssingToCreature((Creature)(Engine.Players[playerResponse.playerIndex].CombatField[playerResponse.creatureIndex]));
                        }
                        else {
                            Console.WriteLine("anuluj, skip");
                            return;
                        }
                    }
                    else
                    {
                        //TODO:
                        Console.WriteLine("Użycie Enchantmenta na postaci");
                        throw new NotImplementedException();
                    }
                    break;

                case Instant i:
                // TODO: rozróżnic kiedy instant potrzebuje celu a kiedy nie
                    List<object> availableTargets = i.GetValidTargets();
                    if(availableTargets.Count == 0) return;

                    Console.WriteLine($"[{(PlayerNumberID==1?0:1)}] => Your Combat field Cards:");
                    this.DisplayPlayerField();
                    Console.WriteLine($"[{(PlayerNumberID==1?1:0)}] => Enemies Combat field Cards:");
                    Engine.Players[PlayerNumberID==1?1:0].DisplayPlayerField();   
                    playerResponse = InputHelper.Input_SinglePairPlayerMonster();
                    
                    if(playerResponse.status == true) 
                    {
                        i.SpellSelectedTarget = ((Creature)(Engine.Players[playerResponse.playerIndex].CombatField[playerResponse.creatureIndex]));             
                        i.UseSpecialAction(ActionType.CastInstant);
                    }
                    else 
                    {
                        Console.WriteLine("anuluj, skip");
                        return;
                    }
                    
                    break;
            }

            Hand.Remove(card);
            landToPayCost.ForEach(land=>((Land)land).isTapped = true);
            Console.ResetColor();
        }
        public void DisplayPlayerField() => TableHelpers.DisplayFieldTable(color, CombatField, PlayerNumberID);
       // public void DisplayPlayerAttackers() => TableHelpers.DisplayFieldTable(color, Engine.DeclaredAttackers.Select(x=>((CardBase)x)).ToList(), PlayerNumberID);
        public Dictionary<int,(bool result,List<CardBase> landsToTap)> Get_and_DisplayPlayerHand()
        {
            Dictionary<string,int> currentMana = SumAvailableManaFromManaField();

            TableHelpers.DisplayHandTable(color, currentMana, PlayerNumberID, Health, Hand);
            
            string myavailableMana = String.Join(",",currentMana.Select(x=>$"{x.Key} = {x.Value}")).ToString();

            Dictionary<int,(bool result,List<CardBase> landsToTap)> validpickwithcostcast = new ();
            Hand.ForEach(card => validpickwithcostcast.Add(key:Hand.IndexOf(card), value:card.CheckAvailability()));
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

        internal void ShuffleDeck()
        {
            var rand = new Random();
            for (var i = Deck.Count - 1; i > 0; i--)
            {
                var temp = Deck[i];
                var index = rand.Next(0, i + 1);
                Deck[i] = Deck[index];
                Deck[index] = temp;
            }
        }
    }
}