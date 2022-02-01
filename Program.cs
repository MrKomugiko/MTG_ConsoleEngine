using MTG_ConsoleEngine;
Console.WriteLine("MTG Game Engine");

Player Player1 = new Player(1,20);
Player Player2 = new Player(2,20); 
Player1.Opponent = Player2;
Player2.Opponent = Player1;


Engine Game = new Engine(Player1, Player2);

Enumerable.Range(0,4).ToList().ForEach( x => {
    Player1.AddToDeck(Creature_Factory.Get_Banehound());
    Player1.AddToDeck(Creature_Factory.Get_WalkingCorpse());
    Player1.AddToDeck(Creature_Factory.Get_InfectiousHorror());    
    Player1.AddToDeck(Enchantment_Factory.Get_DeadWeight());
    Player1.AddToDeck(Enchantment_Factory.Get_InfernalScarring());
    Player1.AddToDeck(Instant_Factory.Get_SorinsThirst());
});
Enumerable.Range(0,14).ToList().ForEach( x => {
    Player1.AddToDeck(Land_Factory.Get_Swamp());
});

Enumerable.Range(0,4).ToList().ForEach( x => {
    Player2.AddToDeck(Creature_Factory.Get_Banehound());
    Player2.AddToDeck(Creature_Factory.Get_WalkingCorpse());
    Player2.AddToDeck(Creature_Factory.Get_InfectiousHorror());    
    Player2.AddToDeck(Enchantment_Factory.Get_DeadWeight());
    Player2.AddToDeck(Enchantment_Factory.Get_InfernalScarring());
    Player2.AddToDeck(Instant_Factory.Get_SorinsThirst());
});
Enumerable.Range(0,14).ToList().ForEach( x => {
    Player2.AddToDeck(Land_Factory.Get_Swamp());
});

Game.Start();

Console.ReadLine();
