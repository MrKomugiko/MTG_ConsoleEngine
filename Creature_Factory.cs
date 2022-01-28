using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Creature_Factory
    {
        public Creature Get_WalkingCorpse()
        {
            return new Creature
            (
                _manaCost: new Dictionary<string, int>() { {"",1}, {"B",1} },
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
                _manaCost: new Dictionary<string, int>(){ {"",3}, {"B",1} },
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
        public Land Get_Swamp()
        {
            var land = new Land
            (
                _identificator: "269_M21",
                _name: "Swamp"
            );
            land.AddSpecialAction($"Add 1 Black mana");
            return land;
        }
    }
}