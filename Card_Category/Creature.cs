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
        public override void Print() {
        {
            Console.WriteLine($"Name:{Name.PadLeft(30)} | Cost:{ManaCostString.Trim().PadLeft(10)} | atk:{CurrentAttack.ToString().PadLeft(2)} / hp:{CurrentHealth.ToString().PadLeft(2)}");
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

