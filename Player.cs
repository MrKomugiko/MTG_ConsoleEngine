using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public partial class Player : PlayerBase
    {

        public Player(int _number, int _health) 
        {
            this.IsAI = false;
            this.PlayerNumberID = _number;
            this.PlayerIndex = _number==1?0:1;

            this.Health = _health;
        
            if(_number == 1) color = ConsoleColor.Blue;
            else color= ConsoleColor.Green;

            SumAvailableManaFromManaField();
        }
        public override void DealDamage(int value)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;

            if (value > 0)
            {
                Console.Write($"Player {PlayerNumberID} otrzymał {value} obrażeń. ");
            }
            Health -= value;
            Console.WriteLine($"Aktualne HP: {Health}");
            Console.ResetColor();
        }
        public override void Heal(int value)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;

            if (value > 0)
            {
                Console.Write($"Player {PlayerNumberID} został uleczony o {value} hp.");
            }
            Health += value;
            Console.WriteLine($"Aktualne HP: {Health}");
            Console.ResetColor();
        }
        public override void DrawCard()
        {
            Console.ForegroundColor = color;
            if (Deck.Count > 0)
            {
                var newCard = Deck.First();
                Deck.Remove(newCard);
                Hand.Add(newCard);
                Console.WriteLine($"Player {PlayerNumberID} dobrał karte: {newCard.Name}");
            }
            else
            {
                Console.WriteLine("PRZEGRANA - BRAK KART!");
                Health = -666;
            }
            Console.ResetColor();
        }
        public override Creature[] Get_AvailableAttackers() {

            var possibleAttackers = CombatField.Where(x=> x.IsTapped == false && x is Creature ).Select(x=>(Creature)x).ToArray();
            if(possibleAttackers.Length == 0) {
                Console.WriteLine("Brak posiadanych jednostek gotowych do ataku");
                return Array.Empty<Creature>();
            }
           // Console.WriteLine("dostępne kreatury do ataku: ");
            //possibleAttackers.ForEach(x =>
            //        Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
            //    );

            return possibleAttackers;
        } 
        public override List<Creature> Get_AvailableDeffenders() {
            var possibleDefenders = CombatField.Where(x=>x.IsTapped == false && x is Creature ).Select(x=>(Creature)x).ToList();
            if(possibleDefenders.Count == 0) return new();

            Console.WriteLine("Dostępne jednostki do obrony");
            possibleDefenders.ForEach(x =>
                Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
            );
            return possibleDefenders;
        }
       
        public Creature[] SelectAttackers_HumanInteraction()
        {
            TryAgain:
            Console.ForegroundColor = color;
            DisplayPlayerField();
         //   Console.WriteLine("Wybierz atakujących z dostępnych kreatur na polu: [ indexy oddzielaj przecinkiem: 0,1,3... ]");
            // TODO: zrobic kopie tej metody dla automatycznej odpowiedzi ze strony AI
            Creature[] availableTargets = Get_AvailableAttackers();
            if(availableTargets.Length == 0) return Array.Empty<Creature>();

            string input = Console.ReadLine()??"";
            
            if(String.IsNullOrEmpty(input)) return Array.Empty<Creature>();

            List<string> attackers = input.Trim().Split(",").ToList();
            
            List<Creature> _attackersToDeclare = new();
            List<int> validIndexes = new();
            
            foreach(var attacker in attackers)
            {
                if(Int32.TryParse(attacker, out int attackIndex))
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
            Console.ResetColor();
            return GetCreaturesFromField(validIndexes.ToArray(),false);
        }

        public Dictionary<Creature,Creature> SelectDeffenders_HumanInteraction()
        {
          //  Console.WriteLine("Nadchodzący Atak:");
            Console.ResetColor();
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
            
            return InputHelper.Input_DefendersDeclaration(_gameEngine, DeclaredAttackers.ToList(), availableDeffenders, PlayerIndex /* index */);
        }
        public override bool PlayCardFromHand(CardBase card, List<CardBase> landsToPay = null)
        {
            Console.ForegroundColor = color;
           // Console.WriteLine($">>>>>>>>>>>>>>>>> Gracz {PlayerNumberID} zagrywa kartą {card.Name}");
            (bool status, int playerIndex, int creatureIndex) playerResponse = (false, -1, -1);

            switch (card)
            {
                case Creature c:  
                    if(! c.CardSpecialActions.Any(x=>x.description == "Haste"))
                    {
                        c.IsTapped = true;
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
                        List<CardBase> availableTargets2 = _gameEngine.GetValidTargetsForCardType(e);
                        if(availableTargets2.Count == 0) return false;

                        Console.WriteLine($"[{(this.PlayerIndex)}] => Your Combat field Cards:");
                        this.DisplayPlayerField();
                        Console.WriteLine($"[{(Opponent.PlayerIndex)}] => Enemies Combat field Cards:");
                        _gameEngine.Players[Opponent.PlayerIndex].DisplayPlayerField();                
                        playerResponse = InputHelper.Input_SinglePairPlayerMonster(_gameEngine);
                        
                        if(playerResponse.status == true) {
                            e.AssingToCreature((Creature)(_gameEngine.Players[playerResponse.playerIndex].CombatField[playerResponse.creatureIndex]));
                            e.UseSpecialAction(ActionType.OnEnnchantAdded);
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
                    //List<object> availableTargets = _gameEngine.GetValidTargetsForCardType(i);

                    //if(availableTargets.Count == 0) return false;

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
            //card.CheckAvailability().landsToTap.ForEach(c => c.isTapped = true);
            Hand.Remove(card);
            RefreshManaStatus();
            Console.ResetColor();
            return true;
        }
        public override void DisplayPlayerField() => TableHelpers.DisplayFieldTable(_gameEngine, color, CombatField, PlayerNumberID);
        public Dictionary<int,(bool result,List<CardBase> landsToTap)> Get_and_DisplayPlayerHand()
        {
            SumAvailableManaFromManaField();
            int[] currentMana = _currentTotalManaStatus;

            TableHelpers.DisplayHandTable(color, currentMana, PlayerNumberID, Health, Hand);

            Dictionary<int, (bool result, List<CardBase> landsToTap)> validpickwithcostcast = new();
            Hand.ForEach(card => validpickwithcostcast.Add(key: Hand.IndexOf(card), value: card.CheckAvailability()));
            return validpickwithcostcast;
        }

        //public void GetAllPossibleSpellCardsCombinations()
        //{
        //    // print available hand
        //    Console.WriteLine("Print hand skills list");
        //    Dictionary<CardBase, (bool result, List<CardBase> landsToPay)> CurrentHandDetailedData = new();
        //    Hand.ForEach(x => CurrentHandDetailedData.Add(x, x.CheckAvailability()));
        //    AvailableCardSpellInHand = CurrentHandDetailedData.Where(X => X.Value.result && X.Key is not Creature && X.Key is not Land).ToList();

        //    Console.WriteLine(" ------------------------------------------------------------------------------- ");

        //    Console.WriteLine("|  [ID]  |  [Can be used?]  |           [Name]           |     [Mana cost]      |");
        //    Console.WriteLine("|--------|------------------|----------------------------|----------------------|");
        //    foreach (var card in AvailableCardSpellInHand)
        //    {
        //        Console.WriteLine($"|    {card.Key.ID,2}  | {card.Value.result,16} | {card.Key.Name,26} | {card.Key.ManaCostString,20} |");
        //    }
        //    if (AvailableCardSpellInHand.Count == 0)
        //    {
        //        Console.WriteLine("         Brak kart skili któe mołbys użyć, lub nie stać cie na żadną");
        //    }
        //    Console.WriteLine(" ------------------------------------------------------------------------------- ");

            
        //    var availableCardsToCombine = AvailableCardSpellInHand.Select(x => x.Key.ID).ToList();

        //    int[] arr = AvailableCardSpellInHand.Select(x => x.Key.ID).ToArray();
        //    List<int[]> OUTPUT = new List<int[]>();
        //    foreach (var item in arr)
        //    {
        //        int[] parentState = new int[1] { item };
        //        int[] temp = arr;
        //        int[] itemsLeft = new int[temp.Length - 1];

        //        int index = 0;
        //        foreach (int leftitem in temp)
        //        {
        //            if (leftitem == parentState[parentState.Length - 1]) continue;

        //            itemsLeft[index++] = leftitem;
        //        }

        //        RecurPowerSet(ref OUTPUT, parentState, itemsLeft);
        //    }
        //}
        
    }
}