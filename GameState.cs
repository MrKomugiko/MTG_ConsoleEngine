using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class GameState
    {
        public Player Player_1 { get; internal set; }
        public Player Player_2 { get; internal set; }
        public int TurnCounter { get; internal set; }
        public Engine.PhaseType LastGamePhase { get; internal set; }
        public Creature[] DeclaredAttackers { get; internal set; }
        public Dictionary<Creature, Creature> DeclaredDeffenders { get; internal set; }

        
    }
}