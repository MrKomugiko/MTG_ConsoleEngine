namespace MTG_ConsoleEngine.Card_Category
{      
    public class Land : CardBase
    {
        public override bool isTapped { 
            get => _isTapped; 
            set {
                _isTapped = value;
                 //if(value == true)
                 //   Console.WriteLine($"Land {this.Name} została tapnięta.");
            }
        }
        private bool _isTapped;
        public Dictionary<string, int> manaValue = new();
      
        public Land(string _identificator, string _name, string _manaCode, int _value = 1) 
            : base(new(), _identificator, _name,"", "Land")
        {
            // TODO: zakłądajac ze land nie da nigdy 2 takich samych znakow many, inaczej trzeba je zsumowac
            manaValue.Add(_manaCode,_value);
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
                        action: () => Actions.PlayLandCard(Int32.Parse(value[0]), manaCode, this.Owner)
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
            return $"{this.GetType().Name.PadLeft(12)} ║ {base.GetCardString()} ║ {"".PadLeft(13)}";
        }
        public override (bool result, List<CardBase> landsToTap) CheckAvailability()
        {
            if(Owner.IsLandPlayedThisTurn)
                return (false, new());

             return (true, new());
        }
    }
}
