namespace MTG_ConsoleEngine.Card_Category
{
    public class Enchantment : CardBase
    {
        public override bool isTapped { get; set; }
        public string Category { get; }
        public string UseOn { get; private set;} = "";
        public CardBase? AssignedToCard = null;
        public Enchantment(Dictionary<string, int> _manaCost, string _identificator, string _name, string _description, string _category) : 
            base(_manaCost, _identificator, _name, _description, "Enchantment")
        {
            Category = _category;
        }
        public List<object> GetValidTargets()
        {
            // TODO: sposob zeby nadpisywac zwracanie dostepnych celow
            Console.WriteLine("Get valid targets for Enchantment: "+Name);
            List<object> targets = new();
         
            var player1Creatures = Engine.Players[0].CombatField.Where(x=>x is Creature).ToList();
            var player2Creatures = Engine.Players[1].CombatField.Where(x=>x is Creature).ToList();
            
            player1Creatures.ForEach(target => Console.WriteLine($"[0-{Engine.Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));
            player2Creatures.ForEach(target => Console.WriteLine($"[1-{Engine.Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));

            targets.AddRange(player1Creatures);
            targets.AddRange(player2Creatures);

            if(targets.Count == 0)
            {
                Console.WriteLine("Brak dostępnych celów użycia karty...");
            }
            return targets;
        }
        public override void AddSpecialAction(string _specialActionInfo)
        {
            _specialActionInfo = _specialActionInfo.ToLower();     
            if(_specialActionInfo == "enchant creature")
            {
                UseOn = "Creature";
                return;
            }
            
            // podzielenie akcji na części slowo "and"
            var splitedDescription = _specialActionInfo.Split("and");
            foreach(var desc in splitedDescription)
            {
                if(desc.Contains("when this creature dies, draw a card."))
                {
                    CardSpecialActions.Add
                    (
                        (
                            trigger: ActionType.CreatureDied,
                            description: "When this creature dies, draw a card.",
                            action: () => Owner.DrawCard()
                        )
                    );
                }
                if(desc.Contains("enchanted creature gets"))
                {
                    var values = desc.Replace("enchanted creature gets","").Replace(".","").Replace(",","").Trim().Split("/");
                    int extraAttackValue = Int32.Parse(values[0]);
                    int extraHealthValue = Int32.Parse(values[1]);

                    CardSpecialActions.Add
                    (
                        (
                            trigger: ActionType.OnEnnchantAdded,
                            description: desc,
                            action: () => Actions.AssingExtraValuesToCreature( hp:extraHealthValue, atk:extraAttackValue, (Creature?)AssignedToCard)
                        )
                    );
                }
            }
        }
        public void AssingToCreature(Creature target)
        {
            // parowanie kart
            target.EnchantmentSlots.Add(this); // 1 karta moze miec kilka enchantów na sobie
            this.AssignedToCard = target; // 1 enchant tylko na 1 karte
            Console.WriteLine($"Enchantment [{this.Name}] został nałożony na karte [{target.Name}]");
            
            UseSpecialAction(ActionType.OnEnnchantAdded);
        }
        public override void UseSpecialAction(ActionType trigger)
        {
            if(CardSpecialActions.Count > 0)
            {
                Console.WriteLine("Action/s Triggered!");
                foreach(var actions in CardSpecialActions.Where(x=>x.trigger == trigger))
                {   
                    actions.action();
                }
            }
        }
        public override string GetCardString()
        {
            return $"{this.GetType().Name.PadLeft(12)} ║ {base.GetCardString()} ║ {"".PadLeft(13)}";
        }
    }
}