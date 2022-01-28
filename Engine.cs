namespace MTG_ConsoleEngine
{
    public class Engine
    {
        private delegate void Phase(Player _player);
        private List<Phase> GamePhases = new List<Phase>();
        public static Player[] Players = new Player[2];
        public PhaseType CurrentPhase { get; private set; }

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
            Console.WriteLine("Draw Hand");

            int currentPlayerId = 0;
            while(true)
            {
                foreach(var Phase in GamePhases)
                {
                    Phase(Players[currentPlayerId]);
                }
                currentPlayerId = currentPlayerId==0?1:0;
                Console.ReadLine();
            }

            Console.WriteLine("End Game Loop");
        }

        public void Beginning_Phase(Player _player)
        {   
            CurrentPhase = PhaseType.Beginning_Phase;
            Console.WriteLine("Beginning_Phase: Player "+_player.ID);
            Console.WriteLine(" - Untap Step");
            Console.WriteLine(" - Upkeep Step");
            Console.WriteLine(" - Draw step");
        }
        public void First_Main_Phase(Player _player)
        {
            CurrentPhase = PhaseType.First_Main_Phase;
            Console.WriteLine("First_Main_Phase: Player "+_player.ID);
        }
        public void Combat_Phase(Player _player)
        {
            CurrentPhase = PhaseType.Combat_Phase;
            Console.WriteLine("Combat_Phase: Player "+_player.ID);
            Console.WriteLine(" - Beginning of Combat Step");
            Console.WriteLine(" - Declare Attackers"); // playerID
            Console.WriteLine(" - Declare Blockers Step");  // drugi gracz
            Console.WriteLine(" - Combat Damage Step");
            Console.WriteLine(" - End of Combat Step");
        }
        public void Second_Main_Phase(Player _player)
        {
            CurrentPhase = PhaseType.Second_Main_Phase;
            Console.WriteLine("Second_Main_Phase: Player "+_player.ID);
        }
        public void End_Phase(Player _player)
        {
            CurrentPhase = PhaseType.End_Phase;
            Console.WriteLine("End_Phase: Player "+_player.ID);
            Console.WriteLine(" - End of turn step");
            Console.WriteLine(" - Cleanup step");
        }
        public enum PhaseType {
            Start = 0,
            Beginning_Phase = 1,
            First_Main_Phase = 2,
            Combat_Phase = 3,
            Second_Main_Phase = 4,
            End_Phase = 5
        }
    }

    public class Actions{
        public static void DealDamageToBothPlayers(int value, Player cardOwner){
            int firstTarget = cardOwner.ID == 1?1:0; 

            Engine.Players[firstTarget].DealDamage(value);
            cardOwner.DealDamage(value);
        }
    }
}