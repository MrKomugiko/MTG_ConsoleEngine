using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public abstract class EngineBase
    {
        protected delegate void Phase(PlayerBase _player);

        public readonly PlayerBase[] Players = new PlayerBase[2];
        public Creature[] DeclaredAttackers;
        public Dictionary<Creature, Creature> DeclaredDeffenders = new(); // <obrońca / atakujacy> , bo mozna blokowac kilkoma stworkami
        public int CurrentPlayerIndex = 0;
        public bool logs = true;
        protected List<Phase> GamePhases = new();
        public int TurnCounter = 0;
        public PhaseType CurrentPhase { get; set; }

        public Creature[] GetAttackerDeclaration() => DeclaredAttackers;
        public Dictionary<Creature, Creature> GetDeffendersDeclaration() => DeclaredDeffenders;

        public abstract void RunGameLoop(ref bool gameEnded);
        public abstract void Beginning_Phase(PlayerBase _player);
        public abstract void First_Main_Phase(PlayerBase _player);
        public abstract void Combat_Phase(PlayerBase _player);
        public abstract void Second_Main_Phase(PlayerBase _player);
        public abstract void End_Phase(PlayerBase _player);
       

        protected abstract void ExecuteCombat();
        protected abstract void HandCardsCleanUpCountChecker(PlayerBase _player);
        protected void RecoverHealthSurvivedCreatures()
        {
            Players[0].CombatField.ForEach(creature => ((Creature)creature).ResetStatsAfterFight());
        }
       // protected abstract void CastInstantIfCan(Player _player);
        public List<CardBase> GetRequiredLandsToPayCards(List<CardBase> cardsToPlay, PlayerBase caster)
        {
            int[] ManaCost = new int[6];
            ManaCost = caster.SimpleSumMana(cardsToPlay.Select(x=>x.ManaCost).ToList());

            int[] SumManaOwnedAndAvailable = caster.CurrentTotalManaStatus;

            int[] creatureManaCostCopy = new int[6];
            creatureManaCostCopy = ManaCost;
            List<CardBase> landCardsToTap = new();
            List<CardBase> currentLandsCardsCopy = caster.ManaField.Where(c => c.IsTapped == false).ToList();

            List<int> coreColors = new();
            for(int i = 1; i <6;i++)
            {
                if (creatureManaCostCopy[i] > 0) coreColors.Add(i);
            }

           creatureManaCostCopy = ManaCost;
            for (int i= 1;i<6; i++) // od indexu '1' , pomijajać index 0 czyli bez koloru
            {
                if (SumManaOwnedAndAvailable[i] >= creatureManaCostCopy[i])
                {
                    foreach (Land coreLand in currentLandsCardsCopy.Where(x => ((Land)x).manaValue.codeIndex  == i))
                    {
                        if (creatureManaCostCopy[i] == 0) break;

                        SumManaOwnedAndAvailable[i] -= coreLand.manaValue.value;
                        creatureManaCostCopy[i] -= coreLand.manaValue.value;
                        landCardsToTap.Add(coreLand);
                        continue;
                    }
                }
                else return (new());
            }

            int clearColorValue = creatureManaCostCopy[0];

           
            if (clearColorValue > 0)
            {
                //if (SumManaOwnedAndAvailable[0] >= clearColorValue)
                //{
                    List<CardBase> listaNoCoreLandow = currentLandsCardsCopy
                            .Except(landCardsToTap)
                            .Where(x => coreColors.Contains(((Land)x).manaValue.codeIndex) == false)
                            .ToList();

                    int sumnoCoreValues = listaNoCoreLandow.Sum(x => ((Land)x).manaValue.value);

                    if (sumnoCoreValues >= clearColorValue)
                    {
                        foreach (Land noCoreLand in listaNoCoreLandow)  // starczy many posługując sie no core colorami
                        {
                            if (creatureManaCostCopy[0] == 0) break;

                            SumManaOwnedAndAvailable[noCoreLand.manaValue.codeIndex] -= noCoreLand.manaValue.value;
                            creatureManaCostCopy[0] -= noCoreLand.manaValue.value;
                            landCardsToTap.Add(noCoreLand);
                        }
                    }
                    else
                    {
                        foreach (Land noCoreLand in listaNoCoreLandow) // przewertowanie nie core landow 
                        {
                            if (creatureManaCostCopy[0] == 0) break; 

                            SumManaOwnedAndAvailable[noCoreLand.manaValue.codeIndex] -= noCoreLand.manaValue.value;
                            creatureManaCostCopy[0] -= noCoreLand.manaValue.value;
                            landCardsToTap.Add(noCoreLand);
                        }
                        foreach (Land coreLand in currentLandsCardsCopy.Except(landCardsToTap)) // cała reszta dostępnych i nie użytych
                        {
                            if (creatureManaCostCopy[0] == 0) break;

                            SumManaOwnedAndAvailable[coreLand.manaValue.codeIndex] -= coreLand.manaValue.value;
                            creatureManaCostCopy[0] -= coreLand.manaValue.value;
                            landCardsToTap.Add(coreLand);
                        }
                    }
                //}
                //else
                //    return (new());
            }

                // sprawdzenie czy wyszliscmy na 0 z kosztami
            if ( creatureManaCostCopy[0] == 0 && creatureManaCostCopy[1] == 0 && creatureManaCostCopy[2] == 0 && 
                 creatureManaCostCopy[3] == 0 && creatureManaCostCopy[4] == 0 && creatureManaCostCopy[5] == 0 )
                return (landCardsToTap);
            else
                return (new());

        }


        public void Start()
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
        public List<CardBase> GetValidTargetsForCardType(CardBase card, int PlayerID = 0)
        {
            if (card is Instant instant)
            {
                List<CardBase> targets = new();
                List<CardBase> playerA = new();
                List<CardBase> playerB = new();

                if (PlayerID == 0)
                {
                    playerA = Players[0].CombatField.Where(x => x is Creature).ToList();
                    playerB = Players[1].CombatField.Where(x => x is Creature).ToList();
                }
                else
                {
                    playerA = Players[1].CombatField.Where(x => x is Creature).ToList();
                    playerB = Players[0].CombatField.Where(x => x is Creature).ToList();
                }

                if (card.TargetsType == TargetType.Ally)
                {
                    targets.AddRange(playerA);
                }
                else if (card.TargetsType == TargetType.Enemy)
                {
                    targets.AddRange(playerB);
                }
                else if (card.TargetsType == TargetType.Both)
                {
                    targets.AddRange(playerA);
                    targets.AddRange(playerB);
                }
                //playerA.ForEach(target => Console.WriteLine($"[0-{Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));
                //playerB.ForEach(target => Console.WriteLine($"[1-{Players[1].CombatField.IndexOf((CardBase)target)}] Player 2 : {((CardBase)target).Name}"));

                targets.AddRange(playerA);
                targets.AddRange(playerB);

                return targets;
            }
            if (card is Enchantment enchantment)
            {
                List<CardBase> targets = new();
                List<CardBase> playerA = new();
                List<CardBase> playerB = new();

                if (PlayerID == 0)
                {
                    playerA = Players[0].CombatField.Where(x => x is Creature).ToList();
                    playerB = Players[1].CombatField.Where(x => x is Creature).ToList();
                }
                else
                {
                    playerA = Players[1].CombatField.Where(x => x is Creature).ToList();
                    playerB = Players[0].CombatField.Where(x => x is Creature).ToList();
                }

                if (card.TargetsType == TargetType.Ally)
                {
                    targets.AddRange(playerA);
                }
                else if (card.TargetsType == TargetType.Enemy)
                {
                    targets.AddRange(playerB);
                }
                else if (card.TargetsType == TargetType.Both)
                {
                    targets.AddRange(playerA);
                    targets.AddRange(playerB);
                }
                //playerA.ForEach(target => Console.WriteLine($"[0-{Players[0].CombatField.IndexOf((CardBase)target)}] Player 1 : {((CardBase)target).Name}"));
                //playerB.ForEach(target => Console.WriteLine($"[1-{Players[1].CombatField.IndexOf((CardBase)target)}] Player 2 : {((CardBase)target).Name}"));

                targets.AddRange(playerA);
                targets.AddRange(playerB);
                return targets;
            }

            return new();
        }
        protected void MoveDeadCreaturesToGraveyard(PlayerBase _player)
        {
            var DeadCreatures = _player.CombatField.Where(c => ((Creature)c).CurrentHealth <= 0).ToList();
            _player.Graveyard.AddRange(DeadCreatures);
            for (int i = 0; i < DeadCreatures.Count; i++)
            {
                var corpse = DeadCreatures[i];
                _player.CombatField.Remove(corpse);
                corpse.Owner.Graveyard.Add(corpse);

                if (corpse is Creature c)
                {
                    c.CurrentHealth = c.BaseHealth;
                    c.CurrentAttack = c.BaseAttack;
                }
            }
        }
        protected void ShuffleDeck(PlayerBase deckOwner)
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
        public enum TargetType
        {
            Ally,
            Enemy,
            Both,
            None
        }
    }
}