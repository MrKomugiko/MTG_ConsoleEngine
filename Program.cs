using MTG_ConsoleEngine;

Console.WriteLine("MTG Game Engine");

Player Player1 = new Player(0,20);
    Player1.ID = 1;

Player Player2 = new Player(1,20); 
    Player2.ID = 2;

Engine Game = new Engine(Player1, Player2);

Creature_Factory _CF = new();

Player1.AddToDeck(_CF.Get_InfectiousHorror());
Player1.AddToDeck(_CF.Get_InfectiousHorror());
Player1.AddToDeck(_CF.Get_InfectiousHorror());
Player1.AddToDeck(_CF.Get_WalkingCorpse());
Player1.AddToDeck(_CF.Get_WalkingCorpse());
Player1.AddToDeck(_CF.Get_InfectiousHorror());
Player1.AddToDeck(_CF.Get_InfectiousHorror());
Player1.AddToDeck(_CF.Get_InfectiousHorror());
Player1.AddToDeck(_CF.Get_WalkingCorpse());
Player1.AddToDeck(_CF.Get_WalkingCorpse());

Player2.AddToDeck(_CF.Get_InfectiousHorror());
Player2.AddToDeck(_CF.Get_InfectiousHorror());
Player2.AddToDeck(_CF.Get_WalkingCorpse());
Player2.AddToDeck(_CF.Get_WalkingCorpse());
Player2.AddToDeck(_CF.Get_InfectiousHorror());
Player2.AddToDeck(_CF.Get_InfectiousHorror());
Player2.AddToDeck(_CF.Get_WalkingCorpse());
Player2.AddToDeck(_CF.Get_WalkingCorpse());


// foreach(var card in Player1.Deck)
// {
//     Console.WriteLine($"----------------------- Player 1 [ Deck: {Player1.Deck.IndexOf(card)+1}/{(Player1.Deck.Count).ToString().PadLeft(2)} ] ------------------------");
//     Console.WriteLine($"--------------- [ Card name: {card.Name.ToString().PadLeft(25)} ] ---------------");
//     switch(card){
//         case Creature creature:
//             creature.Attack();
//         break;
//     }
//     Console.WriteLine("------------------------------------------------------------------------");
// }



Game.Start();

Console.ReadLine();
