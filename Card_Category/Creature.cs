namespace MTG_ConsoleEngine.Card_Category
{
    public class Creature : CardBase
    {
        private readonly int CoreHealth; // orginalnie, bez zanych boostów
        private readonly int CoreAttack; // orginalnie, bez zanych boostów

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
                if(value <= 0)
                {
                    _currentHealth = 0; 

                   // Console.WriteLine($"{Name} died...");

                    // sprawdzenie czy ma na sobie enchantmenty
                    if(EnchantmentSlots.Count > 0)
                    {
                        // wykoananie na kazdym akcji on creature die
                        EnchantmentSlots.ForEach(ench=>ench.UseSpecialAction(ActionType.CreatureDied));
                        // wurzucenie zużytych enczantow do graveyards
                        EnchantmentSlots.ForEach(ench=>Owner.Graveyard.Add(ench));
                        // usunięcie wszystkich enczantów z karty po jej smierci
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

        public Creature(Dictionary<int, int> _manaCost, string _identificator,string _name, string _category, string _description, int _health, int _attack)
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
                //Console.WriteLine("Action/s Triggered!");
                foreach (var (trigger, description, action) in CardSpecialActions.Where(x => x.trigger == actionType))
                {
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
            // attacker ktory nie ma życia nie moze zaatakowac xd ( np wczesniej po potraktowaniu instantem)
            if (CurrentHealth <= 0) 
            {
               // Console.WriteLine($"Jednostka atakujaca {(this.Name)} już nie żyje, atak nie zostaje wyprowadzony. next");
                return; 
            }
           // Console.ForegroundColor = ConsoleColor.DarkGreen;
            //Console.WriteLine($"Player {Owner.PlayerNumberID} Atakuje kartą {this.Name}");
           // Console.ResetColor();
            if (_defender != null)
            {
                if (_defender.CurrentHealth <= 0) 
                {
                   // Console.WriteLine($"Przeciwnik {(_defender.Name)} już nie żyje, atak nie obrona nie zostałą przeprowadzona przez nią.");
                    return; 
                }
                this.CurrentHealth -= _defender.CurrentAttack;
                _defender.CurrentHealth -= this.CurrentAttack;
                _defender.UseSpecialAction(ActionType.Block);
              
            }
            else
            {
               // Console.WriteLine("Atak nie napotkał obrony, atak w Przeciwnika (Player'a)");
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
        }
    }
}

