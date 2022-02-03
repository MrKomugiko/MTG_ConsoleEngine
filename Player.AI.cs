using MTG_ConsoleEngine.Card_Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTG_ConsoleEngine
{
    public partial class Player
    {
        Random rand = new Random();
        private static IEnumerable<int[]> Combinations(int k, int n)
        {
            var result = new int[k];
            var stack = new Stack<int>();
            stack.Push(1);

            while (stack.Count > 0)
            {
                var index = stack.Count - 1;
                var value = stack.Pop();

                while (value <= n)
                {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index == k)
                    {
                        yield return result;
                        break;
                    }
                }
            }
        }
        
        public List<int[]> GetAllPossibleAttackCombinationsIndexes(List<Creature> _availableTargets)
        {
            List<int[]> result = new List<int[]>();
			//Console.WriteLine("count "+availableTargets.Count);
            if (_availableTargets.Count == 0)
                return new List<int[]> { new int[1] { -1 } };

            int[] arr = _availableTargets.Select(x=>CombatField.IndexOf(x)).ToArray();

            for(int i = arr.Length-1; i>=0; i--)
            {
                int n = _availableTargets.Count;
                int k = _availableTargets.Count- i;

                result.Add(new int[k+1]);
                foreach (int[] combo in Combinations(k, n))
                {
                 //  Console.WriteLine(string.Join(", ", combo));
                   result.Add(combo.ToArray());
                }
            }

            var trimmedArr = new List<int[]>();
            foreach (int[] myindexes in result)
            {
                if (myindexes.Length > 0)
                {
                    if (myindexes[0] == 0 && myindexes[1] == 0)
                    {
                        continue;
                    }
                    for (int i = 0; i < myindexes.Length; i++)
                    {
                        myindexes[i] -= 1;
                    }
                }
                trimmedArr.Add(myindexes);
            }

            trimmedArr.Add(new int[1] { -1 });
            return trimmedArr;
        }
        public void AI_RemoveOneCardFromHand()
        {
            /*_AI_*/ // remove one random card if total holding > 7
            int randomIndexToRemove = rand.Next(1, this.Hand.Count);
            CardBase card = this.Hand[randomIndexToRemove];
            //Console.WriteLine("LOSOWO! Wyrzuciłeś " + card.Name);
            this.Graveyard.Add(card);
            this.Hand.Remove(card);
        }
        public void AI_PlayLandCardIfPossible()
        {
            /*_AI_*/ // auto pick land if possible
            CardBase firstLandFromHAnd = this.Hand.FirstOrDefault(x => x is Land );
            if (firstLandFromHAnd != null && IsLandPlayedThisTurn == false)
            {
                //Console.WriteLine("Auto place Land");
                this.PlayCardFromHand(firstLandFromHAnd);
            }
        }
        public void AI_PlayRandomInstatFromHand()
        {
            /*_AI_*/ // random pick instant if can
            //Console.WriteLine($"IDIOTYCZNE, LOSOWE RUCHY ! ~ by AI:Player_{this.PlayerNumberID} \n >> AI CANT CHOOSE INTANTS ... YET! <<");

            if (this.Hand.Any(x => (x is Instant) && ((Instant)x).isAbleToPlay))
            {
                //Console.WriteLine($"Szana na kontre uzywajac instantów lub umiejetnosci kart gracz {this.PlayerNumberID}");
                //Console.WriteLine("---------------- INSTANTS ONLY ---------------- ");

                while (true)
                {
                    List<Instant> availableInstants = this.Hand.Where(x => x is Instant && x.isAbleToPlay)
                            .Select(x => (Instant)x).ToList();
                    //Console.WriteLine("lista mozliwych akcji");
                    //foreach (Instant card in availableInstants)
                    //{
                    //    Console.WriteLine($"Play: {card.Name} [{card.GetType().Name}]");
                    //}

                    if (availableInstants.Count() == 0) break;

                    //TODO: wybieranie skila jakos xd
                    // sprawdzanie czy jest jakis powod do uzywania instanta xd 
                    //  ewentualknie dodac narazie losowy, bedzie dzialac przy 1 intancie 
                    //  gorzej gdy bedziie kilka opcji ... hmm
                   // Console.WriteLine(">> AI CANT CHOOSE INSTANTS YET! <<");
                    break;
                }
            }

        }

        public void AI_MakePlaysWithRandomCardFromHand()
        {
            
            //Console.WriteLine($"IDIOTYCZNE, LOSOWE RUCHY ! ~ by AI:Player_{this.PlayerNumberID} \n zagraj losową kartą z ręki -> hard coded only creatures");
            // hard coded play creature if can
            while (true)
            {
                // get list available draws
                Dictionary<int, (bool result, List<CardBase> landsToTap)> handdata = new();

                List<CardBase> randomPlays = this.Hand.Where(card => card.isAbleToPlay).ToList();
                var creaturesOnly = randomPlays.Where(c => c is Creature).ToList();
                //Console.WriteLine("lista mozliwych akcji");
                //foreach (CardBase card in randomPlays)
                //{
                //    Console.WriteLine($"Play: {card.Name} [{card.GetType().Name}] {((card is Creature)?"<---":"")}");
                //}

                if (creaturesOnly.Count() == 0) break;
                CardBase randomCreatureFromHand = creaturesOnly[rand.Next(0, creaturesOnly.Count())];
                this.PlayCardFromHand(randomCreatureFromHand);
            }
        }
        public void AI_RandomAttackDeclaration()
        {
            /*_AI_*/ // random attack sequention from available

            List<Creature> availableTargets = this.Get_AvailableAttackers();
            List<int[]> randomIndexesList = this.GetAllPossibleAttackCombinationsIndexes(availableTargets);
            int[] inputArr = randomIndexesList[rand.Next(randomIndexesList.Count)];

            _gameEngine.DeclaredAttackers = this.GetCreaturesFromField(inputArr);
            //Console.WriteLine($">>>>>> Attackers declared by Player {PlayerNumberID}: "+String.Join("\n- ", _gameEngine.DeclaredAttackers.Select(x=>x.Name)));
        }
        public Dictionary<Creature,Creature> AI_RandomDeffendersDeclaration()
        {
            //Console.WriteLine(">>>>>> AI CANT CHOOSE DDEFENDERS ... YET! <<");
            return new();
        }

        public List<Creature> GetCreaturesFromField(int[] indexes, bool validated = false)
        {
            indexes = indexes.Distinct().ToArray();
            if (indexes[0] == -1)
            {
               // Console.WriteLine(" [-1] / tym razem bez walki...");
                return new(); // nie atakujemy wcale wartosc -1
            }
                
            if(validated == true)
            {
                List<Creature> _attackersToDeclare = new();
                foreach (int attacker in indexes)
                {
                    Creature attackingCreature = (Creature)CombatField[attacker];
                    _attackersToDeclare.Add(attackingCreature);
                   // Console.WriteLine("dodano do rejestracji :" + attackingCreature.Name);
                }
                return _attackersToDeclare;
                }
            else
            {
                List<Creature> availableTargets = Get_AvailableAttackers();

                if (availableTargets.Count == 0)
                    return new();

                List<Creature> _attackersToDeclare = new();

                foreach (int attacker in indexes)
                {
                    if (attacker > CombatField.Count - 1 || attacker < 0)
                        continue;

                    Creature attackingCreature = (Creature)CombatField[attacker];
                    if (_attackersToDeclare.Contains(attackingCreature))
                        continue;

                    if (availableTargets.Contains(attackingCreature))
                    {
                        _attackersToDeclare.Add(attackingCreature);
                        //Console.WriteLine("zarejestrowano :" + attackingCreature.Name);
                    }

                    continue;
                }

                return _attackersToDeclare;
            }
        }
    }   
}   
