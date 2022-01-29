using MTG_ConsoleEngine;

Console.WriteLine("MTG Game Engine");

Player Player1 = new Player(1,20);
Player Player2 = new Player(2,20); 

Engine Game = new Engine(Player1, Player2);

Creature_Factory _CF = new();
Land_Factory _LF = new();
Enchantment_Factory _EF = new();

Player1.AddToDeck(_LF.Get_Swamp());
Player1.AddToDeck(_CF.Get_Banehound());
Player1.AddToDeck(_LF.Get_Swamp());
Player1.AddToDeck(_CF.Get_InfectiousHorror());
Player1.AddToDeck(_CF.Get_WalkingCorpse());
Player1.AddToDeck(_LF.Get_Swamp());
Player1.AddToDeck(_CF.Get_WalkingCorpse());
Player1.AddToDeck(_CF.Get_Banehound());
Player1.AddToDeck(_CF.Get_Banehound());
Player1.AddToDeck(_EF.Get_InfernalScarring());


Player2.AddToDeck(_LF.Get_Swamp());
Player2.AddToDeck(_CF.Get_InfectiousHorror());
Player2.AddToDeck(_CF.Get_Banehound());
Player2.AddToDeck(_CF.Get_InfectiousHorror());
Player2.AddToDeck(_CF.Get_WalkingCorpse());
Player2.AddToDeck(_LF.Get_Swamp());
Player2.AddToDeck(_CF.Get_WalkingCorpse());
Player2.AddToDeck(_CF.Get_Banehound());
Player2.AddToDeck(_CF.Get_Banehound());


Player1.ManaField.Add(_LF.Get_Swamp());
Player1.ManaField.Add(_LF.Get_Mountain());

Player2.ManaField.Add(_LF.Get_Mountain());

Game.Start();

Console.ReadLine();
