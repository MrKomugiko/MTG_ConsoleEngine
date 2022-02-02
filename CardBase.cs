using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public abstract class CardBase 
    {
        public List<(ActionType trigger, string description, Action action)> CardSpecialActions = new List<(ActionType, string, Action)>();

        public readonly string Identificator; // 128_M21
        public readonly string Name; // Walking Corpse
        public readonly string Description;
        public readonly string CardType; // Creature
        public Dictionary<string,int> ManaCost = new();
        public Player Owner {get;set;}
        public abstract bool isTapped { get; set; }
        public bool isAbleToPlay => CheckAvailability().result;

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
        public virtual string GetCardString()
        {
            return $"{Name.PadLeft(21)} ║ {ManaCostString.Trim().PadLeft(10)}";
        }
        public object Clone()
        {
            //TODO: sprawdzic czy napewno dziala jak powinno 
            return this.MemberwiseClone();
        }

        public virtual (bool result, List<CardBase> landsToTap) CheckAvailability()
        {
            Dictionary<string, int> SumManaOwnedAndAvailable = Owner.SumAvailableManaFromManaField();
            Dictionary<string, int> creatureManaCostCopy = new Dictionary<string, int>();
            List<CardBase> landCardsToTap = new List<CardBase>();
            var sumTotal = SumManaOwnedAndAvailable.Sum(x => x.Value);
            var currentLandsCardsCopy = Owner.ManaField.Where(c => c is Land && c.isTapped == false).ToList();

            foreach (var orginalCost in ManaCost)
            {
                creatureManaCostCopy.Add(key: orginalCost.Key, value: orginalCost.Value);
            }

            foreach (var cost in ManaCost.Where(x=>x.Key != ""))
            {
                if (SumManaOwnedAndAvailable.ContainsKey(cost.Key))
                {
                    if(SumManaOwnedAndAvailable[cost.Key] >= cost.Value)
                    {
                        foreach(Land coreLand in currentLandsCardsCopy.Where(x=>((Land)x).manaValue.ContainsKey(cost.Key)))
                        {
                            if (creatureManaCostCopy[cost.Key] == 0) break;
                            if(creatureManaCostCopy[cost.Key] > 0)
                            {
                                SumManaOwnedAndAvailable[cost.Key] -= coreLand.manaValue[cost.Key];
                                creatureManaCostCopy[cost.Key] -= coreLand.manaValue[cost.Key];
                                landCardsToTap.Add(coreLand);
                                continue;
                            }
                        }
                    }
                    else
                        return (false, new());
                }
            }

            int clearColorValue;
            creatureManaCostCopy.TryGetValue("", out clearColorValue);

            if(creatureManaCostCopy.ContainsKey(""))
            {
                if(clearColorValue > 0)
                {
                    if(SumManaOwnedAndAvailable.Sum(x=>x.Value) >= clearColorValue)
                    {
                        Dictionary<string, int> creatureCoreMana = creatureManaCostCopy
                            .Where(x => x.Key != "")
                            .ToDictionary(x => x.Key, x => x.Value);
                    
                        var listaNoCoreLandow = currentLandsCardsCopy.Where(x=> ((Land)x).manaValue.Any(x=>creatureCoreMana.ContainsKey(x.Key) == false));
                        if (listaNoCoreLandow.Sum(x => ((Land)x).manaValue.First().Value) >= clearColorValue)
                        {
                            foreach (Land noCoreLand in listaNoCoreLandow)
                            {
                                if (creatureManaCostCopy[""] == 0) break;

                                SumManaOwnedAndAvailable[noCoreLand.manaValue.First().Key] -= noCoreLand.manaValue.First().Value;
                                creatureManaCostCopy[""] -= noCoreLand.manaValue.First().Value;
                                landCardsToTap.Add(noCoreLand);
                            }
                        }
                        else
                        {
                            foreach (Land noCoreLand in listaNoCoreLandow)
                            {
                                if (creatureManaCostCopy[""] == 0) break;

                                SumManaOwnedAndAvailable[noCoreLand.manaValue.First().Key] -= noCoreLand.manaValue.First().Value;
                                creatureManaCostCopy[""] -= noCoreLand.manaValue.First().Value;
                                landCardsToTap.Add(noCoreLand);
                            }
                            foreach (Land coreLand in currentLandsCardsCopy.Except(landCardsToTap))
                            {
                                if (creatureManaCostCopy[""] == 0) break;
                                SumManaOwnedAndAvailable[coreLand.manaValue.First().Key] -= coreLand.manaValue.First().Value;
                                creatureManaCostCopy[""] -= coreLand.manaValue.First().Value;
                                landCardsToTap.Add(coreLand);
                            }
                        }
                    }
                    else
                        return (false, new());
                }
            }

            if (creatureManaCostCopy.Sum(x => x.Value) == 0)
                return (true, landCardsToTap);
            else
                return (false, new());
        }
    }
}

