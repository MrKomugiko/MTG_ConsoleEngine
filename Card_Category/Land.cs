namespace MTG_ConsoleEngine.Card_Category
{      
    public class Land : CardBase
    {
        public override bool IsTapped { 
            get => _isTapped; 
            set {
                _isTapped = value;
                 //if(value == true)
                 //   Console.WriteLine($"Land {this.Name} została tapnięta.");
            }
        }

        private bool _isTapped;
        /*

            0   ->   no color
            1   ->   Black
            2   ->   White
            3   ->   Red
            4   ->   Green
            5   ->   Blue

         */
        public (int codeIndex, int value) manaValue = new();

        public Land(string _identificator, string _name) 
            : base(new int[6], _identificator, _name,"", "Land")
        {
            // create land card
            base.TargetsType = EngineBase.TargetType.None;
        }
        public override void AddSpecialAction(string _specialActionInfo)
        {
            if(_specialActionInfo.Contains("Add") && _specialActionInfo.Contains("mana"))
            {
                // assign value to this land card
                var value = _specialActionInfo.Replace("Add", "").Replace("mana", "").Trim().Split(" ");
                switch(value[1])
                {
                    default:        manaValue = (0, 1); break;
                    case "Black":   manaValue = (1, Int32.Parse(value[0])); break;
                    case "White":   manaValue = (2, Int32.Parse(value[0])); break;
                    case "Red":     manaValue = (3, Int32.Parse(value[0])); break;
                    case "Green":   manaValue = (4, Int32.Parse(value[0])); break;
                    case "Blue":    manaValue = (5, Int32.Parse(value[0])); break;
                };

                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Play,
                        description: _specialActionInfo,
                        action: () => Actions.PlayLandCard(this.Owner)
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
               // Console.WriteLine("Aktywowano: " + CardSpecialActions.FirstOrDefault().description);
                CardSpecialActions.FirstOrDefault().action();
            }
        }
        public override string GetCardString() 
        {
            return $"{this.GetType().Name,12} ║ {base.GetCardString()} ║ {"",13}";
        }
        public override (bool result, List<CardBase> landsToTap) CheckAvailability()
        {
            if(Owner.IsLandPlayedThisTurn)
                return (false, new());

             return (true, new());
        }
    }
}
