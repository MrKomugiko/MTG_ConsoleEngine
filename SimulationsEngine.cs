using MTG_ConsoleEngine.Card_Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTG_ConsoleEngine
{
    public class SimulationsEngine : EngineBase
    {
        public SimulationsEngine(PlayerBase FirstPlayer, PlayerBase SecondPlayer)
        {
            GamePhases.Add(Beginning_Phase);
            GamePhases.Add(First_Main_Phase);
            GamePhases.Add(Combat_Phase);
            GamePhases.Add(Second_Main_Phase);
            GamePhases.Add(End_Phase);

            Players[0] = FirstPlayer;
            Players[0]._gameEngine = this;
            Players[1] = SecondPlayer;
            Players[1]._gameEngine = this;
        }

        public override void Start()
        {
            #region StartNewGame
            CurrentPhase = PhaseType.StartNewGame;
            ShuffleDeck(Players[0]);
            ShuffleDeck(Players[1]);

            for (int i = 0; i < 7; i++)
            {
                Players[0].DrawCard();
                Players[1].DrawCard();
            }

            TurnCounter = 0;
            CurrentPlayerIndex = 0;

            #endregion

            bool gameEnded = false;
            RunGameLoop(ref gameEnded);
        }
        public override void RunGameLoop(ref bool gameEnded)
        {
            int gamephaselenght = GamePhases.Count;
            while (true)
            {
                for (int i = 0; i < gamephaselenght; i++)
                {
                    if (Players[CurrentPlayerIndex].Health <= 0)
                    {
                        //Console.WriteLine("Turns played: " + TurnCounter);
                        //Console.WriteLine("end of game:");
                        //Console.WriteLine("player 1 hp:" + Players[0].Health);
                        //Console.WriteLine("player 2 hp:" + Players[1].Health);
                        gameEnded = true;
                        return;

                    }
                    GamePhases[i](Players[CurrentPlayerIndex]);
                }

                CurrentPlayerIndex = CurrentPlayerIndex == 0 ? 1 : 0;
                TurnCounter++;
            };
        }
        public override void Beginning_Phase(PlayerBase _player)
        {
            #region Begining_Phase
            CurrentPhase = PhaseType.Beginning_Phase;

            _player.CombatField.ForEach(card => card.IsTapped = false);
            _player.ManaField.ForEach(card => card.IsTapped = false);

            if (TurnCounter >= 2)
            {
                _player.DrawCard();
            }
            #endregion
        }
        public override void First_Main_Phase(PlayerBase _player)
        {
            #region First_Main_Phase
            CurrentPhase = PhaseType.First_Main_Phase;

            ((PlayerAI)_player).AI_MakePlaysWithRandomCardFromHand();
            return;

            #endregion
        }
    public override void Combat_Phase(PlayerBase _player)
    {
        #region Combat_Phase_Begining
        CurrentPhase = PhaseType.Combat_Phase_Begining;
        #endregion

        #region Combat_Phase_AttackersDeclaration
        CurrentPhase = PhaseType.Combat_Phase_AttackersDeclaration;

            DeclaredAttackers = ((PlayerAI)_player).AI_RandomAttackDeclaration();
        
   
        #endregion

        #region Combat_Phase_DeffendersDeclaration
        CurrentPhase = PhaseType.Combat_Phase_DefendersDeclaration;
        if (DeclaredAttackers.Length > 0)
        {
            DeclaredDeffenders = ((PlayerAI)_player.Opponent).AI_RandomDeffendersDeclaration();
        }
   
        #endregion

        #region Combat_Phase_AttackerInstants
        CurrentPhase = PhaseType.Combat_Phase_AttackerInstants;

            ((PlayerAI)_player).AI_PlayRandomInstatFromHand();

        #endregion

        #region Combat_Phase_DeffenderInstants
        CurrentPhase = PhaseType.Combat_Phase_DeffenderInstants;

        ((PlayerAI)_player.Opponent).AI_PlayRandomInstatFromHand();

        #endregion

        #region Combat_Phase_Combat
        CurrentPhase = PhaseType.Combat_Phase_Combat;
        ExecuteCombat();

        Array.Clear(DeclaredAttackers);
        DeclaredDeffenders.Clear();

        // ewentualne wyłączenie enczantow podczas fazy końca combatu

        MoveDeadCreaturesToGraveyard(_player.Opponent);
        MoveDeadCreaturesToGraveyard(_player);
        #endregion

        #region Combat_Phase_End of Combat
        CurrentPhase = PhaseType.Combat_Phase_End;
        #endregion
    }
    public override void Second_Main_Phase(PlayerBase _player)
    {
        #region Second_Main_Phase
        CurrentPhase = PhaseType.Second_Main_Phase;

        //Console.WriteLine("Second_Main_Phase: Player "+_player.ID);

        ((PlayerAI)_player).AI_MakePlaysWithRandomCardFromHand();
      
        #endregion
    }
    public override void End_Phase(PlayerBase _player)
    {
        #region End_Phase
        CurrentPhase = PhaseType.End_Phase;

        _player.IsLandPlayedThisTurn = false;

        RecoverHealthSurvivedCreatures();
      
        HandCardsCleanUpCountChecker(Players[_player.PlayerNumberID == 1 ? 1 : 0]);
        #endregion
    }


    public override List<object> GetValidTargetsForCardType(CardBase card)
    {
        if (card is Instant instant)
        {
            // TODO: sposob zeby nadpisywac zwracanie dostepnych celow
            Console.WriteLine("Get valid targets for Instant: " + instant.Name);
            List<object> targets = new();

            var player1Creatures = Players[0].CombatField.Where(x => x is Creature).ToList();
            var player2Creatures = Players[1].CombatField.Where(x => x is Creature).ToList();

          // player1Creatures.ForEach(target => Console.WriteLine($"[0-{Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));
          // player2Creatures.ForEach(target => Console.WriteLine($"[1-{Players[1].CombatField.IndexOf((CardBase)target)}] Player 2 : {((CardBase)target).Name}"));

            targets.AddRange(player1Creatures);
            targets.AddRange(player2Creatures);

            return targets;
        }
        if (card is Enchantment enchantment)
        {
            // TODO: sposob zeby nadpisywac zwracanie dostepnych celow
            Console.WriteLine("Get valid targets for Enchantment: " + enchantment.Name);
            List<object> targets = new();

            var player1Creatures = Players[0].CombatField.Where(x => x is Creature).ToList();
            var player2Creatures = Players[1].CombatField.Where(x => x is Creature).ToList();

            //player1Creatures.ForEach(target => Console.WriteLine($"[0-{Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));
           // player2Creatures.ForEach(target => Console.WriteLine($"[1-{Players[1].CombatField.IndexOf((CardBase)target)}] Player 2 : {((CardBase)target).Name}"));

            targets.AddRange(player1Creatures);
            targets.AddRange(player2Creatures);

            if (targets.Count == 0)
            {
              //  Console.WriteLine("Brak dostępnych celów użycia karty...");
            }
            return targets;
        }

        return new();
    }


    protected override void ExecuteCombat()
    {
        foreach (var attacker in DeclaredAttackers) /* <DEFFENDER, ATTACKER> */
        {
            if (DeclaredDeffenders.Any(x => x.Value == attacker))
            {
                if (attacker.CurrentHealth <= 0) continue;

                foreach (var defender in DeclaredDeffenders.Where(x => x.Value == attacker))
                {
                    if (defender.Key.CurrentHealth <= 0) continue;

                    attacker.Attack(defender.Key);
                }
            }
            attacker.Attack(_defender: null);
        }
    }
    protected override void HandCardsCleanUpCountChecker(PlayerBase _player)
    {
        if (TurnCounter < 2) return;

        while (_player.Hand.Count >= 7)
        {
                ((PlayerAI)_player).AI_RemoveOneCardFromHand();
        }
    }

    protected override void MoveDeadCreaturesToGraveyard(PlayerBase _player)
    {
        var DeadCreatures = _player.CombatField.Where(c => ((Creature)c).CurrentHealth <= 0).ToList();
        _player.Graveyard.AddRange(DeadCreatures);
        for (int i = 0; i < DeadCreatures.Count; i++)
        {
            var corpse = DeadCreatures[i];
            _player.CombatField.Remove(corpse);
            //Console.WriteLine($"send {corpse.Name} to Graveyard...");
            corpse.Owner.Graveyard.Add(corpse);
            corpse.Owner.CombatField.Remove(corpse);
        }
    }
    protected override void ShuffleDeck(PlayerBase deckOwner)
    {
        var rand = new Random();
        for (var i = deckOwner.Deck.Count - 1; i > 0; i--)
        {
            var temp = deckOwner.Deck[i];
            var index = rand.Next(0, i + 1);
            deckOwner.Deck[i] = deckOwner.Deck[index];
            deckOwner.Deck[index] = temp;
        }
    }
}
}
