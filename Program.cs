using MTG_ConsoleEngine;
using System.Diagnostics;

Console.WriteLine("MTG Game Engine");




Stopwatch stopWatch = new Stopwatch();
stopWatch.Start();



List<CardBase> TEST_DECK = new List<CardBase>();

Enumerable.Range(0, 4).ToList().ForEach(x => {
    TEST_DECK.Add(Creature_Factory.Get_Banehound());
    TEST_DECK.Add(Creature_Factory.Get_WalkingCorpse());
    TEST_DECK.Add(Creature_Factory.Get_InfectiousHorror());
    TEST_DECK.Add(Enchantment_Factory.Get_DeadWeight());
    TEST_DECK.Add(Enchantment_Factory.Get_InfernalScarring());
    TEST_DECK.Add(Instant_Factory.Get_SorinsThirst());
});
Enumerable.Range(0, 14).ToList().ForEach(x => {
    TEST_DECK.Add(Land_Factory.Get_Swamp());
});

List<CardBase> TEST_DECK_2 = new List<CardBase>();

Enumerable.Range(0, 4).ToList().ForEach(x => {
    TEST_DECK_2.Add(Creature_Factory.Get_Banehound());
    TEST_DECK_2.Add(Creature_Factory.Get_WalkingCorpse());
    TEST_DECK_2.Add(Creature_Factory.Get_InfectiousHorror());
    TEST_DECK_2.Add(Enchantment_Factory.Get_DeadWeight());
    TEST_DECK_2.Add(Enchantment_Factory.Get_InfernalScarring());
    TEST_DECK_2.Add(Instant_Factory.Get_SorinsThirst());
});
Enumerable.Range(0, 14).ToList().ForEach(x => {
    TEST_DECK_2.Add(Land_Factory.Get_Swamp());
});

for(int i = 0; i < 10; i++)
{
    TEST_DECK[i].ID = i;
    TEST_DECK_2[i].ID = i;
}

//B: Run stuff you want timed

for (int i = 0; i < 50_000; i++)
{
    Player Player1 = new Player(1, 20, true);
    Player Player2 = new Player(2, 20, true);
    Player1.Opponent = Player2;
    Player2.Opponent = Player1;

    Engine Game = new Engine(Player1, Player2);
    Game.logs = false;
    for (int j = 0; j < TEST_DECK.Count; j++)
    {
        Player1.AddToDeck(TEST_DECK[j]);
        Player2.AddToDeck(TEST_DECK_2[j]);
    }


    Game.Start();
}

Console.WriteLine("Stop");




stopWatch.Stop();
// Get the elapsed time as a TimeSpan value.
TimeSpan ts = stopWatch.Elapsed;

// Format and display the TimeSpan value.
string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    ts.Hours, ts.Minutes, ts.Seconds,
    ts.Milliseconds / 10);

Console.WriteLine(elapsedTime);
Console.ReadLine();








//Player Player1 = new Player(1, 20, false);
//Player Player2 = new Player(2, 20, true); 
//Player1.Opponent = Player2;
//Player2.Opponent = Player1;


//Engine Game = new Engine(Player1, Player2);

//Enumerable.Range(0,4).ToList().ForEach( x => {
//    Player1.AddToDeck(Creature_Factory.Get_Banehound());
//    Player1.AddToDeck(Creature_Factory.Get_WalkingCorpse());
//    Player1.AddToDeck(Creature_Factory.Get_InfectiousHorror());    
//    Player1.AddToDeck(Enchantment_Factory.Get_DeadWeight());
//    Player1.AddToDeck(Enchantment_Factory.Get_InfernalScarring());
//    Player1.AddToDeck(Instant_Factory.Get_SorinsThirst());
//});
//Enumerable.Range(0,14).ToList().ForEach( x => {
//    Player1.AddToDeck(Land_Factory.Get_Swamp());
//});

//Enumerable.Range(0,4).ToList().ForEach( x => {
//    Player2.AddToDeck(Creature_Factory.Get_Banehound());
//    Player2.AddToDeck(Creature_Factory.Get_WalkingCorpse());
//    Player2.AddToDeck(Creature_Factory.Get_InfectiousHorror());    
//    Player2.AddToDeck(Enchantment_Factory.Get_DeadWeight());
//    Player2.AddToDeck(Enchantment_Factory.Get_InfernalScarring());
//    Player2.AddToDeck(Instant_Factory.Get_SorinsThirst());
//});
//Enumerable.Range(0,14).ToList().ForEach( x => {
//    Player2.AddToDeck(Land_Factory.Get_Swamp());
//});

//Game.Start();

//Console.ReadLine();
