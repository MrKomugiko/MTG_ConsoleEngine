using MTG_ConsoleEngine;

//PlayerAI Player1 = new PlayerAI(1, 20);
//Player Player2 = new Player(2, 20);
//Player1.Opponent = Player2;
//Player2.Opponent = Player1;

//Engine Game = new Engine(Player1, Player2);
//Game.logs = true;
//Enumerable.Range(0, 4).ToList().ForEach(x =>
//{
//    Player1.AddToDeck(Creature_Factory.Get_Banehound());
//    Player1.AddToDeck(Creature_Factory.Get_WalkingCorpse());
//    Player1.AddToDeck(Creature_Factory.Get_InfectiousHorror());
//    Player1.AddToDeck(Enchantment_Factory.Get_DeadWeight());
//    Player1.AddToDeck(Enchantment_Factory.Get_InfernalScarring());
//    Player1.AddToDeck(Instant_Factory.Get_SorinsThirst());
//});
//Enumerable.Range(0, 14).ToList().ForEach(x =>
//{
//    Player1.AddToDeck(Land_Factory.Get_Swamp());
//});
//int CardID = 0;
//Player1.Deck.ForEach(x => x.ID = CardID++);

//Enumerable.Range(0, 4).ToList().ForEach(x =>
//{
//    Player2.AddToDeck(Creature_Factory.Get_Banehound());
//    Player2.AddToDeck(Creature_Factory.Get_WalkingCorpse());
//    Player2.AddToDeck(Creature_Factory.Get_InfectiousHorror());
//    Player2.AddToDeck(Enchantment_Factory.Get_DeadWeight());
//    Player2.AddToDeck(Enchantment_Factory.Get_InfernalScarring());
//    Player2.AddToDeck(Instant_Factory.Get_SorinsThirst());
//});
//Enumerable.Range(0, 14).ToList().ForEach(x =>
//{
//    Player2.AddToDeck(Land_Factory.Get_Swamp());
//});
//CardID = 0;
//Player2.Deck.ForEach(x => x.ID = CardID++);

//Game.Start();
//Console.ReadLine();




//B: Run stuff you want timed


List<CardBase> TEST_DECK = new();

Enumerable.Range(0, 4).ToList().ForEach(x =>
{
TEST_DECK.Add(Creature_Factory.Get_Banehound());
TEST_DECK.Add(Creature_Factory.Get_WalkingCorpse());
TEST_DECK.Add(Creature_Factory.Get_InfectiousHorror());
TEST_DECK.Add(Enchantment_Factory.Get_DeadWeight());
TEST_DECK.Add(Enchantment_Factory.Get_InfernalScarring());
TEST_DECK.Add(Instant_Factory.Get_SorinsThirst());
});
Enumerable.Range(0, 14).ToList().ForEach(x =>
{
TEST_DECK.Add(Land_Factory.Get_Swamp());
});

List<CardBase> TEST_DECK_2 = new();

Enumerable.Range(0, 4).ToList().ForEach(x =>
{
TEST_DECK_2.Add(Creature_Factory.Get_Banehound());
TEST_DECK_2.Add(Creature_Factory.Get_WalkingCorpse());
TEST_DECK_2.Add(Creature_Factory.Get_InfectiousHorror());
TEST_DECK_2.Add(Enchantment_Factory.Get_DeadWeight());
TEST_DECK_2.Add(Enchantment_Factory.Get_InfernalScarring());
TEST_DECK_2.Add(Instant_Factory.Get_SorinsThirst());
});
Enumerable.Range(0, 14).ToList().ForEach(x =>
{
TEST_DECK_2.Add(Land_Factory.Get_Swamp());
});

int CardID = 0;
TEST_DECK_2.ForEach(x => x.ID = CardID++);
CardID = 0;
TEST_DECK.ForEach(x => x.ID = CardID++);


for (int i = 0; i < 500_000; i++)
{
    PlayerAI Player1 = new(1, 20);
    PlayerAI Player2 = new(2, 20);
    Player1.Opponent = Player2;
    Player2.Opponent = Player1;

    SimulationsEngine Game = new(Player1, Player2);
    Game.logs = false;
    for (int j = 0; j < TEST_DECK.Count; j++)
    {
        Player1.AddToDeck(TEST_DECK[j]);
        Player2.AddToDeck(TEST_DECK_2[j]);
    }

    Game.Start();
    //Console.WriteLine("end of game:");
    //Console.WriteLine("Turns played: " + Game.TurnCounter);
    //Console.WriteLine("player 1 hp:" + Game.Players[0].Health);
    //Console.WriteLine("player 2 hp:" + Game.Players[1].Health);
    //Console.WriteLine();
    //Console.WriteLine("Player 1 Deck count:       " + Player1.Deck.Count);
    //Console.WriteLine("         Hand count:       " + Player1.Hand.Count);
    //Console.WriteLine("         LandArea count:   " + Player1.ManaField.Count);
    //Console.WriteLine("         CombatZone count: " + Player1.CombatField.Count);
    //Console.WriteLine("         Graveyard count:  " + Player1.Graveyard.Count);
    //Console.WriteLine();
    //Console.WriteLine("Player 2 Deck count:       " + Player2.Deck.Count);
    //Console.WriteLine("         Hand count:       " + Player2.Hand.Count);
    //Console.WriteLine("         LandArea count:   " + Player2.ManaField.Count);
    //Console.WriteLine("         CombatZone count: " + Player2.CombatField.Count);
    //Console.WriteLine("         Graveyard count:  " + Player2.Graveyard.Count);
    //Console.WriteLine("------------------------------------------");
    //Console.WriteLine();
}

Console.WriteLine("Stop");


