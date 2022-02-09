using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public static  class Creature_Factory
    {
        public  static Creature Get_WalkingCorpse()
        {
            return new Creature
            (
                _manaCost: new int[] { 1, 1, 0, 0, 0, 0 },
                _identificator: "128_M21",
                _name: "Walking Corpse",
                _category: "Zombie",
                _description: "''Feeding a normal army is a problem of logistics. With zombies, it is an asset. Feeding is why they fight. Feeding is why they are feared.'' -- Jadar, ghoulcaller of Nephalia",
                _health: 2,
                _attack: 2
            );
        }
        public static Creature Get_InfectiousHorror()
        {
            var creature = new Creature
            (
                _manaCost: new int[] { 3, 1, 0, 0, 0, 0 },
                _identificator: "101_M19",
                _name: "Infectious Horror",
                _category: "Zombie",
                _description: "Not one in the history of Grixis has anyone died of old age",
                _health: 2,
                _attack: 2
            );
            creature.AddSpecialAction($"Whenever {creature.Name} attacks, each opponent loses 2 life.");
            return creature;
        }
        public static Creature Get_Banehound()
        {
            var creature = new Creature
            (
                _manaCost: new int[] { 0, 1, 0, 0, 0, 0 },
                _identificator: "077_WAR",
                _name: "Banehound",
                _category: "Nightmare Hound",
                _description: "''I wish I could train a pack of them for hunting in the undercity. But I`d never dare turn muy back, and I hate to think what I`d have to feed them.'' ~ Zhomir, urban huntmaster",
                _health: 1,
                _attack: 1
            );
            creature.AddSpecialAction($"Lifelink, haste");
            return creature;
        }
        public static Creature Get_EpicureOfBlood()
        {
            var creature = new Creature
            (
                _manaCost: new int[] { 4, 1, 0, 0, 0, 0 },
                _identificator: "095_M19",
                _name: "Epicure of Blood",
                _category: "Vampire",
                _description: "'Fleshy, wish just a hint of leather. A fine vintage.'",
                _health: 4,
                _attack: 4
            );
            //TODO: not implemented
            creature.AddSpecialAction($"Whenever you gain life, each opponent loses 1 life");
            return creature;
        }
        public static Creature Get_FellSpecter()
        {
            var creature = new Creature
            (
                _manaCost: new int[] { 4, 1, 0, 0, 0, 0 },
                _identificator: "096_M19",
                _name: "Fell Specter",
                _category: "Specter",
                _description: "'Fleshy, wish just a hint of leather. A fine vintage.'",
                _health: 3,
                _attack: 1
            );

            //TODO: not implemented
            creature.AddSpecialAction($"Flying");
            //TODO: not implemented
            creature.AddSpecialAction($"When Fell Specter enters the battlefield, target opponent discard a card.");
            //TODO: not implemented
            creature.AddSpecialAction($"Whenever an opponent discards a card, that player loses 2 life.");


            return creature;
        }
    }
    public  static class Land_Factory
    {
        public  static Land Get_Swamp()
        {
            var land = new Land
            (
                _identificator: "311_MID",
                _name: "Swamp"
            );
            land.AddSpecialAction($"Add 1 Black mana");
            return land;
        }
        public static  Land Get_Mountain()
        {
            var land = new Land
            (
                _identificator: "275_MID",
                _name: "Mountain"
            );
            land.AddSpecialAction($"Add 1 Red mana");
            return land;
        }
        public static Land Get_Plains()
        {
            var land = new Land
            (
                _identificator: "268_MID",
                _name: "Plains"
            );
            land.AddSpecialAction($"Add 1 White mana");
            return land;
        }
        public static Land Get_Forest()
        {
            var land = new Land
            (
                _identificator: "277_MID",
                _name: "Forest"
            );
            land.AddSpecialAction($"Add 1 Green mana");
            return land;
        }
        public static  Land Get_Island()
        {
            var land = new Land
            (
                _identificator: "265_M21",
                _name: "Island"
            );
            land.AddSpecialAction($"Add 1 Blue mana");
            return land;
        }
    }
    public  static class Enchantment_Factory
    {
        public static Enchantment Get_DeadWeight()
        {
            var enchantment = new Enchantment
            (
                _manaCost: new int[] { 0, 1, 0, 0, 0, 0 },
                _identificator: "067_GRN",
                _name: "Dead Weight",
                _category: "Aura",
                _description: "All things considered, his first day on patrol could have gone better",
                _mainTarget: EngineBase.TargetType.Enemy
            );
             enchantment.AddSpecialAction($"Enchant creature");
             enchantment.AddSpecialAction($"Enchanted creature gets -2/-2.");
             return enchantment;
        }
        public static Enchantment Get_InfernalScarring()
        {
            var enchantment = new Enchantment
            (
                _manaCost: new int[] { 1, 1, 0, 0, 0, 0 },
                _identificator: "105_M21",
                _name: "Infernal Scarring",
                _category: "Aura",
                _description: "One who is marked by demon in life is sure to be rembered as one in death.",
                _mainTarget: EngineBase.TargetType.Ally
            );
            
             enchantment.AddSpecialAction($"Enchant creature");
             enchantment.AddSpecialAction($"Enchanted creature gets +2/+0 and has \"When this creature dies, draw a card.\"");

             return enchantment;
        }
    }
    public  static class Instant_Factory
    {
        public static Instant Get_SorinsThirst()
        {
            var instant = new Instant
            (
                _manaCost: new int[] { 0, 2, 0, 0, 0, 0 },
                _identificator: "104_WAR",
                _name: "Sorin's Thirst",
                _description: "''I see you're out of the wall.'' ~ Nahiri",
                _mainTarget: EngineBase.TargetType.Enemy
            );
             instant.AddSpecialAction($"{instant.Name} deals 2 damage to target creature and you gain 2 life.");
             return instant;
        }
    }
}