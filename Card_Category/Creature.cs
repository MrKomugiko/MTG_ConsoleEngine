namespace MTG_ConsoleEngine.Card_Category
{
    public class Creature : CardBase
    {
        public readonly int BaseHealth;
        public readonly int BaseAattack;
        public readonly string Category;
        public int CurrentHealth { get; private set; }
        public int CurrentAttack { get; private set; }
        public override bool isTapped { 
            get => _isTapped; 
            set {
                _isTapped = value;
                if(value == true)
                    Console.WriteLine($"Karta {Name} została tapnięta.");
            }
        }


        public List<(ActionType trigger, string description, Action action)> CardSpecialActions = new List<(ActionType, string, Action)>();
        private bool _isTapped;
        public Creature(Dictionary<string, int> _manaCost, string _identificator, 
            string _name, string _category, string _description, int _health, int _attack)
            : base(_manaCost, _identificator, _name, _description, "Creature")
        {
            BaseHealth = _health;
            CurrentHealth = BaseHealth;
            BaseAattack = _attack;
            CurrentAttack = BaseAattack;
            Category = _category;
        }
        public override void AddSpecialAction(string _specialActionInfo)
        {
            _specialActionInfo = _specialActionInfo.ToLower();
            if(_specialActionInfo.Contains("whenever") && _specialActionInfo.Contains("attacks, each opponent loses") && _specialActionInfo.Contains("life."))
            {
                int value = Int32.Parse(_specialActionInfo.Replace(this.Name.ToLower(), "")
                        .Replace("whenever  attacks, each opponent loses", "")
                        .Replace("life.", "").Trim());

                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Attack,
                        description: _specialActionInfo,
                        action: () => Actions.DealDamageToBothPlayers(value, Owner)
                    )
                );
            }          
            if(_specialActionInfo.Contains("lifelink"))
            {
                int value = CurrentAttack;
                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Attack,
                        description: "Lifelink",
                        action: () => Actions.Lifelink(value, Owner)
                    )
                );
            }
            if(_specialActionInfo.Contains("haste"))
            {
                int value = CurrentAttack;
                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Play,
                        description: "Haste",
                        action: () => Actions.Haste(this)
                    )
                );
            }
        }
        public override void UseSpecialAction(ActionType actionType)
        {
            if(CardSpecialActions.Count > 0)
            {
                Console.WriteLine("Action/s Triggered!");
                foreach(var actions in CardSpecialActions.Where(x=>x.trigger == actionType))
                {   
                    actions.action();
                }
            }
        }
        public override string GetCardString() {
        {
            return $"| Name:{Name.PadLeft(30)} | Cost:{ManaCostString.PadLeft(10)} | atk:{CurrentAttack.ToString().PadLeft(2)} / hp:{CurrentHealth.ToString().PadLeft(2)} ";
        }
    }
        public void Attack(Creature? defender)
        {   
            if(CurrentHealth <= 0) return;
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            UseSpecialAction(ActionType.Attack);
            Console.ResetColor();
            Console.WriteLine($"Player {Owner.ID} Atakuje kartą {Name}");
            if(defender != null)
            {
                CurrentHealth -= defender.CurrentAttack;
                defender.CurrentHealth -= CurrentAttack;
            }
            else
            {
                Engine.Players[Owner.ID==1?1:0].DealDamage(CurrentAttack);
            }

            isTapped = true;
        }

        internal override (bool result, List<CardBase> landsToTap)  CheckAvailability()
        {
            Dictionary<string,int> SumManaOwnedAndAvailable = new();
            //sumowanie dostepnej many
            foreach(Land manaCard in Owner.ManaField.Where(c=>c is Land && c.isTapped == false ))
            {
                foreach(var mana in manaCard.manaValue)
                {
                    if(SumManaOwnedAndAvailable.ContainsKey(mana.Key))
                    {
                        SumManaOwnedAndAvailable[mana.Key] += mana.Value;
                    } 
                    else
                    {
                        SumManaOwnedAndAvailable.Add(mana.Key,mana.Value);
                    }
                }
            }
            
            Dictionary<string,int> creatureManaCostCopy = new Dictionary<string, int>();
            foreach(var orginalCost in ManaCost)
            {
                creatureManaCostCopy.Add(key:orginalCost.Key, value: orginalCost.Value); 
            }
        
            var sumTotal = SumManaOwnedAndAvailable.Sum(x=>x.Value);

            var currentLandsCardsCopy = Owner.ManaField.Where(c=>c is Land && c.isTapped == false).ToList();

            List<CardBase> landCardsToTap = new List<CardBase>();
            // sprawdzanie pokolei posiadanych AKTYWNYCH kart landow
            foreach(Land manaCard in currentLandsCardsCopy)
            {    
                if(creatureManaCostCopy.Sum(x=>x.Value) == 0) break;
            
                // dopasowywanie kosztu z karty do wartosci z londu
                foreach(KeyValuePair<string,int> cardCost in creatureManaCostCopy)
                {
                    if(cardCost.Value == 0) continue;
                    if(manaCard.manaValue.ContainsKey(cardCost.Key))
                    {
                        // land jest tego samego typu co rodzaj kosztu
                        if(manaCard.manaValue[cardCost.Key] >= cardCost.Value)
                        {
                            // wystarczajaca+ ilosc zasowu, został jeszcze zapas a ten wymagany sie wyzerowal
                            landCardsToTap.Add(manaCard);
                            creatureManaCostCopy[cardCost.Key] = 0;

                        }
                        else if(cardCost.Key != "")
                        {
                            // odejmowanie wszystkiego co mamy, i zostalo jeszcze kosztów = nie stać nas
                            return (false, new());
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
            landCardsToTap.ForEach(x=>currentLandsCardsCopy.Remove(x));

            var leftLandsCount = currentLandsCardsCopy.Count;
            if(creatureManaCostCopy.ContainsKey(""))
            {
                if(creatureManaCostCopy[""] <= creatureManaCostCopy.Sum(x=>x.Value))
                {
                    while(creatureManaCostCopy[""] > 0)
                    {
                        leftLandsCount = currentLandsCardsCopy.Count;
                        if(leftLandsCount == 0)
                        {
                            break;
                        }

                        var card = (Land)currentLandsCardsCopy[rand.Next(0,leftLandsCount)];
                        if(card.manaValue.Sum(x=>x.Value) >= creatureManaCostCopy[""])
                        {
                            landCardsToTap.Add(card);
                            creatureManaCostCopy[""] = 0;
                            currentLandsCardsCopy.Remove(card);
                        }
                        else
                        {
                            // wez wszystco co sie da xd
                            landCardsToTap.Add(card);
                            creatureManaCostCopy[""] -= card.manaValue.Sum(x=>x.Value);
                            currentLandsCardsCopy.Remove(card);
                        }
                    }
                }
            }

            // na koniec liczymy czy potrzebny koszt == 0;
            if(creatureManaCostCopy.Sum(x=>x.Value) == 0)
            {
                foreach(var land in landCardsToTap)
                {
                    Console.WriteLine("lands needs to tap for sake summon this creature:"+land.Name);
                }
                return (true, landCardsToTap);
            }
            return (false, new());
        }
    }
}

