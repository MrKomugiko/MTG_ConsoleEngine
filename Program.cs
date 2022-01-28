using MTG_ConsoleEngine;

Console.WriteLine("MTG Game Engine");

Player Player1 = new Player(0,20);
    Player1.ID = 1;

Player Player2 = new Player(1,20); 
    Player2.ID = 2;

Engine Game = new Engine(Player1, Player2);
// Game.Start();

Creature WalkingCorpse = new Creature
(
    _manaCost: new Dictionary<string, int>()
    {
            {"",1},
            {"B",1}
        },
    _identificator: "128_M21",
    _name: "Walking Corpse",
    _category: "Zombie",
    _description: "''Feeding a normal army is a problem of logistics. With zombies, it is an asset. Feeding is why they fight. Feeding is why they are feared.'' -- Jadar, ghoulcaller of Nephalia",
    _health: 2,
    _attack: 2
);

Creature InfectiousHorror = new Creature
(
    _manaCost: new Dictionary<string, int>()
    {
            {"",3},
            {"B",1}
        },
    _identificator: "101_M19",
    _name: "Infectious Horror",
    _category: "Zombie",
    _description: "Not one in the history of Grixis has anyone died of old age",
    _health: 2,
    _attack: 2
);

InfectiousHorror.AddSpecialAction($"Whenever {InfectiousHorror.Name} attacks, each opponent loses 2 life.");
Player1.AddToDeck(InfectiousHorror);
Player1.AddToDeck((Creature)InfectiousHorror.Clone());
Player1.AddToDeck((Creature)InfectiousHorror.Clone());
Player1.AddToDeck(WalkingCorpse);
Player2.AddToDeck((Creature)InfectiousHorror.Clone());
Player2.AddToDeck(WalkingCorpse);


foreach(var card in Player1.Deck)
{
    Console.WriteLine($"----------------------- Player 1 [ Deck: {Player1.Deck.IndexOf(card)+1}/{(Player1.Deck.Count).ToString().PadLeft(2)} ] ------------------------");
    Console.WriteLine($"--------------- [ Card name: {card.Name.ToString().PadLeft(25)} ] ---------------");
    switch(card){
        case Creature creature:
            creature.Attack();
        break;
    }
    Console.WriteLine("------------------------------------------------------------------------");
}
Console.ReadLine();
