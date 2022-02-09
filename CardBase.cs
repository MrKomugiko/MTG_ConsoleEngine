using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public abstract class CardBase 
    {
        public int ID;

        public List<(ActionType trigger, string description, Action action)> CardSpecialActions = new();

        public readonly string Identificator; // 128_M21
        public readonly string Name; // Walking Corpse
        public readonly string Description;
        public readonly string CardType; // Creature
        public int[] ManaCost = new int[6];
        public PlayerBase Owner {get;set;}
        public abstract bool IsTapped { get; set; }
        public bool IsAbleToPlay => Owner.SimpleManaCheck(ManaCost);

        public string ManaCostString = "free";

        public EngineBase.TargetType TargetsType { get; protected set; }
        protected CardBase(int[] _manaCost, string _identificator, 
            string _name, string _description, string _cardType)
        {
            Identificator = _identificator;
            Name = _name;
            Description = _description;
            CardType = _cardType;
            ManaCost = _manaCost;

            ManaCostString = String.Join("|", _manaCost);
        }

        public abstract void UseSpecialAction(ActionType trigger);
        public abstract void AddSpecialAction(string _specialActionInfo);
        public virtual string GetCardString()
        {
            return $"{Name,21} ║ {ManaCostString.Trim(),10}";
        }
        public object Clone()
        {
            //TODO: sprawdzic czy napewno dziala jak powinno 
            return this.MemberwiseClone();
        }

        public virtual (bool result, List<CardBase> landsToTap) CheckAvailability()
        {

            return (false, new());
        }
    }
}

