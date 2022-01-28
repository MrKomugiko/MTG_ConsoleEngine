using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public abstract class CardBase 
    {
        public readonly string Identificator; // 128_M21
        public readonly string Name; // Walking Corpse
        public readonly string Description;
        public readonly string CardType; // Creature
        public Dictionary<string,int> ManaCost = new();
        public Player Owner {get;set;} = new();

        protected CardBase(Dictionary<string,int> _manaCost, string _identificator, 
            string _name, string _description, string _cardType)
        {
            Identificator = _identificator;
            Name = _name;
            Description = _description;
            CardType = _cardType;
            ManaCost = _manaCost;
        }

        public abstract void UseSpecialAction(ActionType trigger);
        public abstract void AddSpecialAction(string _specialActionInfo);

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

