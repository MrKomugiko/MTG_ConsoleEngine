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
        public Player Owner {get;set;} = new();
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
            return this.MemberwiseClone();
        }

        internal virtual (bool result, List<CardBase> landsToTap) CheckAvailability()
        {
            Dictionary<string, int> SumManaOwnedAndAvailable = Owner.SumAvailableManaFromManaField();

            Dictionary<string, int> creatureManaCostCopy = new Dictionary<string, int>();
            
            foreach (var orginalCost in ManaCost)
            {
                creatureManaCostCopy.Add(key: orginalCost.Key, value: orginalCost.Value);
            }

            var sumTotal = SumManaOwnedAndAvailable.Sum(x => x.Value);

            var currentLandsCardsCopy = Owner.ManaField.Where(c => c is Land && c.isTapped == false).ToList();

            List<CardBase> landCardsToTap = new List<CardBase>();
            // sprawdzanie pokolei posiadanych AKTYWNYCH kart landow
            foreach (Land manaCard in currentLandsCardsCopy)
            {
                if (creatureManaCostCopy.Sum(x => x.Value) == 0) break;

                // dopasowywanie kosztu z karty do wartosci z londu
                foreach (KeyValuePair<string, int> cardCost in creatureManaCostCopy)
                {
                    if (cardCost.Value == 0) continue;
                    if (manaCard.manaValue.ContainsKey(cardCost.Key))
                    {
                        // land jest tego samego typu co rodzaj kosztu
                        if (manaCard.manaValue[cardCost.Key] >= cardCost.Value)
                        {
                            // karta landu ma wiekszą wartosc many niz wymanagany koszt
                            // wystarczajaca+ ilosc zasowu, został jeszcze zapas a ten wymagany sie wyzerowal
                            landCardsToTap.Add(manaCard);
                            creatureManaCostCopy[cardCost.Key] = 0;

                        }
                        else
                        {
                            // odejmowanie od many bez koloru, bierzem pokolei wszystko co jest na liscie
                            landCardsToTap.Add(manaCard);
                            creatureManaCostCopy[cardCost.Key] -= manaCard.manaValue[cardCost.Key];
                            // sprawdzenie czy to wsio
                        }
                    }
                }
            }

            var rand = new Random();
            // deal with no color cost value, random pick cards
            landCardsToTap.ForEach(x => currentLandsCardsCopy.Remove(x));

            var leftLandsCount = currentLandsCardsCopy.Count;
            if (creatureManaCostCopy.ContainsKey(""))
            {
                if (creatureManaCostCopy[""] <= creatureManaCostCopy.Sum(x => x.Value))
                {
                    while (creatureManaCostCopy[""] > 0)
                    {
                        leftLandsCount = currentLandsCardsCopy.Count;
                        if (leftLandsCount == 0)
                        {
                            break;
                        }

                        var card = (Land)currentLandsCardsCopy[rand.Next(0, leftLandsCount)];
                        if (card.manaValue.Sum(x => x.Value) >= creatureManaCostCopy[""])
                        {
                            landCardsToTap.Add(card);
                            creatureManaCostCopy[""] = 0;
                            currentLandsCardsCopy.Remove(card);
                        }
                        else
                        {
                            // wez wszystco co sie da xd
                            landCardsToTap.Add(card);
                            creatureManaCostCopy[""] -= card.manaValue.Sum(x => x.Value);
                            currentLandsCardsCopy.Remove(card);
                        }
                    }
                }
            }

            // na koniec liczymy czy potrzebny koszt == 0;
            if (creatureManaCostCopy.Sum(x => x.Value) == 0)
            {
                return (true, landCardsToTap);
            }
            return (false, new());
        }
    }
}

