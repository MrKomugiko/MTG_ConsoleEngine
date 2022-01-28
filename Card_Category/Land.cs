namespace MTG_ConsoleEngine.Card_Category
{      
    public class Land : CardBase
    {
        public bool isTapped { 
            get => _isTapped; 
            set {
                Console.WriteLine($"Karta {Name} została tapnięta.");
                _isTapped = value;
            }
        }
        List<(ActionType trigger, string description, Action action)> CardSpecialActions = new List<(ActionType, string, Action)>();
        private bool _isTapped;
      
        public Land(string _identificator, string _name) 
            : base(new(), _identificator, _name,"", "Creature")
        {

        }

        public override void AddSpecialAction(string _specialActionInfo)
        {
            // skill 1 
            if(_specialActionInfo.Contains("Add") && _specialActionInfo.Contains("mana"))
            {
                var value = _specialActionInfo.Replace("Add", "").Replace("mana", "").Trim().Split(" ");
                string manaCode= "";
                switch(value[1])
                {
                    case "Black":   manaCode = "B"; break;
                    case "White":   manaCode = "W"; break;
                    case "Green":   manaCode = "G"; break;
                    case "Red":     manaCode = "R"; break;
                    case "Blue":    manaCode = "U"; break;
                };

                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Play,
                        description: _specialActionInfo,
                        action: () => Actions.AddMana(Int32.Parse(value[0]), manaCode, Owner)
                    )
                );
            }          
        }
        public override void UseSpecialAction(ActionType trigger)
        {
            if (CardSpecialActions.Count == 0) return;

            var specialAction = CardSpecialActions.Where(x=>x.trigger == trigger);
            if(specialAction == null) return;

            foreach(var action in specialAction)
            {
                Console.WriteLine("Aktywowano: " + CardSpecialActions.FirstOrDefault().description);
                CardSpecialActions.FirstOrDefault().action();
            }
        }

        public override void Print() 
        {
            Console.WriteLine($"Name:{Name.PadLeft(30)} | Cost:{ManaCostString.PadLeft(10)} | ");
        }
    }
}
