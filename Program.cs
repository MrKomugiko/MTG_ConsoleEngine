using MTG_ConsoleEngine;
Console.WriteLine("MTG Game Engine");

Player Player1 = new Player(1,20);
Player Player2 = new Player(2,20); 

Cards_Factory _CF = new();
Land_Factory _LF = new();
Enchantment_Factory _EF = new();
Instant_Factory _IF = new();

Engine Game = new Engine(Player1, Player2);
for(int i = 0; i<2; i++)
{
    Enumerable.Range(0,4).ToList().ForEach( x => {
        Engine.Players[i].AddToDeck(_CF.Get_Banehound());
        Engine.Players[i].AddToDeck(_CF.Get_WalkingCorpse());
        Engine.Players[i].AddToDeck(_CF.Get_InfectiousHorror());    
        Engine.Players[i].AddToDeck(_EF.Get_DeadWeight());
        Engine.Players[i].AddToDeck(_EF.Get_InfernalScarring());
        Engine.Players[i].AddToDeck(_IF.Get_SorinsThirst());
    });
    Enumerable.Range(0,7).ToList().ForEach( x => {
        Engine.Players[i].AddToDeck(_LF.Get_Swamp());
    });
}



// Player1.ManaField.Add(_LF.Get_Swamp());
// Player1.ManaField.Add(_LF.Get_Swamp());
// Player1.ManaField.Add(_LF.Get_Swamp());
// Player1.ManaField.Add(_LF.Get_Swamp());
// Player2.ManaField.Add(_LF.Get_Swamp());

// Player2.ManaField.Add(_LF.Get_Swamp());
// Player1.ManaField.Add(_LF.Get_Swamp());
// Player1.ManaField.Add(_LF.Get_Swamp());
// Player1.ManaField.Add(_LF.Get_Swamp());
// Player1.ManaField.Add(_LF.Get_Swamp());

Game.Start();

Console.ReadLine();
