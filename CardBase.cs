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
        public abstract bool isTapped { get; set; }

        public string ManaCostString = "free";

        protected CardBase(Dictionary<string,int> _manaCost, string _identificator, 
            string _name, string _description, string _cardType)
        {
            Identificator = _identificator;
            Name = _name;
            Description = _description;
            CardType = _cardType;
            ManaCost = _manaCost;
  
            if(_manaCost.Keys.Count != 0 )
            {
                ManaCostString = "";
                foreach(KeyValuePair<string,int> mana in _manaCost )
                {
                    ManaCostString += $"{mana.Key}{mana.Value} ";
                }          
            }
        }

        public abstract void UseSpecialAction(ActionType trigger);
        public abstract void AddSpecialAction(string _specialActionInfo);
        public abstract string GetCardString();

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        internal abstract (bool result, List<CardBase> landsToTap) CheckAvailability();
    }
}

