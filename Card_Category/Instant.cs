namespace MTG_ConsoleEngine.Card_Category
{
    public class Instant : CardBase
    {
        public Instant(Dictionary<string, int> _manaCost, string _identificator, string _name, string _description) 
            : base(_manaCost, _identificator, _name, _description, "Instant")
        {
        }

        public override bool isTapped { get; set; }
        public void ShowInstantRequirments()
        {
            foreach(var action in CardSpecialActions)
            {
                Console.WriteLine($"\t {action.description}");
            }
        }

        public object SpellSelectedTarget { get; set; } = new();
        public List<object> GetValidTargets()
        {
            Console.WriteLine("Get valid targets for instant: "+Name);
            List<object> targets = new();
            // sprawdzenie naszego pola
            targets.AddRange(Engine.Players[0].CombatField.Where(x=>x is Creature));
            targets.AddRange(Engine.Players[1].CombatField.Where(x=>x is Creature));

            return targets;
        }

        public override void AddSpecialAction(string _specialActionInfo)
        {
             _specialActionInfo = _specialActionInfo.ToLower(); 
                                                                                                // sorin's thirst deals 2 damage to target creature and you gain 2 life.
            // podzielenie akcji na części slowo "and"
            var splitedDescription = _specialActionInfo.Replace(".","").Replace(",","").Replace(Name.ToLower(),"").Trim().Split("and");           
                                                                                                 // [ deals 2 damage to target creature ]  [ you gain 2 life ]
            foreach(var desc in splitedDescription)
            {
                if(desc.Contains("deals") && desc.Contains("damage to target creature"))
                {
                    int damageValue = Int32.Parse(desc.Replace("damage to target creature","").Replace("deals","").Trim());

                    CardSpecialActions.Add
                    (
                        (
                            trigger: ActionType.CastInstant,
                            description: desc,
                            action: () => Actions.DamageSelectedCreature(damageValue, (Creature)SpellSelectedTarget)
                        )
                    );
                    continue;
                }

                if(desc.Contains("you gain") && desc.Contains("life"))
                {
                    int healValue = Int32.Parse(desc.Replace("you gain","").Replace("life","").Trim());      

                    CardSpecialActions.Add
                    (
                        (
                            trigger: ActionType.CastInstant,
                            description: desc,
                            action: () => Actions.Heal(healValue,Owner)
                        )
                    );
                    continue;
                }
            }
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