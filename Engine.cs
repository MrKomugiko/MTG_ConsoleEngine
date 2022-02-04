using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public partial class Engine
    {
        private delegate void Phase(Player _player);
        private List<Phase> GamePhases = new();

        public readonly Player[] Players = new Player[2];
        public PhaseType CurrentPhase { get; set; }
        public Creature[] DeclaredAttackers;
        public Dictionary<Creature,Creature> DeclaredDeffenders = new(); // <obrońca / atakujacy> , bo mozna blokowac kilkoma stworkami
        private int TurnCounter = 0;
        public int CurrentPlayerIndex = 0;
        public bool logs = true;
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

        public Creature[] GetAttackerDeclaration() => DeclaredAttackers;
        public Dictionary<Creature,Creature> GetDeffendersDeclaration() => DeclaredDeffenders;
        
        public void Start()
        {
            #region StartNewGame
            CurrentPhase = PhaseType.StartNewGame;
            ShuffleDeck(Players[0]);
            ShuffleDeck(Players[1]);

            for (int i = 0; i < 7; i++)
            {
                Players[0].DrawCard();
                Players[1].DrawCard();
            }

            TurnCounter = 0;
            CurrentPlayerIndex = 0;

            #endregion

            bool gameEnded = false;
            RunGameLoop(ref gameEnded);
        }

        public void RunGameLoop(ref bool gameEnded)
        {
            int gamephaselenght = GamePhases.Count;
            while (true)
            {
                for (int i = 0; i < gamephaselenght; i++)
                {
                    if (Players[CurrentPlayerIndex].Health <= 0)
                    {
                        Console.WriteLine("end of game:");
                        Console.WriteLine("player 1 hp:" + Players[0].Health);
                        Console.WriteLine("player 2 hp:" + Players[1].Health);
                        gameEnded = true;
                        return;
                       
                    }
                    GamePhases[i](Players[CurrentPlayerIndex]);
                }

                CurrentPlayerIndex = CurrentPlayerIndex == 0 ? 1 : 0;
                TurnCounter++;
                    Console.WriteLine("Turn: " + TurnCounter);
                if (logs)
                {
                    Console.WriteLine("Player 1 HP:" + Players[0].Health);
                    Console.WriteLine("Player 2 HP:" + Players[1].Health);
                }
            };
        }

        public void Beginning_Phase(Player _player)
        {
            #region Begining_Phase
            CurrentPhase = PhaseType.Beginning_Phase;
            if(logs)
            {
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║    {("[Turn:"+TurnCounter+"]").PadRight(11)}             Beginning_Phase: Player {_player.PlayerNumberID}                           ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");

                Console.WriteLine(" - Untap Step [ DONE ]");
            }
            
            _player.CombatField.ForEach(card => card.isTapped = false);
            _player.ManaField.ForEach(card => card.isTapped = false);
            if (logs)
            {
                Console.WriteLine(" - Upkeep Step [Work In Progress]");
                Console.WriteLine("\t odpalenie akcji wykonujacych sie na początku tury gracza");
                Console.WriteLine(" - Draw step [ DONE ]");
            }

            if(TurnCounter >= 2)
            {
                _player.DrawCard();
            }
            #endregion
        }
        public void First_Main_Phase(Player _player)
        {
            #region First_Main_Phase
            CurrentPhase = PhaseType.First_Main_Phase;
            if (logs)
            {
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║                            First_Main_Phase: Player {_player.PlayerNumberID}                          ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
            }
            if (_player.IsAI)
            {
                _player.AI_MakePlaysWithRandomCardFromHand();
                return;
            }

            while(true)
            {
                Dictionary<int,(bool result,List<CardBase> landsToTap)> handdata = new();
                handdata = _player.Get_and_DisplayPlayerHand();
                
                var input = Console.ReadLine()??"";
                if(String.IsNullOrEmpty(input)) break;
                
                int choosenIndex;
                if(Int32.TryParse(input, out choosenIndex) == false) continue;
                if(choosenIndex >= handdata.Count)
                {
                    Console.WriteLine("niewłasciwy index");
                    continue;
                }

                if(handdata[choosenIndex].result == false)
                {
                    Console.WriteLine("nie mozesz zagrać tej karty i / lub nie stac cie");
                    continue;
                }

                if(_player.PlayCardFromHand(_player.Hand[choosenIndex]))
                {
                    Console.WriteLine("zapłąc za karte");
                    handdata[choosenIndex].landsToTap.ForEach(land => ((Land)land).isTapped = true);
                }
            }
            #endregion
        }

        public void Combat_Phase(Player _player)
        {
            #region Combat_Phase_Begining
            CurrentPhase = PhaseType.Combat_Phase_Begining;
            if (logs)
            {
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║                              Combat_Phase: Player {_player.PlayerNumberID}                            ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
                Console.WriteLine(" - Beginning of Combat Step");

                Console.WriteLine(" - Declare Attackers"); // playerID
            }                                       //List<Creature> availableAttackers = _player.Get_AvailableAttackers();
            #endregion

            #region Combat_Phase_AttackersDeclaration
            CurrentPhase = PhaseType.Combat_Phase_AttackersDeclaration;
            if (_player.IsAI)
            {
                DeclaredAttackers = _player.AI_RandomAttackDeclaration();
            }
            else
            {
                DeclaredAttackers = _player.SelectAttackers_HumanInteraction();
            }
            #endregion

            #region Combat_Phase_DeffendersDeclaration
            CurrentPhase = PhaseType.Combat_Phase_DefendersDeclaration;
            if (DeclaredAttackers.Length > 0)
            {   
                //Console.WriteLine(" - Declare Blockers Step - Player "+_player.Opponent.PlayerNumberID);  // drugi gracz
                if(_player.Opponent.IsAI)
                {
                    DeclaredDeffenders = _player.Opponent.AI_RandomDeffendersDeclaration();
                }
                else
                {
                /* <DEFFENDER, ATTACKER> */ 
                    DeclaredDeffenders = _player.Opponent.SelectDeffenders_HumanInteraction();
                }

            }
            //Console.ResetColor();
            #endregion

            #region Combat_Phase_AttackerInstants
            CurrentPhase = PhaseType.Combat_Phase_AttackerInstants;
            if (_player.IsAI)
            {
                _player.AI_PlayRandomInstatFromHand();
            }
            else
            {
                CastInstantIfCan(_player); // gracz zaczynajacy
            }
            #endregion

            #region Combat_Phase_DeffenderInstants
            CurrentPhase = PhaseType.Combat_Phase_DeffenderInstants;
            if (_player.Opponent.IsAI)
            {
                _player.Opponent.AI_PlayRandomInstatFromHand();
            }
            else
            {
                CastInstantIfCan(_player.Opponent); // gracz zaczynajacy
            }
            #endregion

            #region Combat_Phase_Combat
            CurrentPhase = PhaseType.Combat_Phase_Combat;
            ExecuteCombat();

            Array.Clear(DeclaredAttackers);
            DeclaredDeffenders.Clear();

            // ewentualne wyłączenie enczantow 
            if (logs)
            {
                Console.WriteLine("Przenoszenie zniszczonych jednostek na cmentarz");
            }
            MoveDeadCreaturesToGraveyard(_player.Opponent);
            MoveDeadCreaturesToGraveyard(_player);
            #endregion

            #region Combat_Phase_End of Combat
            CurrentPhase = PhaseType.Combat_Phase_End;
            if (logs)
            {
                Console.WriteLine(" - End of combat step");
            }
            #endregion
        }
        public void Second_Main_Phase(Player _player)
        {
            #region Second_Main_Phase
            CurrentPhase = PhaseType.Second_Main_Phase;
            if (logs)
            {
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║                           Second_Main_Phase: Player {_player.PlayerNumberID}                          ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
            }
            //Console.WriteLine("Second_Main_Phase: Player "+_player.ID);
            Dictionary<int,(bool result,List<CardBase> landsToTap)> handdata = new();

            if (_player.IsAI)
            {
                _player.AI_MakePlaysWithRandomCardFromHand();
                return;
            }

            while (true)
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

                if (_player.PlayCardFromHand(_player.Hand[choosenIndex]))
                {
                    handdata[choosenIndex].landsToTap.ForEach(land => ((Land)land).isTapped = true);
                }
            }
            #endregion
        }
        public void End_Phase(Player _player)
        {
            #region End_Phase
            CurrentPhase = PhaseType.End_Phase;
            if (logs)
            {
                Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                               End Phase: Player 2                              ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════╝");
            }
            //Console.WriteLine("End_Phase: Player " + _player.ID);
            //Console.WriteLine(" - End of turn step");
            _player.IsLandPlayedThisTurn = false;

            if (logs)
            {
                Console.WriteLine("przywracanie hp ocalałym jednostom");
            }
            RecoverHealthSurvivedCreatures();
            //  _player.DisplayPlayerField();
            if (logs)
            {
                Console.WriteLine(" - Cleanup step");
                Console.WriteLine("Sprawdzanie ilosci kart na ręce przeciwnika) - 7 max inaczej wyrzuć jakąś przed dostaniem kolejnej.");
            }
            HandCardsCleanUpCountChecker(Players[_player.PlayerNumberID == 1 ? 1 : 0]);
            #endregion
        }

      
        private void ExecuteCombat()
        {
            foreach(var attacker in DeclaredAttackers) /* <DEFFENDER, ATTACKER> */
            {
                int attackerMaxHP = attacker.CurrentHealth;
                List<int> defendersHpBeforeCombat = new();
                //Console.WriteLine($"Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                if(DeclaredDeffenders.Any(x=>x.Value == attacker))
                {
                    if(attacker.CurrentHealth <= 0) {
                       // Console.WriteLine("Atakujaca jednostka juz nie zyje, walka pominięta");
                        continue;
                    }
                    foreach(var defender in DeclaredDeffenders.Where(x=>x.Value == attacker))
                    {
                        defendersHpBeforeCombat.Add(attacker.CurrentHealth);
                        // pomin w przypadku gdy deffender juz ni e zyje xd

                        //Console.WriteLine($"Broniąca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");
                        
                        if(defender.Key.CurrentHealth <= 0) continue;

                        attacker.Attack(defender.Key);
                    }

                   // Console.WriteLine("---- wynik potyczki ----");
                   // Console.WriteLine($"{(attacker.CurrentHealth <= 0?"DEAD":(attacker.CurrentHealth < attackerMaxHP)?"DAMAGED":"UNTOUCHED")} Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                  //  int index = 0;
                   // foreach(var defender in DeclaredDeffenders.Where(x=>x.Key == attacker))
                   // {
                    //    Console.WriteLine($"{(defender.Key.CurrentHealth <= 0?"DEAD":(defender.Key.CurrentHealth < defendersHpBeforeCombat[index])?"DAMAGED":"UNTOUCHED")} Broniąca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");
                   //     index++;
                  //  }
                }
                attacker.Attack(_defender:null);

               // Console.WriteLine();
            }
        }
        private void HandCardsCleanUpCountChecker(Player _player)
        {
            if(TurnCounter < 2) return;

            while(_player.Hand.Count >= 7)
            {
                if( _player.IsAI)
                {
                    _player.AI_RemoveOneCardFromHand();
                    continue;
                }

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
                        _player.Graveyard.Add(x);
                        _player.Hand.Remove(x);
                    });
                }
            }
        }
        private void RecoverHealthSurvivedCreatures()
        {
            Players[0].CombatField.ForEach(creature => ((Creature)creature).ResetStatsAfterFight());
        }
        private void CastInstantIfCan(Player _player)
        {
            if (_player.Hand.Any(x => (x is Instant) && ((Instant)x).isAbleToPlay))
            {
                Console.WriteLine($"Szana na kontre uzywajac instantów lub umiejetnosci kart gracz {_player.PlayerNumberID}");
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

                    if (_player.PlayCardFromHand(_player.Hand[choosenIndex]))
                    {
                        Console.WriteLine("zapłąc za karte");
                        handdata[choosenIndex].landsToTap.ForEach(land => ((Land)land).isTapped = true);
                    }
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
                //Console.WriteLine($"send {corpse.Name} to Graveyard...");
                corpse.Owner.Graveyard.Add(corpse);
                corpse.Owner.CombatField.Remove(corpse);    
            }
        }
        private void ShuffleDeck(Player deckOwner)
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
        
        public GameState GetGameState()
        {
            // zapisanie 
            // zapisanie aktualnych stanów graczy 
            // zapisanie numeru tury
            // zapisanie info o ostatniego GamePhase'a
            // zapisanie ostatniego wykonanego ruchu gracza w ostatnim GamePhasie

            GameState state = new();

            state.Player_1 = Players[0];
            state.Player_2 = Players[1];
            state.TurnCounter = this.TurnCounter;
            state.LastGamePhase = this.CurrentPhase;
            state.DeclaredAttackers = this.DeclaredAttackers;
            state.DeclaredDeffenders = this.DeclaredDeffenders;

            return state;
        }

        public void ContinueGameFromState(GameState state)
        {
            Console.WriteLine("kontynuowanie gry");

           // RunGameLoop();

        }
    }
}