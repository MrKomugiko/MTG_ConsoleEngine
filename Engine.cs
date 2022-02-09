using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Engine : EngineBase
    {
        public Engine(PlayerBase FirstPlayer, PlayerBase SecondPlayer)
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
        public override void RunGameLoop(ref bool gameEnded)
        {
            int gamephaselenght = GamePhases.Count;
            while (true)
            {
                Console.ResetColor();
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
        public override void Beginning_Phase(PlayerBase _player)
        {
            #region Begining_Phase
            Console.ResetColor();
            CurrentPhase = PhaseType.Beginning_Phase;
            if (logs)
            {
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║    {"[Turn:" + TurnCounter + "]",-11}             Beginning_Phase: Player {_player.PlayerNumberID}                           ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");

                Console.WriteLine(" - Untap Step [ DONE ]");
            }

            _player.CombatField.ForEach(card => card.IsTapped = false);
            _player.ManaField.ForEach(card => card.IsTapped = false);
            _player.RefreshManaStatus(); // reset cached at start new turn
            if (logs)
            {
                Console.WriteLine(" - Upkeep Step [Work In Progress]");
                Console.WriteLine("\t odpalenie akcji wykonujacych sie na początku tury gracza");
                Console.WriteLine(" - Draw step [ DONE ]");
            }

            if (TurnCounter >= 2)
            {
                _player.DrawCard();
            }
            #endregion
        }
        public override void First_Main_Phase(PlayerBase _player)
        {
            #region First_Main_Phase
            Console.ResetColor();
            CurrentPhase = PhaseType.First_Main_Phase;
            if (logs)
            {
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║                            First_Main_Phase: Player {_player.PlayerNumberID}                          ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
            }
            if (_player.IsAI)
            {
                ((PlayerAI)_player).AI_MakePlaysWithRandomCardFromHand();
                ((PlayerAI)_player).DisplayPlayerField();
                return;
            }

            while (true)
            {
                ((Player)_player).RefreshManaStatus();

                Dictionary<int, (bool result, List<CardBase> landsToTap)> handdata = new();
                handdata = ((Player)_player).Get_and_DisplayPlayerHand();
                var input = Console.ReadLine() ?? "";
                if (String.IsNullOrEmpty(input)) break;

                if (Int32.TryParse(input, out int choosenIndex) == false) continue;
                if (choosenIndex >= handdata.Count)
                {
                    Console.WriteLine("niewłasciwy index");
                    continue;
                }

                if (handdata[choosenIndex].result == false)
                {
                    Console.WriteLine("nie mozesz zagrać tej karty i / lub nie stac cie");
                    continue;
                }

                if (_player.PlayCardFromHand(_player.Hand[choosenIndex]))
                {
                    Console.WriteLine("ok");
                }
            }
            #endregion
        }
        public override void Combat_Phase(PlayerBase _player)
        {
            #region Combat_Phase_Begining
            Console.ResetColor();
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
                DeclaredAttackers = ((PlayerAI)_player).AI_RandomAttackDeclaration();
            }
            else
            {
                DeclaredAttackers = ((Player)_player).SelectAttackers_HumanInteraction();
            }
            #endregion

            #region Combat_Phase_DeffendersDeclaration
            CurrentPhase = PhaseType.Combat_Phase_DefendersDeclaration;
            if (DeclaredAttackers.Length > 0)
            {
                //Console.WriteLine(" - Declare Blockers Step - Player "+_player.Opponent.PlayerNumberID);  // drugi gracz
                if (_player.Opponent.IsAI)
                {
                    DeclaredDeffenders = ((PlayerAI)_player.Opponent).AI_RandomDeffendersDeclaration();
                }
                else
                {
                    /* <DEFFENDER, ATTACKER> */
                    DeclaredDeffenders = ((Player)_player.Opponent).SelectDeffenders_HumanInteraction();
                }

            }
            Console.ResetColor();
            #endregion

            #region Combat_Phase_AttackerInstants
            CurrentPhase = PhaseType.Combat_Phase_AttackerInstants;
            if (_player.IsAI)
            {
                ((PlayerAI)_player).AI_PlayRandomInstatFromHand();
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
                ((PlayerAI)_player.Opponent).AI_PlayRandomInstatFromHand();
            }
            else
            {
                CastInstantIfCan(_player.Opponent); // gracz zaczynajacy
            }
            #endregion

            #region Combat_Phase_Combat
            CurrentPhase = PhaseType.Combat_Phase_Combat;
            ExecuteCombat();


            // ewentualne wyłączenie enczantow 
            if (logs)
            {
                Console.WriteLine("Przenoszenie zniszczonych jednostek na cmentarz");
            }
            Array.Clear(DeclaredAttackers);
            DeclaredDeffenders.Clear();
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
        public override void Second_Main_Phase(PlayerBase _player)
        {
            #region Second_Main_Phase
            Console.ResetColor();
            CurrentPhase = PhaseType.Second_Main_Phase;
            if (logs)
            {
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║                           Second_Main_Phase: Player {_player.PlayerNumberID}                          ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
            }
            //Console.WriteLine("Second_Main_Phase: Player "+_player.ID);
            Dictionary<int, (bool result, List<CardBase> landsToTap)> handdata = new();

            if (_player.IsAI)
            {
                ((PlayerAI)_player).AI_MakePlaysWithRandomCardFromHand();
                return;
            }

            while (true)
            {
                handdata = ((Player)_player).Get_and_DisplayPlayerHand();

                var input = Console.ReadLine() ?? "";
                if (String.IsNullOrEmpty(input)) break;

                 
                if (Int32.TryParse(input, out int choosenIndex) == false)
                {

                    continue;
                }
                if (handdata[Int32.Parse(input)].result == false)
                {
                    Console.WriteLine("nie mozesz zagrać tej karty i / lub nie stac cie");
                    continue;
                }

                if (_player.PlayCardFromHand(_player.Hand[choosenIndex]))
                {
                    Console.WriteLine("ok");
                }
            }
            #endregion
        }
        public override void End_Phase(PlayerBase _player)
        {
            #region End_Phase
            Console.ResetColor();
            CurrentPhase = PhaseType.End_Phase;
            if (logs)
            {
                Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║                               End Phase: Player {_player.PlayerNumberID}                              ║");
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


        protected override void ExecuteCombat()
        {
            foreach (var attacker in DeclaredAttackers) /* <DEFFENDER, ATTACKER> */
            {
                int attackerMaxHP = attacker.CurrentHealth;
                List<int> defendersHpBeforeCombat = new();
                Console.WriteLine($"Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                if (DeclaredDeffenders.Any(x => x.Value == attacker))
                {
                    if (attacker.CurrentHealth <= 0)
                    {
                        Console.WriteLine("Atakujaca jednostka juz nie zyje, walka pominięta");
                        continue;
                    }

                    foreach (var defender in DeclaredDeffenders.Where(x => x.Value == attacker))
                    {
                        defendersHpBeforeCombat.Add(attacker.CurrentHealth);
                        // pomin w przypadku gdy deffender juz ni e zyje xd

                        Console.WriteLine($"Broniąca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");

                        if (defender.Key.CurrentHealth <= 0) continue;

                        attacker.Attack(defender.Key);
                    }

                    Console.WriteLine("---- wynik potyczki ----");
                    Console.WriteLine($"{(attacker.CurrentHealth <= 0 ? "DEAD" : (attacker.CurrentHealth < attackerMaxHP) ? "DAMAGED" : "UNTOUCHED")} Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                    int index = 0;
                    foreach (var defender in DeclaredDeffenders.Where(x => x.Value == attacker))
                    {
                        Console.WriteLine($"{(defender.Key.CurrentHealth <= 0 ? "DEAD" : (defender.Key.CurrentHealth < defendersHpBeforeCombat[index]) ? "DAMAGED" : "UNTOUCHED")} Broniąca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");
                        index++;
                    }

                }
                attacker.Attack(_defender: null);

                Console.WriteLine();
            }
        }       
        protected override void HandCardsCleanUpCountChecker(PlayerBase _player)
        {
            if (TurnCounter < 2) return;

            while (_player.Hand.Count >= 7)
            {
                if (_player.IsAI)
                {
                    ((PlayerAI)_player).AI_RemoveOneCardFromHand();
                    continue;
                }

                 ((Player)_player).Get_and_DisplayPlayerHand();
                Console.WriteLine($"Wyrzuć {_player.Hand.Count - 6} kart, podaj indexy 0,1,2..");
                var cardsToRemove = Console.ReadLine();

                if (String.IsNullOrEmpty(cardsToRemove) == false)
                {
                    var removearr = cardsToRemove.Split(",").Select(x => Int32.Parse(x)).ToList();
                    var cardsList = removearr.Select(x => _player.Hand[x]).ToList();
                    cardsList.ForEach(x =>
                    {
                        Console.WriteLine("Wyrzuciłeś " + x.Name);
                        _player.Graveyard.Add(x);
                        _player.Hand.Remove(x);
                    });
                }
            }
        }


        private void CastInstantIfCan(PlayerBase _player)
        {
            if (_player.Hand.Any(x => (x is Instant instant) && instant.IsAbleToPlay))
            {
                Console.WriteLine($"Szana na kontre uzywajac instantów lub umiejetnosci kart gracz {_player.PlayerNumberID}");
                Console.WriteLine("---------------- INSTANTS ONLY ---------------- ");

                Dictionary<int, (bool result, List<CardBase> landsToTap)> handdata = new();

                while (true)
                {

                    handdata = ((Player)_player).Get_and_DisplayPlayerHand();

                    var input = Console.ReadLine() ?? "";
                    if (String.IsNullOrEmpty(input)) break;

                    if (Int32.TryParse(input, out int choosenIndex) == false)
                    {
                        continue;
                    }
                    if (handdata[choosenIndex].result == false)
                    {
                        Console.WriteLine("nie mozesz zagrać tej karty i / lub nie stac cie");
                        continue;
                    }

                    if (_player.Hand[choosenIndex] is Instant == false)
                    {
                        Console.WriteLine("Wybierz kartę typu instant, enter zeby anulowac");
                        continue;
                    }

                    if (_player.PlayCardFromHand(_player.Hand[choosenIndex]))
                    {
                        Console.WriteLine("ok");
                    }
                }
            }
        }
    }
}