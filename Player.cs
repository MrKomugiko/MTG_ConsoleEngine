using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public partial class Player
    {
        public readonly bool IsAI = false;
        public Engine _gameEngine;
        public Player Opponent;

        public readonly int PlayerNumberID;
        public readonly int PlayerIndex;
        public readonly ConsoleColor color;

        public int Health { get; internal set; } = 20;
        public List<CardBase> ManaField {get; internal set; } = new();
        public List<CardBase> CombatField {get; private set; } = new();
        public List<CardBase> Hand {get; private set; } = new();
        public List<CardBase> Deck { get; private set; } = new();
        public List<CardBase> Graveyard { get; private set; } = new();
        public List<CardBase> Exiled { get; private set; } = new();
        public bool IsLandPlayedThisTurn { get; internal set; }

        
        public Player(int _number, int _health, bool _isAI = false) 
        {
            this.IsAI = _isAI;
            this.PlayerNumberID = _number;
            this.PlayerIndex = _number==1?0:1;

            this.Health = _health;
        
            if(_number == 1) color = ConsoleColor.Blue;
            else color= ConsoleColor.Green;
        }
        public void DealDamage(int value)
        {
          //  Console.BackgroundColor = ConsoleColor.DarkRed;
         //   Console.ForegroundColor = ConsoleColor.White;

           // if(value > 0)
          //  {
          //      Console.Write($"Player {PlayerNumberID} otrzymał {value} obrażeń. ");
          //  }  
            Health -= value;
            //Console.WriteLine($"Aktualne HP: {Health}");
          //  Console.ResetColor();
        }
        public void Heal(int value)
        {
         //   Console.BackgroundColor = ConsoleColor.DarkGreen;
         //   Console.ForegroundColor = ConsoleColor.White;

         //   if(value > 0)
          //  {
          //      Console.Write($"Player {PlayerNumberID} został uleczony o {value} hp.");
          //  }  
            Health += value;
            //Console.WriteLine($"Aktualne HP: {Health}");
          //  Console.ResetColor();
        }
        public void AddToDeck(CardBase _card)
        {
            _card.Owner = this;
            Deck.Add(_card);
        }
        public void DrawCard()
        {
         //   Console.ForegroundColor = color;
            var newCard = Deck.First();
            Deck.Remove(newCard);
            Hand.Add(newCard);
         
            //Console.WriteLine($"Player {PlayerNumberID} dobrał karte: {newCard.Name}");
          //  Console.ResetColor();
        }
        public List<Creature> Get_AvailableAttackers() {

            var possibleAttackers = CombatField.Where(x=> x.isTapped == false && x is Creature ).Select(x=>(Creature)x).ToList();
            if(possibleAttackers.Count == 0) {
                //Console.WriteLine("Brak posiadanych jednostek gotowych do ataku");
                return new();
            }
            //Console.WriteLine("dostępne kreatury do ataku: ");
            //possibleAttackers.ForEach(x =>
            //        Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
            //    );

            return possibleAttackers;
        } 
        private List<Creature> Get_AvailableDeffenders() {
            var possibleDefenders = CombatField.Where(x=>x.isTapped == false && x is Creature ).Select(x=>(Creature)x).ToList();
            if(possibleDefenders.Count == 0) return new();

          //  Console.WriteLine("Dostępne jednostki do obrony");
          //  possibleDefenders.ForEach(x => 
          //      Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
          //  );
            return possibleDefenders;

        }
       
        public List<Creature> SelectAttackers_HumanInteraction()
        {
            TryAgain:
          //  Console.ForegroundColor = color;
            DisplayPlayerField();
         //   Console.WriteLine("Wybierz atakujących z dostępnych kreatur na polu: [ indexy oddzielaj przecinkiem: 0,1,3... ]");
            // TODO: zrobic kopie tej metody dla automatycznej odpowiedzi ze strony AI
            List<Creature> availableTargets = Get_AvailableAttackers();
            if(availableTargets.Count == 0) return new();

            string input = Console.ReadLine()??"";
            //Console.ResetColor();
            if(String.IsNullOrEmpty(input)) return new();

            List<string> attackers = input.Trim().Split(",").ToList();
            
            List<Creature> _attackersToDeclare = new();
            List<int> validIndexes = new();
            int attackIndex;
            foreach(var attacker in attackers)
            {
                if(Int32.TryParse(attacker, out attackIndex))
                {
                    if(attackIndex > CombatField.Count-1 || attackIndex < 0 ) {
                   //     Console.WriteLine("niepoprawny index potworka => "+attackIndex+", pominięto.");
                        continue;
                        }

                    Creature attackingCreature = (Creature)CombatField[attackIndex];
                    if(availableTargets.Contains(attackingCreature))
                    {
                        validIndexes.Add(attackIndex);
                    }
                    else
                    {
                   //     Console.WriteLine($"pominieto, {attackingCreature.Name} nie moze teraz walczyć");
                        continue;
                    }
                }
                else
                {
             //       Console.WriteLine($"błędna wartość => {attacker}, wprwadz sekwencje np.: 0,1,2");
              //      Console.WriteLine("Wprwadz poprawnie cały ciąg, ponownie. Enter to skip.");
                    _attackersToDeclare.Clear();
                    goto TryAgain;
                }
            }
        
            return GetCreaturesFromField(validIndexes.ToArray(),false);
        }

        public Dictionary<Creature,Creature> SelectDeffenders_HumanInteraction()
        {
          //  Console.WriteLine("Nadchodzący Atak:");
            var DeclaredAttackers = _gameEngine.GetAttackerDeclaration();
          //  foreach (Creature creature in DeclaredAttackers)
          //  {
          //      Console.WriteLine($"[{DeclaredAttackers.IndexOf(creature)}] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth})");
          //  }

          //  Console.WriteLine($"---------- Enemy Attackers ID ----------");
            _gameEngine.Players[Opponent.PlayerIndex].DisplayPlayerField();                
        //    Console.WriteLine($"[{(PlayerIndex)}] => Your Combat field Cards:  <--- niepotrzebne,usunąć ?");
            this.DisplayPlayerField();
            
            // TODO: zrobic kopie tej metody dla automatycznej odpowiedzi ze strony AI
            var availableDeffenders = Get_AvailableDeffenders();
            
            if(availableDeffenders.Count == 0){
          //      Console.WriteLine("twoje pole jest puste, nie masz żadnej kreatury do wystawienia na obronie.");
                return new();
            }
            
            return InputHelper.Input_DefendersDeclaration(_gameEngine, DeclaredAttackers, availableDeffenders, PlayerIndex /* index */);
        }
        public bool PlayCardFromHand(CardBase card)
        {
           // Console.ForegroundColor = color;
          //  Console.WriteLine($">>>>>>>>>>>>>>>>> Gracz {PlayerNumberID} zagrywa kartą {card.Name}");
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
                    if (IsLandPlayedThisTurn == true) return false;
                    ManaField.Add(card);
                    card.UseSpecialAction(ActionType.Play);
                    break;

                case Enchantment e:
                    if(e.UseOn == "Creature")   
                    {
                        List<object> availableTargets2 = _gameEngine.GetValidTargetsForCardType(e);
                        if(availableTargets2.Count == 0) return false;

                        Console.WriteLine($"[{(this.PlayerIndex)}] => Your Combat field Cards:");
                        this.DisplayPlayerField();
                        Console.WriteLine($"[{(Opponent.PlayerIndex)}] => Enemies Combat field Cards:");
                        _gameEngine.Players[Opponent.PlayerIndex].DisplayPlayerField();                
                        playerResponse = InputHelper.Input_SinglePairPlayerMonster(_gameEngine);
                        
                        if(playerResponse.status == true) {
                            e.AssingToCreature((Creature)(_gameEngine.Players[playerResponse.playerIndex].CombatField[playerResponse.creatureIndex]));
                        }
                        else {
                            Console.WriteLine("anuluj, skip");
                            return false;
                        }
                    }
                    else
                    {
                        //TODO: rozwazyc opcje dodania enczanta permamentnego, ktory poprostu jest na stole 
            //            Console.WriteLine("Użycie Enchantmenta na postaci/stole");
                        throw new NotImplementedException();
                    }
                    break;

                case Instant i:
                    List<object> availableTargets = _gameEngine.GetValidTargetsForCardType(i);

                    if(availableTargets.Count == 0) return false;

                  //  Console.WriteLine($"[{this.PlayerIndex}] => Your Combat field Cards:");
                    this.DisplayPlayerField();
                 //   Console.WriteLine($"[{(Opponent.PlayerIndex)}] => Enemies Combat field Cards:");
                    _gameEngine.Players[Opponent.PlayerIndex].DisplayPlayerField();   
                    playerResponse = InputHelper.Input_SinglePairPlayerMonster(_gameEngine);
                    
                    if(playerResponse.status == true) 
                    {
                        i.SpellSelectedTarget = ((Creature)(_gameEngine.Players[playerResponse.playerIndex].CombatField[playerResponse.creatureIndex]));             
                        i.UseSpecialAction(ActionType.CastInstant);
                    }
                    else 
                    {
              //          Console.WriteLine("anuluj, skip");
                        return false;
                    }
                    
                    break;
            }
            //Console.WriteLine("zapłąc za karte / usun ją z ręki");
            card.CheckAvailability().landsToTap.ForEach(c => c.isTapped = true);
            Hand.Remove(card);
            //Console.ResetColor();
            return true;
        }
        public void DisplayPlayerField() => TableHelpers.DisplayFieldTable(_gameEngine, color, CombatField, PlayerNumberID);
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
    }
}