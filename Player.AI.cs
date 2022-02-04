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
        
        public List<int[]> GetAllPossibleAttackCombinationsIndexes(Creature[] _availableTargets)
        {
            List<int[]> result = new List<int[]>();
			//Console.WriteLine("count "+availableTargets.Count);
            if (_availableTargets.Length == 0)
                return new List<int[]> { new int[1] { -1 } };

            int[] arr = _availableTargets.Select(x=>CombatField.IndexOf(x)).ToArray();

            for(int i = arr.Length-1; i>=0; i--)
            {
                int n = _availableTargets.Length;
                int k = _availableTargets.Length - i;

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
            CardBase firstLandFromHAnd = this.Hand.FirstOrDefault(x => x is Land);
            if (firstLandFromHAnd != null && IsLandPlayedThisTurn == false)
            {
                //Console.WriteLine("Auto place Land");
                this.PlayCardFromHand(firstLandFromHAnd);
            }
        }
        public void AI_PlayRandomInstatFromHand()
        {
            
            if (this.Hand.Any(x => (x is Instant) && ((Instant)x).isAbleToPlay))
            {
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
            AI_PlayLandCardIfPossible();
            //Console.WriteLine($"IDIOTYCZNE, LOSOWE RUCHY ! ~ by AI:Player_{this.PlayerNumberID} \n zagraj losową kartą z ręki -> hard coded only creatures");
            // hard coded play creature if can
            while (true)
            {
                // get list available draws
                //Dictionary<int, (bool result, List<CardBase> landsToTap)> handdata = new();

                List<CardBase> creaturesOnly = this.Hand.Where(card => card is Creature && card.isAbleToPlay).ToList();
               // var creaturesOnly = randomPlays.Where(c => c is Creature).ToList();
                //Console.WriteLine("lista mozliwych akcji");
                //foreach (CardBase card in randomPlays)
                //{
                //    Console.WriteLine($"Play: {card.Name} [{card.GetType().Name}] {((card is Creature)?"<---":"")}");
                //}

                if (creaturesOnly.Count == 0) break;
                CardBase randomCreatureFromHand = creaturesOnly[rand.Next(0, creaturesOnly.Count())];
                this.PlayCardFromHand(randomCreatureFromHand);
            }
        }
        public Creature[] AI_RandomAttackDeclaration()
        {
            /*_AI_*/ // random attack sequention from available

            Creature[] availableTargets = this.Get_AvailableAttackers();
            List<int[]> randomIndexesList = this.GetAllPossibleAttackCombinationsIndexes(availableTargets);
            //Console.WriteLine("\t >> possible attack combinations: ");
            //foreach(var combination in randomIndexesList)
           // {
           //     Console.WriteLine("\t\t - "+String.Join(",", combination));
           // }
            int[] inputArr = randomIndexesList[rand.Next(randomIndexesList.Count)];

            return this.GetCreaturesFromField(inputArr,true).ToArray();
            //Console.WriteLine($">>>>>> Attackers declared by Player {PlayerNumberID}: "+String.Join("\n- ", _gameEngine.DeclaredAttackers.Select(x=>x.Name)));
        }
        public List<Dictionary<Creature,Creature>> GetAllPossibleDeffenseCombination(Creature[] incommingAttacks, Creature[] validDeffenders)
        {
            List<Dictionary<Creature, Creature>> possiblescenarios = new(4);

            Dictionary<Creature, Creature> scenarioAllSurvive = new();
            foreach (var _attacker in incommingAttacks.OrderByDescending(x => x.CurrentHealth))
            {
               // Console.WriteLine("attacker");
                foreach (var _defender in validDeffenders.Where(x => x.CurrentHealth > _attacker.CurrentAttack))
                {
                    if (scenarioAllSurvive.ContainsValue(_attacker)) break;
                    if (scenarioAllSurvive.ContainsKey(_defender)) continue;
                  //  Console.WriteLine("add defender");
                    scenarioAllSurvive.Add(_defender, _attacker);

                }
            }
            if (scenarioAllSurvive.Count > 0)
            {
                possiblescenarios.Add(scenarioAllSurvive);
            }
            else
            {
                //Console.WriteLine("scenario 1 - odpada");
                possiblescenarios.Add(new());

            }

            Dictionary<Creature, Creature> scenarioDeffUntilEnemyDie = new();
            foreach (var _attacker in incommingAttacks.OrderBy(x => x.CurrentHealth))
            {
            //    Console.WriteLine("attacker");
                foreach (var _defender in validDeffenders.Where(x => x.CurrentAttack <= _attacker.CurrentHealth).OrderByDescending(X => X.CurrentAttack))
                {
                    // Console.WriteLine(_defender.Name);
                    if (scenarioDeffUntilEnemyDie.ContainsKey(_defender)) continue;
                    if (scenarioDeffUntilEnemyDie.Where(x => x.Value == _attacker).Sum(x => x.Key.CurrentAttack) >= _attacker.CurrentHealth) break;
                 //   Console.WriteLine("add defender");
                    scenarioDeffUntilEnemyDie.Add(_defender, _attacker);
                }
                if (scenarioDeffUntilEnemyDie.Where(x => x.Value == _attacker).Sum(x => x.Key.CurrentAttack) < _attacker.CurrentHealth)
                {
                    // bezsensowna smierc, kilka jednostek i tak nie zabije tego atakujacego, usuń te wpisy z listy i przejdz do kolejnego atakuajcegto
                    var todelete = scenarioDeffUntilEnemyDie.Where(x => x.Value == _attacker);
                    foreach (var c in todelete)
                    {
                      //  Console.WriteLine("del");
                        scenarioDeffUntilEnemyDie.Remove(c.Key);
                    }
                }
            }
            if (scenarioDeffUntilEnemyDie.Count > 0)
            {
                possiblescenarios.Add(scenarioDeffUntilEnemyDie);
            }
            else
            {
                //Console.WriteLine("scenario 2 - odpada");
                possiblescenarios.Add(new());

            }

            Dictionary<Creature, Creature> scenarioWinAndSacrificeForDeffenseFromStrongest = new();
            foreach(var option1 in scenarioAllSurvive)
            {
                scenarioWinAndSacrificeForDeffenseFromStrongest.Add(option1.Key,option1.Value);
            }

            //sprawdz czy zostałktos  atakujacy bez blocka
            var attackersleft = incommingAttacks.Except(scenarioWinAndSacrificeForDeffenseFromStrongest.Select(x => x.Value));
            if (attackersleft.Count() > 0)
            {
                var deffendersLeft = validDeffenders.Except(scenarioWinAndSacrificeForDeffenseFromStrongest.Keys);
                if (deffendersLeft.Count() > 0)
                {
                    // wybranie najslabszego zostałęgo i przypisanie go do najmocniejszego
                    var najslabszyDeff = deffendersLeft.OrderBy(x => x.CurrentHealth).ThenBy(x => x.CurrentAttack).First();
                    var najsilniejszyAtkakujacy = attackersleft.OrderByDescending(x => x.CurrentAttack).First();

                    scenarioWinAndSacrificeForDeffenseFromStrongest.Add(najslabszyDeff, najsilniejszyAtkakujacy);
                }
            }
            if (scenarioWinAndSacrificeForDeffenseFromStrongest.Count > 0)
            {
                possiblescenarios.Add(scenarioWinAndSacrificeForDeffenseFromStrongest);
            }
            else
            {
                //Console.WriteLine("scenario 3 - odpada");
                possiblescenarios.Add(new());

            }

            possiblescenarios.Add(new());

            return possiblescenarios;
        }

        public Dictionary<Creature,Creature> AI_RandomDeffendersDeclaration()
        {
            //Console.WriteLine(">>>>>> AI CANT CHOOSE DDEFENDERS ... YET! <<");

            // AI gonna block ONLY WHEN monster can survive attack
            Creature[] validDeffenders = Get_AvailableDeffenders().ToArray();
            Creature[] attackers = _gameEngine.GetAttackerDeclaration();

            var defenseoptions = GetAllPossibleDeffenseCombination(attackers, validDeffenders);
            var selectedDeffOption = defenseoptions[0].Count==0?defenseoptions[3]:defenseoptions[0];
            if(selectedDeffOption.Count > 0)
            {
                foreach (var deffend in selectedDeffOption)
                {
                    string your = $" [You] { deffend.Key.Name} ({ deffend.Key.CurrentAttack}/{ deffend.Key.CurrentHealth})".PadRight(35);
                    string enemy = $" ({ deffend.Value.CurrentAttack}/{ deffend.Value.CurrentHealth}) { deffend.Value.Name} [Enemy]".PadLeft(35);
                    Console.WriteLine($">> Obrona: {your} vs {enemy} ");
                }
            }
            else
            {
                Console.WriteLine(">> Obrona: Przyjęcie obrażeń na klate xD");

            }
            return selectedDeffOption;
        }

        public Creature[] GetCreaturesFromField(int[] indexes, bool validated = false)
        {
            indexes = indexes.Distinct().ToArray();
            if (indexes[0] == -1)
            {
                Console.WriteLine(" >>>>> tym razem bez walki...");
                return Array.Empty<Creature>(); // nie atakujemy wcale wartosc -1
            }
                
            if(validated == true)
            {
                Creature[] _attackersToDeclare = new Creature[indexes.Length];
                for(int i=0;i<indexes.Length;i++)
                {
                    _attackersToDeclare[i] = (Creature)CombatField[indexes[i]];
                    Console.WriteLine($">> Atak :[{CombatField.IndexOf(_attackersToDeclare[i])}] {_attackersToDeclare[i].Name} [{_attackersToDeclare[i].CurrentAttack}/{_attackersToDeclare[i].CurrentHealth}]"); 
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
    }   
}   
