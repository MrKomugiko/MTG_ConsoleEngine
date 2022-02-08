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
        public Dictionary<int,int> ManaCost = new(6);
        public PlayerBase Owner {get;set;}
        public abstract bool IsTapped { get; set; }
        public bool IsAbleToPlay => CheckAvailability().result;

        public string ManaCostString = "free";

        public EngineBase.TargetType TargetsType { get; protected set; }
        protected CardBase(Dictionary<int,int> _manaCost, string _identificator, 
            string _name, string _description, string _cardType)
        {
            Identificator = _identificator;
            Name = _name;
            Description = _description;
            CardType = _cardType;
            
            foreach(var cost in _manaCost)
            {
                this.ManaCost[cost.Key] = cost.Value;
            }
  
            if(_manaCost.Keys.Count > 0 )
            {
                ManaCostString = "";
                if(_manaCost.ContainsKey(0)) ManaCostString +=  $"{(_manaCost[0] == 0 ? "" : $"{_manaCost[0]},")}";
                if(_manaCost.ContainsKey(1)) ManaCostString += $"{(_manaCost[1] == 0 ? "" : $"B:{_manaCost[1]},")}";
                if(_manaCost.ContainsKey(2)) ManaCostString += $"{(_manaCost[2] == 0 ? "" : $"W:{_manaCost[2]},")}";
                if(_manaCost.ContainsKey(3)) ManaCostString += $"{(_manaCost[3] == 0 ? "" : $"R:{_manaCost[3]},")}";
                if(_manaCost.ContainsKey(4)) ManaCostString += $"{(_manaCost[4] == 0 ? "" : $"G:{_manaCost[4]},")}";
                if(_manaCost.ContainsKey(5)) ManaCostString += $"{(_manaCost[5] == 0 ? "" : $"U:{_manaCost[5]}")}";
            }
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
            Dictionary<int, int> SumManaOwnedAndAvailable = new(6);
            foreach (KeyValuePair<int, int> mana in Owner.CurrentTotalManaStatus)
            {
                SumManaOwnedAndAvailable.Add(mana.Key, mana.Value);
            }

            Dictionary<int, int> creatureManaCostCopy = new(6);
            List<CardBase> landCardsToTap = new();
            var currentLandsCardsCopy = Owner.ManaField.Where(c => c.IsTapped == false);

            foreach (KeyValuePair<int, int> orginalCost in ManaCost)
            {
                creatureManaCostCopy.Add(key: orginalCost.Key, value: orginalCost.Value);
            }
            List<int> corelandCodes = new();
            foreach (KeyValuePair<int, int> cost in ManaCost.Where(x => x.Key > 0))
            {
                corelandCodes.Add(cost.Key);
                if (SumManaOwnedAndAvailable[cost.Key] > 0)
                {
                    if (SumManaOwnedAndAvailable[cost.Key] >= cost.Value)
                    {
                        foreach (Land coreLand in currentLandsCardsCopy.Where(x => ((Land)x).manaValue[cost.Key] > 0))
                        {
                            if (creatureManaCostCopy[cost.Key] == 0) break;
   
                            SumManaOwnedAndAvailable[cost.Key] -= coreLand.manaValue[cost.Key];
                            creatureManaCostCopy[cost.Key] -= coreLand.manaValue[cost.Key];
                            landCardsToTap.Add(coreLand);
                            continue;
                        }
                    }
                    else
                        return (false, new());
                }
            }

            creatureManaCostCopy.TryGetValue(0, out int clearColorValue);

            if (clearColorValue > 0)
            {
                if (SumManaOwnedAndAvailable.Sum(x => x.Value) >= clearColorValue)
                {
                    Dictionary<int, int> creatureCoreMana = creatureManaCostCopy
                        .Where(x => x.Key > 0)
                        .ToDictionary(x => x.Key, x => x.Value);

                    List<CardBase> listaCoreLandow = new();

                    for (int i = 0; i < corelandCodes.Count; i++)
                    {
                        listaCoreLandow.AddRange(currentLandsCardsCopy.Where(land => ((Land)land).manaValue[corelandCodes[i]] > 0 ));
                    }

                    var listaNoCoreLandow = currentLandsCardsCopy.Where(x => corelandCodes.Contains(((Land)x).manaCode) == false);

                    if (listaNoCoreLandow.Sum(x => ((Land)x).manaValue.First(m=>m.Value>0).Value) >= clearColorValue)
                    {
                        foreach (Land noCoreLand in listaNoCoreLandow)
                        {
                            if (creatureManaCostCopy[0] == 0) break;

                            var keyVp = noCoreLand.manaValue.First(m => m.Value > 0);

                            SumManaOwnedAndAvailable[keyVp.Key] -= keyVp.Value;
                            creatureManaCostCopy[0] -= keyVp.Value;
                            landCardsToTap.Add(noCoreLand);
                        }
                    }
                    else
                    {
                        foreach (Land noCoreLand in listaNoCoreLandow)
                        {
                            if (creatureManaCostCopy[0] == 0) break;

                            var keyVp = noCoreLand.manaValue.First(m => m.Value > 0);

                            SumManaOwnedAndAvailable[keyVp.Key] -= keyVp.Value;
                            creatureManaCostCopy[0] -= keyVp.Value;
                            landCardsToTap.Add(noCoreLand);
                        }
                        foreach (Land coreLand in currentLandsCardsCopy.Except(landCardsToTap))
                        {
                            if (creatureManaCostCopy[0] == 0) break;

                            var keyVp = coreLand.manaValue.First(m => m.Value > 0);

                            SumManaOwnedAndAvailable[keyVp.Key] -= keyVp.Value;
                            creatureManaCostCopy[0] -= keyVp.Value;
                            landCardsToTap.Add(coreLand);
                        }
                    }
                }
                else
                    return (false, new());
            }
            

            if (creatureManaCostCopy.Sum(x => x.Value) == 0)
                return (true, landCardsToTap);
            else
                return (false, new());

        }
    }
}

