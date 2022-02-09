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


        public override void RunGameLoop(ref bool gameEnded)
        {
            int gamephaselenght = GamePhases.Count;
            while (true)
            {
                // Console.WriteLine("Turn number: "+TurnCounter);
                for (int i = 0; i < gamephaselenght; i++)
                {
                    if (Players[CurrentPlayerIndex].Health <= 0)
                    {
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

            _player.RefreshManaStatus(); // reset cached at start new turn
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


        protected override void ExecuteCombat()
    {
        if(DeclaredDeffenders.Count > 0)
        {
           ///* DEBUG INFO */ Console.WriteLine();
           ///* DEBUG INFO */ Console.WriteLine("---------------------------------------------------------------------------------------------------------------");
           ///* DEBUG INFO */ Console.WriteLine("---------------------------------------------- Combat notes: --------------------------------------------------");
        }
        foreach (var attacker in DeclaredAttackers) /* <DEFFENDER, ATTACKER> */
        {
            if (DeclaredDeffenders.Any(x => x.Value == attacker))
            {
                if (attacker.CurrentHealth <= 0) continue;

                foreach (var defender in DeclaredDeffenders.Where(x => x.Value == attacker))
                {
                   ///* DEBUG INFO */Console.WriteLine($"(owner: player {attacker.Owner.PlayerNumberID}) " + $"{attacker.Name} ({attacker.CurrentAttack}/{attacker.CurrentHealth}) " + $"Vs (owner: player {defender.Key.Owner.PlayerNumberID}) " + $"{defender.Key.Name} ({defender.Key.CurrentAttack}/{defender.Key.CurrentHealth})");

                    if (defender.Key.CurrentHealth <= 0) continue;

                    attacker.Attack(defender.Key);
                }
            }
            attacker.Attack(_defender: null);
        }
        if (DeclaredDeffenders.Count > 0)
        {
           ///* DEBUG INFO */ Console.WriteLine("---------------------------------------------------------------------------------------------------------------");
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
    }
}
