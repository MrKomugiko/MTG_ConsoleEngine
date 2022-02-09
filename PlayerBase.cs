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

        protected int[] _currentTotalManaStatus = new int[6];

        public int[] CurrentTotalManaStatus { get => _currentTotalManaStatus; }
        
        public void RefreshManaStatus()
        {
          SumAvailableManaFromManaField();
        } 

        public abstract Creature[] Get_AvailableAttackers();
        public abstract List<Creature> Get_AvailableDeffenders();
        public abstract void Heal(int value);
        public abstract bool PlayCardFromHand(CardBase card, List<CardBase> landsToPay = null);
        public void SumAvailableManaFromManaField()
        {
            _currentTotalManaStatus[0] = 0;
            _currentTotalManaStatus[1] = 0;
            _currentTotalManaStatus[2] = 0;
            _currentTotalManaStatus[3] = 0;
            _currentTotalManaStatus[4] = 0;
            _currentTotalManaStatus[5] = 0;

            //sumowanie dostepnej many
            foreach (Land manaCard in ManaField.Where(c=> c.IsTapped == false))
            {
                _currentTotalManaStatus[manaCard.manaValue.codeIndex] += manaCard.manaValue.value;
            }
        }    
        public void AddToDeck(CardBase _card)
        {
            _card.Owner = this;
            Deck.Add(_card);
        }
        protected int RecurCounter = 0;
        protected void RecurPowerSet(ref List<int[]> _OUTPUT, int[] _parentState, int[] leftToCheck, List<CardBase> _playableCardsFromHand)
        {
            
           // Console.WriteLine("VALID COMBINATION :" + String.Join(",", _parentState));
            _OUTPUT.Add(_parentState);
            // if (RecurCounter > 25) return;

            int[] newParentState = new int[_parentState.Length+1];
            _parentState.CopyTo(newParentState,0);

            if (leftToCheck.Length == 0) return;

            int[] temp = leftToCheck;

            int index = 0;
            for (int j= 0; j< temp.Length; j++)
            {
                newParentState[newParentState.Length-1] = temp[j];

                int[] itemsLeft = new int[temp.Length - 1];
                index = 0;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] == newParentState[newParentState.Length - 1]) continue;
                    itemsLeft[index++] = temp[i];
                }

                var skillsToSum = new List<int[]>();
                for (int i = 0; i < newParentState.Length; i++)
                {
                    skillsToSum.Add(_playableCardsFromHand.First(x => x.ID == newParentState[i]).ManaCost);
                };

                int[] summed = SimpleSumMana(skillsToSum);

                var costcheck = SimpleManaCheck(summed);
                if (costcheck ==false) continue;
                //if (newParentState.Length > 2) continue; // <------ restriction to only max 2 cards pairs

                RecurPowerSet(ref _OUTPUT, newParentState, itemsLeft, _playableCardsFromHand);
            }
        }
        public int[] SimpleSumMana(List<int[]> cardsToSum)
        {
            int[] sumData = new int[6];
            for (int i = 0; i < cardsToSum.Count; i++)
            {
                sumData[0] += cardsToSum[i][0];
                sumData[1] += cardsToSum[i][1];
                sumData[2] += cardsToSum[i][2];
                sumData[3] += cardsToSum[i][3];
                sumData[4] += cardsToSum[i][4];
                sumData[5] += cardsToSum[i][5];
            }
            return sumData;
        }

        public  bool SimpleManaCheck(int[] totalCost)
        {
            // sprawdzenie różnic pomiędzy "core colorami
            int[] manadiff = new int[6];
            manadiff[0] = CurrentTotalManaStatus[0] - totalCost[0];
            manadiff[1] = CurrentTotalManaStatus[1] - totalCost[1];
            manadiff[2] = CurrentTotalManaStatus[2] - totalCost[2];
            manadiff[3] = CurrentTotalManaStatus[3] - totalCost[3];
            manadiff[4] = CurrentTotalManaStatus[4] - totalCost[4];
            manadiff[5] = CurrentTotalManaStatus[5] - totalCost[5];

            if (manadiff[1] < 0 || manadiff[2] < 0 || manadiff[3] < 0 || manadiff[4] < 0 || manadiff[5] < 0)
            {
               // Console.WriteLine("brak podstawowej ilosci core landow");
                return false;
            }

            // sprawdzenie czy suma reszty jest w stanie pokryc koszta no-color
            int sum = manadiff[1] + manadiff[2] + manadiff[3] + manadiff[4] + manadiff[5];
            if (-manadiff[0] > sum)
            {
              //  Console.WriteLine("nie starczy core many na pokrycie many no-color");
                return false;
            }
           // Console.WriteLine("starczy many mana difference: " + String.Join(" | ", manadiff.Select(kvp => kvp.Value)));
           // Console.WriteLine("reszta: " + (manadiff[0] + sum));

            return true;
        }
    }
}