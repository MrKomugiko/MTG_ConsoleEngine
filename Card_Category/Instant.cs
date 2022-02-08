namespace MTG_ConsoleEngine.Card_Category
{
    public class Instant : CardBase
    {
        public Instant(Dictionary<int, int> _manaCost, string _identificator, string _name, string _description, EngineBase.TargetType _mainTarget) 
            : base(_manaCost, _identificator, _name, _description, "Instant")
        {
            base.TargetsType = _mainTarget;
        }
        public override bool IsTapped { get; set; }
        public object SpellSelectedTarget { get; set; } = new();
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
                            action: () => Actions.DamageSelectedCreature(damageValue, (Creature)this.SpellSelectedTarget)
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
                            action: () => Actions.Heal(healValue,this.Owner)
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
            return $"{this.GetType().Name,12} ║ {base.GetCardString()} ║ {"",13}";
        }
    }
}