using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public abstract class EngineBase
    {
        protected delegate void Phase(PlayerBase _player);

        public readonly PlayerBase[] Players = new PlayerBase[2];
        public Creature[] DeclaredAttackers;
        public Dictionary<Creature, Creature> DeclaredDeffenders = new(); // <obrońca / atakujacy> , bo mozna blokowac kilkoma stworkami
        public int CurrentPlayerIndex = 0;
        public bool logs = true;
        protected List<Phase> GamePhases = new();
        public int TurnCounter = 0;
        public PhaseType CurrentPhase { get; set; }

        public Creature[] GetAttackerDeclaration() => DeclaredAttackers;
        public Dictionary<Creature, Creature> GetDeffendersDeclaration() => DeclaredDeffenders;

        public abstract void Start();
        public abstract void RunGameLoop(ref bool gameEnded);
        public abstract void Beginning_Phase(PlayerBase _player);
        public abstract void First_Main_Phase(PlayerBase _player);
        public abstract void Combat_Phase(PlayerBase _player);
        public abstract void Second_Main_Phase(PlayerBase _player);
        public abstract void End_Phase(PlayerBase _player);
       
        public abstract List<CardBase> GetValidTargetsForCardType(CardBase card, int PlayerID = 0);

        protected abstract void ExecuteCombat();
        protected abstract void HandCardsCleanUpCountChecker(PlayerBase _player);
        protected void RecoverHealthSurvivedCreatures()
        {
            Players[0].CombatField.ForEach(creature => ((Creature)creature).ResetStatsAfterFight());
        }
       // protected abstract void CastInstantIfCan(Player _player);
        protected abstract void MoveDeadCreaturesToGraveyard(PlayerBase _player);
        protected abstract void ShuffleDeck(PlayerBase deckOwner);
 
        public enum TargetType
        {
            Ally,
            Enemy,
            Both,
            None
        }
    }
}