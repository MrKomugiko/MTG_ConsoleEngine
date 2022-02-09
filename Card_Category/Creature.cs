namespace MTG_ConsoleEngine.Card_Category
{
    public class Creature : CardBase
    {
        private readonly int CoreHealth; // orginalnie, bez zanych boostów
        private readonly int CoreAttack; // orginalnie, bez zanych boostów
        private bool isDead = false;
        public int BaseHealth { 
            get => _baseHealth; 
            set {
                // difference with old and new health
                int difference = value - _baseHealth;   // np nowe zycie 3 stare 1 , roznica 2.
                CurrentHealth += difference;            // zaktualizowanie aktualnego żyćka w zaleznosci od modyfikazji bazy 

                _baseHealth = value>=0?value:0;         // życie nie moze byc mniejsze niz 0
            }
        }
        public int BaseAttack { 
            get => _baseAttack; 
            set {
                int difference = value - _baseAttack;   // np atak nowy 3 stary 1 , roznica 2.
                CurrentAttack += difference;            // zaktualizowanie aktualnego dmg w zaleznosci od modyfikazji bazy 

                _baseAttack = value>=0?value:0;
            }
         }
        public int CurrentHealth { 
            get => _currentHealth; 
            set {
                if(value <= 0 && isDead == false)
                {
                    isDead = true;
                    _currentHealth = 0;

                   ///* DEBUG INFO */ Console.WriteLine($"\t\tCreature: {Name} died... [owner: player {Owner.PlayerNumberID}]");

                    if (EnchantmentSlots.Count > 0)
                    {
                        
                        EnchantmentSlots.ForEach(ench=>
                        {
                            ench.UseSpecialAction(ActionType.CreatureDied);
                        });
                        
                        EnchantmentSlots.ForEach(ench=>Owner.Graveyard.Add(ench));
                        EnchantmentSlots.Clear();
                    }                
                }
                else
                {
                    _currentHealth = value;
                }
            } 
        }
        public readonly string Category;
        public int CurrentAttack;
        public override bool IsTapped
        {
            get => _isTapped;
            set
            {
                _isTapped = value;
            }
        }
        public List<string> Perks { get; internal set; } = new List<string>();
        private int _currentHealth;
        public List<Enchantment> EnchantmentSlots = new();
        private bool _isTapped;
        private int _baseHealth;
        private int _baseAttack;
        public Creature(int[] _manaCost, string _identificator,string _name, string _category, string _description, int _health, int _attack)
            : base(_manaCost, _identificator, _name, _description, "Creature")
        {
            this.BaseHealth = _health;
            this.CurrentHealth = BaseHealth;
            this.BaseAttack = _attack;
            this.CurrentAttack = BaseAttack;
            this.Category = _category;

            this.CoreHealth = _health;
            this.CoreAttack = _attack;
            base.TargetsType = EngineBase.TargetType.Enemy;
        }
        public override void AddSpecialAction(string _specialActionInfo)
        {
            _specialActionInfo = _specialActionInfo.ToLower();
            if (_specialActionInfo.Contains("whenever") && _specialActionInfo.Contains("attacks, each opponent loses") && _specialActionInfo.Contains("life."))
            {
                int value = Int32.Parse(_specialActionInfo.Replace(this.Name.ToLower(), "")
                        .Replace("whenever  attacks, each opponent loses", "")
                        .Replace("life.", "").Trim());

                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Attack,
                        description: _specialActionInfo,
                        action: () => Actions.DealDamageToBothPlayers(value, this.Owner)
                    )
                );
            }
            if (_specialActionInfo.Contains("lifelink"))
            {
                Perks.Add("Lifelink");
                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Attack,
                        description: "Lifelink",
                        action: () => Actions.Heal(this.CurrentAttack, this.Owner)
                    )
                );
                CardSpecialActions.Add
                (
                    (
                        trigger: ActionType.Block,
                        description: "Lifelink",
                        action: () => Actions.Heal(this.CurrentAttack, this.Owner)
                    )
                );
            }
            if (_specialActionInfo.Contains("haste"))
            {
                Perks.Add("Haste");

                int value = this.CurrentAttack;
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
            if (CardSpecialActions.Any())
            {
                foreach (var (trigger, description, action) in CardSpecialActions.Where(x => x.trigger == actionType))
                {
                   ///* DEBUG INFO */ Console.WriteLine($"\tAction/s Triggered! ~ {description}");
                    action();
                }
            }
        }
        public override string GetCardString()
        {
            {
                return $"{this.GetType().Name,12} ║ {base.GetCardString()} ║ atk:{CurrentAttack,2}, hp:{CurrentHealth,2}";
            }
        }
        public void Attack(Creature? _defender)
        {
            if (CurrentHealth <= 0) 
            {
                return; 
            }

           ///* DEBUG INFO */Console.WriteLine($"[Player {Owner.PlayerNumberID}] Atakuje kartą {this.Name}");

            if (_defender != null)
            {
                if (_defender.CurrentHealth <= 0) 
                {
                    return; 
                }
                this.CurrentHealth -= _defender.CurrentAttack;
                _defender.CurrentHealth -= this.CurrentAttack;
                _defender.UseSpecialAction(ActionType.Block);
              
            }
            else
            {
                Owner.Opponent.DealDamage(this.CurrentAttack);
            }

            UseSpecialAction(ActionType.Attack);
            IsTapped = true;
        }
        public void ResetStatsAfterFight()
        {
            CurrentHealth = BaseHealth;
            CurrentAttack = BaseAttack;
        }
        public void RevieweResetToDefault()
        {
            CurrentHealth = CoreHealth;
            CurrentAttack = CoreAttack;
            isDead = false;
        }
    }
}

