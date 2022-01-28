using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Creature_Factory
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
                _name: "Swamp"
            );
            land.AddSpecialAction($"Add 1 Black mana");
            return land;
        }
        public Land Get_Mountain()
        {
            var land = new Land
            (
                _identificator: "275_MID",
                _name: "Mountain"
            );
            land.AddSpecialAction($"Add 1 Red mana");
            return land;
        }
        public Land Get_Plains()
        {
            var land = new Land
            (
                _identificator: "268_MID",
                _name: "Plains"
            );
            land.AddSpecialAction($"Add 1 White mana");
            return land;
        }
        public Land Get_Forest()
        {
            var land = new Land
            (
                _identificator: "277_MID",
                _name: "Forest"
            );
            land.AddSpecialAction($"Add 1 Green mana");
            return land;
        }
        public Land Get_Island()
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
}