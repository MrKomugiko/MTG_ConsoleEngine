using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public partial class Engine
    {
        private delegate void Phase(Player _player);
        private List<Phase> GamePhases = new List<Phase>();
        public static Player[] Players = new Player[2];
        public static PhaseType CurrentPhase { get; private set; }
        public static List<Creature> DeclaredAttackers = new();
        public static Dictionary<Creature,Creature> DeclaredDeffenders = new(); // <obrońca / atakujacy> , bo mozna blokowac kilkoma stworkami
        public static int TurnCounter = 0;
        public static int CurrentPlayerIndex = 0;
        public Engine(Player FirstPlayer, Player SecondPlayer)
        {
            GamePhases.Add(Beginning_Phase);
            GamePhases.Add(First_Main_Phase);
            GamePhases.Add(Combat_Phase);
            GamePhases.Add(Second_Main_Phase);
            GamePhases.Add(End_Phase);

            Players[0] = FirstPlayer;
            Players[1] = SecondPlayer;
        }
        public void Start()
        {
            Console.WriteLine("Start Game Loop");
            Console.WriteLine("Draw 7 card Hand");
            
            for(int i = 0;i<7;i++)
               Players[0].DrawCard();
               

            for(int i = 0;i<7;i++)
               Players[1].DrawCard();
               

            TurnCounter = 0;
            CurrentPlayerIndex = 0;
            while(true)
            {
                Players[0].DisplayPlayerHand();
                Players[1].DisplayPlayerHand();

                foreach(var Phase in GamePhases)
                {
                    Phase(Players[CurrentPlayerIndex]);
                }

                CurrentPlayerIndex = CurrentPlayerIndex==0?1:0;
                TurnCounter++;
            };
        }
        public void Beginning_Phase(Player _player)
        {   
            CurrentPhase = PhaseType.Beginning_Phase;
            Console.WriteLine("Beginning_Phase: Player "+_player.ID);
            Console.WriteLine(" - Untap Step [ DONE ]");
            _player.CombatField.ForEach(card => card.isTapped = false);
            _player.ManaField.ForEach(card => card.isTapped = false);

            Console.WriteLine(" - Upkeep Step [Work In Progress]");
            Console.WriteLine("\t odpalenie akcji wykonujacych sie na początku tury gracza");
            Console.WriteLine(" - Draw step [ DONE ]");
            if(TurnCounter >= 2)
            {
                _player.DrawCard();
            }
        }
        public void First_Main_Phase(Player _player)
        {
            CurrentPhase = PhaseType.First_Main_Phase;
            Console.WriteLine("First_Main_Phase: Player "+_player.ID);
            Console.WriteLine("\t Cast spells, instants, play enchantments, play creatures");
            _player.DisplayPlayerHand();
            Console.WriteLine("enter card index to play with / type 'skip' to skip ");
            while(true)
            {
                var input = Console.ReadLine()??"";
                
                if(String.IsNullOrEmpty(input)) break;

                _player.PlayCardFromHand(_player.Hand[Int32.Parse(input)]);
                _player.DisplayPlayerHand();
            }
        }
        public void Combat_Phase(Player _player)
        {
            CurrentPhase = PhaseType.Combat_Phase;
            Console.WriteLine("Combat_Phase: Player "+_player.ID);
            Console.WriteLine(" - Beginning of Combat Step");
            Console.WriteLine(" - Declare Attackers"); // playerID
            DeclaredAttackers = _player.SelectAttackers();

            Console.WriteLine(" - Declare Blockers Step");  // drugi gracz
            if(DeclaredAttackers.Count > 0)
            {
                DeclaredDeffenders = Players[_player.ID==1?1:0].SelectDeffenders();
            }
            Console.ResetColor();
            Console.WriteLine($"Szana na kontre uzywajac instantów lub umiejetnosci kart gracz {_player.ID}");
            Console.WriteLine($"Szana na kontre uzywajac instantów lub umiejetnosci kart gracz {Players[_player.ID==1?1:0].ID}");
            Console.WriteLine(" - Combat Damage Step");
            
            ExecuteCombat();
            DeclaredAttackers.Clear();
            DeclaredDeffenders.Clear();

            Console.WriteLine(" - End of Combat Step");
        }

        private void ExecuteCombat()
        {
            foreach(var attacker in DeclaredAttackers)
            {
                int attackerMaxHP = attacker.CurrentHealth;
                List<int> defendersHpBeforeCombat = new();
                Console.WriteLine($"Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                if(DeclaredDeffenders.Any(x=>x.Value == attacker))
                {
                    foreach(var defender in DeclaredDeffenders.Where(x=>x.Value == attacker))
                    {
                        // if(attacker.CurrentHealth <=0) 
                        //     break;

                        defendersHpBeforeCombat.Add(attacker.CurrentHealth);
                        Console.WriteLine($"Broniąca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");

                        attacker.Attack(defender.Key);
                    }
                    Console.WriteLine("---- wynik potyczki ----");

                    Console.WriteLine($"{(attacker.CurrentHealth <= 0?"DEAD":(attacker.CurrentHealth < attackerMaxHP)?"DAMAGED":"UNTOUCHED")} Atakująca kreatura {attacker.Name} [ATK:{attacker.CurrentAttack} HP:{attacker.CurrentHealth}]");
                    int index = 0;
                    foreach(var defender in DeclaredDeffenders.Where(x=>x.Value == attacker))
                    {
                        Console.WriteLine($"{(defender.Key.CurrentHealth <= 0?"DEAD":(defender.Key.CurrentHealth < defendersHpBeforeCombat[index])?"DAMAGED":"UNTOUCHED")} Atakująca kreatura {defender.Key.Name} [ATK:{defender.Key.CurrentAttack} HP:{defender.Key.CurrentHealth}]");
                        index++;
                    }
                }
                attacker.Attack(defender:null);

                Console.WriteLine();
            }
        }

        public void Second_Main_Phase(Player _player)
        {
            CurrentPhase = PhaseType.Second_Main_Phase;
            Console.WriteLine("Second_Main_Phase: Player "+_player.ID);
        }
        public void End_Phase(Player _player)
        {
            CurrentPhase = PhaseType.End_Phase;
            Console.WriteLine("End_Phase: Player " + _player.ID);
            Console.WriteLine(" - End of turn step");
            Console.WriteLine(" - Cleanup step");
            _player.IsLandPlayedThisTurn = false;
            MoveDeadCreaturesToGraveyard(_player);
            MoveDeadCreaturesToGraveyard(Players[_player.ID==1?0:1]);
        }

        private static void MoveDeadCreaturesToGraveyard(Player _player)
        {
            var DeadCreatures = _player.CombatField.Where(c => ((Creature)c).CurrentHealth <= 0);
            _player.Graveyard.AddRange(DeadCreatures);
            foreach (var corpse in DeadCreatures)
            {
                _player.CombatField.Remove(corpse);
            }
        }
    }
}