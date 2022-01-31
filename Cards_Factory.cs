using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Cards_Factory
    {
        public Creature Get_WalkingCorpse()
        {
            return new Creature
            (
                _manaCost: new Dictionary<string, int>() { { "", 1 }, { "B", 1 } },
                _identificator: "128_M21",
                _name: "Walking Corpse",
                _category: "Zombie",
                _description: "''Feeding a normal army is a problem of logistics. With zombies, it is an asset. Feeding is why they fight. Feeding is why they are feared.'' -- Jadar, ghoulcaller of Nephalia",
                _health: 2,
                _attack: 2
            );
        }
        public Creature Get_InfectiousHorror()
        {
            var creature = new Creature
            (
                _manaCost: new Dictionary<string, int>() { { "", 3 }, { "B", 1 } },
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
        public Creature Get_Banehound()
        {
            var creature = new Creature
            (
                _manaCost: new Dictionary<string, int>() { { "B", 1 } },
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
    }
    public class Land_Factory
    {
        public Land Get_Swamp()
        {
            var land = new Land
            (
                _identificator: "311_MID",
                _name: "Swamp",
                _manaCode: "B"
            );
            land.AddSpecialAction($"Add 1 Black mana");
            return land;
        }
        public Land Get_Mountain()
        {
            var land = new Land
            (
                _identificator: "275_MID",
                _name: "Mountain",
                _manaCode: "R"
            );
            land.AddSpecialAction($"Add 1 Red mana");
            return land;
        }
        public Land Get_Plains()
        {
            var land = new Land
            (
                _identificator: "268_MID",
                _name: "Plains",
                _manaCode: "W"
            );
            land.AddSpecialAction($"Add 1 White mana");
            return land;
        }
        public Land Get_Forest()
        {
            var land = new Land
            (
                _identificator: "277_MID",
                _name: "Forest",
                _manaCode: "G"
            );
            land.AddSpecialAction($"Add 1 Green mana");
            return land;
        }
        public Land Get_Island()
        {
            var land = new Land
            (
                _identificator: "265_M21",
                _name: "Island",
                _manaCode: "U"
            );
            land.AddSpecialAction($"Add 1 Blue mana");
            return land;
        }
    }
    public class Enchantment_Factory
    {
        public Enchantment Get_DeadWeight()
        {
            var enchantment = new Enchantment
            (
                _manaCost: new Dictionary<string, int>() {{ "B", 1 } },
                _identificator: "067_GRN",
                _name: "Dead Weight",
                _category: "Aura",
                _description: "All things considered, his first day on patrol could have gone better"
            );
             enchantment.AddSpecialAction($"Enchant creature");
             enchantment.AddSpecialAction($"Enchanted creature gets -2/-2.");
             return enchantment;
        }
        public Enchantment Get_InfernalScarring()
        {
            var enchantment = new Enchantment
            (
                _manaCost: new Dictionary<string, int>() { { "",1 },{ "B",1 } },
                _identificator: "105_M21",
                _name: "Infernal Scarring",
                _category: "Aura",
                _description: "One who is marked by demon in life is sure to be rembered as one in death."
            );
            
             enchantment.AddSpecialAction($"Enchant creature");
             enchantment.AddSpecialAction($"Enchanted creature gets +2/+0 and has \"When this creature dies, draw a card.\"");

             return enchantment;
        }
    }
    public class Instant_Factory
    {
        public Instant Get_SorinsThirst()
        {
            var instant = new Instant
            (
                _manaCost: new Dictionary<string, int>() {{ "B", 2 } },
                _identificator: "104_WAR",
                _name: "Sorin's Thirst",
                _description: "''I see you're out of the wall.'' ~ Nahiri"
            );
             instant.AddSpecialAction($"{instant.Name} deals 2 damage to target creature and you gain 2 life.");
             return instant;
        }
    }
}