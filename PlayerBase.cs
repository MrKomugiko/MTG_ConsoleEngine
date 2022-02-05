using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public abstract class PlayerBase
    {
        public bool IsAI = false;

        public int PlayerNumberID;
        public int PlayerIndex;
        public ConsoleColor color;
        public EngineBase _gameEngine;
        public PlayerBase Opponent;
        protected Random rand = new();

        public int Health { get; internal set; } = 20;
        public List<CardBase> ManaField { get; internal set; } = new();
        public List<CardBase> CombatField { get; private set; } = new();
        public List<CardBase> Hand { get; private set; } = new();
        public List<CardBase> Deck { get; private set; } = new();
        public List<CardBase> Graveyard { get; private set; } = new();
        public List<CardBase> Exiled { get; private set; } = new();
        public bool IsLandPlayedThisTurn { get; internal set; }
        public abstract void DealDamage(int value);
        public abstract void DisplayPlayerField();
        public abstract void DrawCard();
        public Creature[] GetCreaturesFromField(int[] indexes, bool validated = false)
        {
            indexes = indexes.Distinct().ToArray();
            if (indexes[0] == -1)
            {
                //Console.WriteLine(" >>>>> tym razem bez walki...");
                return Array.Empty<Creature>(); // nie atakujemy wcale wartosc -1
            }

            if (validated == true)
            {
                Creature[] _attackersToDeclare = new Creature[indexes.Length];
                for (int i = 0; i < indexes.Length; i++)
                {
                    _attackersToDeclare[i] = (Creature)CombatField[indexes[i]];
                   // Console.WriteLine($">> Atak :[{CombatField.IndexOf(_attackersToDeclare[i])}] {_attackersToDeclare[i].Name} [{_attackersToDeclare[i].CurrentAttack}/{_attackersToDeclare[i].CurrentHealth}]");
                }
                return _attackersToDeclare;
            }
            else
            {
                Creature[] availableTargets = Get_AvailableAttackers().ToArray();

                if (availableTargets.Length == 0)
                    return Array.Empty<Creature>();

                List<Creature> _attackersToDeclare = new();

                foreach (int attacker in indexes)
                {
                    if (attacker > CombatField.Count - 1 || attacker < 0)
                        continue;

                    //Creature attackingCreature = (Creature)CombatField[attacker];
                    var attackingCreature = CombatField[attacker];

                    if (_attackersToDeclare.Contains(attackingCreature))
                        continue;

                    if (availableTargets.Contains(attackingCreature))
                    {
                        _attackersToDeclare.Add((Creature)attackingCreature);
                        //Console.WriteLine("zarejestrowano :" + attackingCreature.Name);
                    }

                    continue;
                }

                return _attackersToDeclare.ToArray();
            }
        }
        public abstract Creature[] Get_AvailableAttackers();
        public abstract List<Creature> Get_AvailableDeffenders();
        public abstract void Heal(int value);
        public abstract bool PlayCardFromHand(CardBase card, List<CardBase> landsToPay = null);
        public Dictionary<string, int> SumAvailableManaFromManaField()
        {
            Dictionary<string, int> SumManaOwnedAndAvailable = new();
            //sumowanie dostepnej many
            foreach (Land manaCard in ManaField.Where(c => c is Land && c.IsTapped == false))
            {
                foreach (var mana in manaCard.manaValue)
                {
                    if (SumManaOwnedAndAvailable.ContainsKey(mana.Key))
                    {
                        SumManaOwnedAndAvailable[mana.Key] += mana.Value;
                    }
                    else
                    {
                        SumManaOwnedAndAvailable.Add(mana.Key, mana.Value);
                    }
                }
            }

            return SumManaOwnedAndAvailable;
        }    
        public void AddToDeck(CardBase _card)
        {
            _card.Owner = this;
            Deck.Add(_card);
        }
    }
}