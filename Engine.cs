using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public partial class Engine
    {
        private delegate void Phase(Player _player);
        private List<Phase> GamePhases = new List<Phase>();
        public readonly Player[] Players = new Player[2];
        private PhaseType CurrentPhase { get; set; }
        private List<Creature> DeclaredAttackers = new();
        public static Dictionary<Creature,Creature> DeclaredDeffenders = new(); // <obrońca / atakujacy> , bo mozna blokowac kilkoma stworkami
        private int TurnCounter = 0;
        public static int CurrentPlayerIndex = 0;
        
        public Engine(Player FirstPlayer, Player SecondPlayer)
        {
            GamePhases.Add(Beginning_Phase);
            GamePhases.Add(First_Main_Phase);
            GamePhases.Add(Combat_Phase);
            GamePhases.Add(Second_Main_Phase);
            GamePhases.Add(End_Phase);

            Players[0] = FirstPlayer;
            Players[0]._gameEngine = this;
            Players[1] = SecondPlayer;
            Players[1]._gameEngine = this;

        }
        //public void SetAttackerDeclaration(List<Creature> _creaturesToDeclareAsAttacker) => DeclaredAttackers = _creaturesToDeclareAsAttacker;
        public List<Creature> GetAttackerDeclaration() => DeclaredAttackers;
        public Dictionary<Creature,Creature> GetDeffendersDeclaration()=> DeclaredDeffenders;
        
        public void Start()
        {
            ShuffleDeck(Players[0]);
            ShuffleDeck(Players[1]);

            for(int i = 0;i<7;i++) Players[0].DrawCard();
            for(int i = 0;i<7;i++) Players[1].DrawCard();

            TurnCounter = 0;
            CurrentPlayerIndex = 0;

            while(true)
            {
                foreach(var Phase in GamePhases)
                {
                    if(CurrentPlayerIndex == 1)
                    {
                        AI_GetPossibleMoves(CurrentPlayerIndex, Phase.ToString());
                    }
                    Phase(Players[CurrentPlayerIndex]);
                }

                CurrentPlayerIndex = CurrentPlayerIndex==0?1:0;
                TurnCounter++;
            };
        }


        public void Beginning_Phase(Player _player)
        {   
            CurrentPhase = PhaseType.Beginning_Phase;
            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║    {("[Turn:"+TurnCounter+"]").PadRight(11)}             Beginning_Phase: Player {_player.PlayerNumberID}                           ║");
            Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");

            Console.WriteLine(" - Untap Step [ DONE ]");
            _player.CombatField.ForEach(card => card.isTapped = false);
            _player.ManaField.ForEach(card => card.isTapped = false);

            Console.WriteLine(" - Upkeep Step [Work In Progress]");
            // Console.WriteLine("\t odpalenie akcji wykonujacych sie na początku tury gracza");
            
            Console.WriteLine(" - Draw step [ DONE ]");
            if(TurnCounter >= 2)
            {
                _player.DrawCard();
            }
        }
        public void First_Main_Phase(Player _player)
        {
            CurrentPhase = PhaseType.First_Main_Phase;
            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║                            First_Main_Phase: Player {_player.PlayerNumberID}                          ║");
            Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
            // Console.WriteLine("\t Cast spells, instants, play enchantments, play creatures");
            // Console.WriteLine("enter card index to play with / enter to skip ");
            Dictionary<int,(bool result,List<CardBase> landsToTap)> handdata = new();
            while(true)
            {
                handdata = _player.Get_and_DisplayPlayerHand();
                
                var input = Console.ReadLine()??"";
                if(String.IsNullOrEmpty(input)) break;
                
                int choosenIndex;
                if(Int32.TryParse(input, out choosenIndex) == false) 
                {
                    
                    continue;
                }
                if(handdata[Int32.Parse(input)].result == false)
                {
                    Console.WriteLine("nie mozesz zagrać tej karty i / lub nie stac cie");
                    continue;
                }
                
                _player.PlayCardFromHand(_player.Hand[Int32.Parse(input)], handdata[Int32.Parse(input)].landsToTap);
            }
        }

        Random rand = new Random();
        public void Combat_Phase(Player _player)
        {
            CurrentPhase = PhaseType.Combat_Phase;
            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║                              Combat_Phase: Player {_player.PlayerNumberID}                            ║");
            Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine(" - Beginning of Combat Step");

            Console.WriteLine(" - Declare Attackers"); // playerID
            //List<Creature> availableAttackers = _player.Get_AvailableAttackers();

            if(_player.IsAI)
            {
                /*_AI_*/
                List<Creature> availableTargets = _player.Get_AvailableAttackers();
                List<int[]> randomIndexesList = _player.GetAllPossibleAttackCombinationsIndexes(availableTargets);
                int[] inputArr = randomIndexesList[rand.Next(randomIndexesList.Count)];
                
                DeclaredAttackers = _player.SelectAttackers_AI(inputArr, true);
            }
            else
            {
                DeclaredAttackers = _player.SelectAttackers_HumanInteraction();
            }

            if (DeclaredAttackers.Count > 0)
            {   
                AI_GetPossibleMoves(_player.Opponent.PlayerIndex, "DeclareBlockers");

                Console.WriteLine(" - Declare Blockers Step - Player "+_player.Opponent.PlayerNumberID);  // drugi gracz
                
                /* <DEFFENDER, ATTACKER> */ DeclaredDeffenders = _player.Opponent.SelectDeffenders_HumanInteraction();
            }
            Console.ResetColor();

            CastInstantIfCan(_player); // gracz zaczynajacy
            
            AI_GetPossibleMoves(_player.Opponent.PlayerIndex, "Cast Instant");
            CastInstantIfCan(_player.Opponent); // przeciwnik
            
            ExecuteCombat();
           
            DeclaredAttackers.Clear();
            DeclaredDeffenders.Clear();

            // ewentualne wyłączenie enczantow 
            Console.WriteLine("Przenoszenie zniszczonych jednostek na cmentarz");
            MoveDeadCreaturesToGraveyard(_player.Opponent);
            MoveDeadCreaturesToGraveyard(_player);

            Console.WriteLine(" - End of combat step");
        }
        public void Second_Main_Phase(Player _player)
        {
            CurrentPhase = PhaseType.Second_Main_Phase;
            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║                           Second_Main_Phase: Player {_player.PlayerNumberID}                          ║");
            Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
            //Console.WriteLine("Second_Main_Phase: Player "+_player.ID);
            Dictionary<int,(bool result,List<CardBase> landsToTap)> handdata = new();
            while(true)
            {
                handdata = _player.Get_and_DisplayPlayerHand();
                
                var input = Console.ReadLine()??"";
                if(String.IsNullOrEmpty(input)) break;
                
                int choosenIndex;
                if(Int32.TryParse(input, out choosenIndex) == false) 
                {
                    
                    continue;
                }
                if(handdata[Int32.Parse(input)].result == false)
                {
                    Console.WriteLine("nie mozesz zagrać tej karty i / lub nie stac cie");
                    continue;
                }
                
                _player.PlayCardFromHand(_player.Hand[Int32.Parse(input)], handdata[Int32.Parse(input)].landsToTap);
            }
        }
        public void End_Phase(Player _player)
        {
            CurrentPhase = PhaseType.End_Phase;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                               End Phase: Player 2                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════╝");
            //Console.WriteLine("End_Phase: Player " + _player.ID);
            //Console.WriteLine(" - End of turn step");
            _player.IsLandPlayedThisTurn = false;

            Console.WriteLine("przywracanie hp ocalałym jednostom");
            RecoverHealthSurvivedCreatures();
          //  _player.DisplayPlayerField();
            Console.WriteLine(" - Cleanup step");
            Console.WriteLine("Sprawdzanie ilosci kart na ręce przeciwnika) - 7 max inaczej wyrzuć jakąś przed dostaniem kolejnej.");
            
            AI_GetPossibleMoves(_player.PlayerNumberID == 1 ? 1 : 0, "Remove hand over limit.");
            HandCardsCleanUpCountChecker(Players[_player.PlayerNumberID == 1 ? 1 : 0]);
        }

 

        private void ExecuteCombat()
        {
            foreach(var attacker in DeclaredAttackers) /* <DEFFENDER, ATTACKER> */
            {
                int attackerMaxHP = attacker.CurrentHealth;
                List<int> defendersHpBeforeCombat = new();
                Console.WriteLine($"Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                if(DeclaredDeffenders.Any(x=>x.Value == attacker))
                {
                    if(attacker.CurrentHealth <= 0) {
                        Console.WriteLine("Atakujaca jednostka juz nie zyje, walka pominięta");
                        continue;
                    }
                    foreach(var defender in DeclaredDeffenders.Where(x=>x.Value == attacker))
                    {
                        defendersHpBeforeCombat.Add(attacker.CurrentHealth);
                        // pomin w przypadku gdy deffender juz ni e zyje xd

                        Console.WriteLine($"Broniąca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");
                        
                        if(defender.Key.CurrentHealth <= 0) continue;

                        attacker.Attack(defender.Key);
                    }

                    Console.WriteLine("---- wynik potyczki ----");
                    Console.WriteLine($"{(attacker.CurrentHealth <= 0?"DEAD":(attacker.CurrentHealth < attackerMaxHP)?"DAMAGED":"UNTOUCHED")} Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                    int index = 0;
                    foreach(var defender in DeclaredDeffenders.Where(x=>x.Key == attacker))
                    {
                        Console.WriteLine($"{(defender.Key.CurrentHealth <= 0?"DEAD":(defender.Key.CurrentHealth < defendersHpBeforeCombat[index])?"DAMAGED":"UNTOUCHED")} Broniąca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");
                        index++;
                    }
                }
                attacker.Attack(_defender:null);

                Console.WriteLine();
            }
        }
        
        
      
        private void HandCardsCleanUpCountChecker(Player _player)
        {
            if(TurnCounter < 2) return;

            while(_player.Hand.Count >= 7)
            {
                _player.Get_and_DisplayPlayerHand();
                Console.WriteLine($"Wyrzuć {_player.Hand.Count - 6} kart, podaj indexy 0,1,2..");
                var cardsToRemove = Console.ReadLine();

                if(String.IsNullOrEmpty(cardsToRemove) == false)
                {
                    var removearr = cardsToRemove.Split(",").Select(x=>Int32.Parse(x)).ToList();
                    var cardsList = removearr.Select(x=>_player.Hand[x]).ToList();
                    cardsList.ForEach(x=>
                    {
                        Console.WriteLine("Wyrzuciłeś "+x.Name);
                        _player.Hand.Remove(x);
                    });
                }
            }
        }
        private void RecoverHealthSurvivedCreatures()
        {
            Console.WriteLine("Work in progress...");
        }
        private void CastInstantIfCan(Player _player)
        {
            if (_player.Hand.Any(x => (x is Instant) && ((Instant)x).isAbleToPlay))
            {
                Console.WriteLine($"Szana na kontre uzywajac instantów lub umiejetnosci kart gracz {Players[_player.PlayerNumberID == 1 ? 1 : 0].PlayerNumberID}");
                Console.WriteLine("---------------- INSTANTS ONLY ---------------- ");
                Dictionary<int, (bool result, List<CardBase> landsToTap)> handdata = new();
                while (true)
                {
                    handdata = _player.Get_and_DisplayPlayerHand();

                    var input = Console.ReadLine() ?? "";
                    if (String.IsNullOrEmpty(input)) break;

                    int choosenIndex;
                    if (Int32.TryParse(input, out choosenIndex) == false)
                    {
                        continue;
                    }
                    if (handdata[choosenIndex].result == false)
                    {
                        Console.WriteLine("nie mozesz zagrać tej karty i / lub nie stac cie");
                        continue;
                    }

                    if(_player.Hand[choosenIndex] is Instant == false)
                    {
                        Console.WriteLine("Wybierz kartę typu instant, enter zeby anulowac");
                        continue;
                    }

                    _player.PlayCardFromHand(_player.Hand[Int32.Parse(input)], handdata[Int32.Parse(input)].landsToTap);
                }
            }
        }
        private void MoveDeadCreaturesToGraveyard(Player _player)
        {
            var DeadCreatures = _player.CombatField.Where(c => ((Creature)c).CurrentHealth <= 0).ToList();
            _player.Graveyard.AddRange(DeadCreatures);
            for(int i = 0; i<DeadCreatures.Count; i++)
            {
                var corpse = DeadCreatures[i];
                _player.CombatField.Remove(corpse);
                Console.WriteLine($"send {corpse.Name} to Graveyard...");
                corpse.Owner.Graveyard.Add(corpse);
                corpse.Owner.CombatField.Remove(corpse);    
            }
        }
    
    
    
         public void AI_GetPossibleMoves(int currentPlayerIndex, string title)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[AI] Sprawdzanie możliwych ruchów. dla "+title);
            Console.ResetColor();
        }
     
        public List<object> GetValidTargetsForCardType(CardBase card)
        {
            if(card is Instant instant)
            {
                // TODO: sposob zeby nadpisywac zwracanie dostepnych celow
                Console.WriteLine("Get valid targets for Enchantment: "+instant.Name);
                List<object> targets = new();
            
                var player1Creatures = Players[0].CombatField.Where(x=>x is Creature).ToList();
                var player2Creatures = Players[1].CombatField.Where(x=>x is Creature).ToList();
                
                player1Creatures.ForEach(target => Console.WriteLine($"[0-{Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));
                player2Creatures.ForEach(target => Console.WriteLine($"[1-{Players[1].CombatField.IndexOf((CardBase)target)}] Player 2 : {((CardBase)target).Name}"));

                targets.AddRange(player1Creatures);
                targets.AddRange(player2Creatures);

                return targets;
            }
            if(card is Enchantment enchantment)
            {
                     // TODO: sposob zeby nadpisywac zwracanie dostepnych celow
                Console.WriteLine("Get valid targets for Enchantment: "+enchantment.Name);
                List<object> targets = new();
            
                var player1Creatures = Players[0].CombatField.Where(x=>x is Creature).ToList();
                var player2Creatures = Players[1].CombatField.Where(x=>x is Creature).ToList();
                
                player1Creatures.ForEach(target => Console.WriteLine($"[0-{Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));
                player2Creatures.ForEach(target => Console.WriteLine($"[1-{Players[1].CombatField.IndexOf((CardBase)target)}] Player 2 : {((CardBase)target).Name}"));

                targets.AddRange(player1Creatures);
                targets.AddRange(player2Creatures);

                if(targets.Count == 0)
                {
                    Console.WriteLine("Brak dostępnych celów użycia karty...");
                }
                return targets;
            }

            return new();
        }

        public void ShuffleDeck(Player deckOwner)
        {
            var rand = new Random();
            for (var i = deckOwner.Deck.Count - 1; i > 0; i--)
            {
                var temp = deckOwner.Deck[i];
                var index = rand.Next(0, i + 1);
                deckOwner.Deck[i] = deckOwner.Deck[index];
                deckOwner.Deck[index] = temp;
            }
        }
    }
}