using static MTG_ConsoleEngine.Creature;

namespace MTG_ConsoleEngine
{
    public abstract class CardBase 
    {
        public readonly string Identificator; // 128_M21
        public readonly string Name; // Walking Corpse
        public readonly string Description;
        public readonly string CardType; // Creature
        public Dictionary<string,int> ManaCost = new();
        public Player owner;

        public CardBase(Dictionary<string,int> _manaCost, string _identificator, string _name, string _description, string _cardType)
        {
            Identificator = _identificator;
            Name = _name;
            Description = _description;
            CardType = _cardType;
            ManaCost = _manaCost;
        }

        public abstract void UseSpecialAction(ActionType trigger);

    }

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

        internal void AddSpecialAction(string _specialActionInfo)
        {
            // skill 1 
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
                        action: () => Actions.DealDamageToBothPlayers(value, owner)
                    )
                );
            }          
        }

        public Creature(Dictionary<string, int> _manaCost, string _identificator, string _name, string _category, string _description, int _health, int _attack)
            : base(_manaCost, _identificator, _name, _description, "Creature")
        {
            BaseHealth = _health;
            CurrentHealth = BaseHealth;
            BaseAattack = _attack;
            CurrentAttack = BaseAattack;
            Category = _category;
        }

        public override void UseSpecialAction(ActionType actionType)
        {
            if (CardSpecialActions.FirstOrDefault().action != null)
            {
                Console.WriteLine("Aktywowano: " + CardSpecialActions.FirstOrDefault().description);
                CardSpecialActions.FirstOrDefault().action();
                isTapped = true;
            }
            else
            {
                Console.WriteLine("Brak efektu dodatkowego.");
            }
        }

        public void Attack()
        {   
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Action Triggered!");
            if(CardSpecialActions.Count > 0)
            {
                foreach(var actions in CardSpecialActions.Where(x=>x.trigger == ActionType.Attack))
                {   
                    actions.action();
                }
            }
            Console.ResetColor();
            Console.WriteLine($"Player {owner.ID} Atakuje kartą {Name}");
            Engine.Players[owner.ID==1?1:0].DealDamage(CurrentAttack);

        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public enum ActionType{
            Attack,
            Block,
            Draw,
            Die,
            Heal
        }
    }
}

