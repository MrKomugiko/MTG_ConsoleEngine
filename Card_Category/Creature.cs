namespace MTG_ConsoleEngine.Card_Category
{
    public class Creature : CardBase
    {
        public readonly int BaseHealth;
        public readonly int BaseAattack;
        public readonly string Category;
        public int CurrentHealth { get; private set; }
        public int CurrentAttack { get; private set; }
        public bool isTapped { 
            get => _isTapped; 
            set {
                _isTapped = value;
                Console.WriteLine($"Karta {Name} została tapnięta.");
            }
        }


        List<(ActionType trigger, string description, Action action)> CardSpecialActions = new List<(ActionType, string, Action)>();
        private bool _isTapped;
        internal string getmanastring { get {
            string manaCostFormated = "";
            foreach(KeyValuePair<string,int> mana in ManaCost )
            {
                manaCostFormated += $"{mana.Key}{mana.Value} ";
            }            
            return manaCostFormated;
        }}
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
            if(_specialActionInfo.Contains("Whenever") && _specialActionInfo.Contains("attacks, each opponent loses") && _specialActionInfo.Contains("life."))
            {
                int value = Int32.Parse(_specialActionInfo.Replace(this.Name, "")
                        .Replace("Whenever  attacks, each opponent loses", "")
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
        }
        public override void UseSpecialAction(ActionType actionType)
        {
            if(CardSpecialActions.Count > 0)
            {
                Console.WriteLine("Action/s Triggered!");
                foreach(var actions in CardSpecialActions.Where(x=>x.trigger == ActionType.Attack))
                {   
                    actions.action();
                }
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
    }
}

