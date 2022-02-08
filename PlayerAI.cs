using MTG_ConsoleEngine.Card_Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTG_ConsoleEngine
{
    public class PlayerAI : PlayerBase
    {
        public PlayerAI(int _number, int _health)
        {
            this.IsAI = true;
            this.PlayerNumberID = _number;
            this.PlayerIndex = _number == 1 ? 0 : 1;

            this.Health = _health;

            if (_number == 1) color = ConsoleColor.Blue;
            else color = ConsoleColor.Green;

            SumAvailableManaFromManaField();
        }
        public override void DealDamage(int value)
        {
            //  Console.BackgroundColor = ConsoleColor.DarkRed;
            //   Console.ForegroundColor = ConsoleColor.White;

            //if (value > 0)
            //{
            //    Console.Write($"Player {PlayerNumberID} otrzymał {value} obrażeń. ");
            //}
            Health -= value;
            // Console.Writeline($"Aktualne HP: {Health}");
            //  Console.ResetColor();
        }
        public override void Heal(int value)
        {
            //   Console.BackgroundColor = ConsoleColor.DarkGreen;
            //   Console.ForegroundColor = ConsoleColor.White;

            //if (value > 0)
            //{
            //    Console.Write($"Player {PlayerNumberID} został uleczony o {value} hp.");
            //}
            Health += value;
            // Console.Writeline($"Aktualne HP: {Health}");
            //  Console.ResetColor();
        }

        public override void DrawCard()
        {
            //   Console.ForegroundColor = color;
            if (Deck.Count > 0)
            {
                var newCard = Deck.First();
                Deck.Remove(newCard);
                Hand.Add(newCard);
                // Console.Writeline($"Player {PlayerNumberID} dobrał karte: {newCard.Name}");
            }
            else
            {
                // Console.Writeline("PRZEGRANA - BRAK KART!");
                Health = -666;
            }
            //  Console.ResetColor();
        }
        public override Creature[] Get_AvailableAttackers()
        {

            var possibleAttackers = CombatField.Where(x => x.IsTapped == false && x is Creature).Select(x => (Creature)x).ToArray();
            if (possibleAttackers.Any() == false)
            {
                //// Console.Writeline("Brak posiadanych jednostek gotowych do ataku");
                return Array.Empty<Creature>();
            }
            //// Console.Writeline("dostępne kreatury do ataku: ");
            //possibleAttackers.ForEach(x =>
            //        Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
            //    );

            return possibleAttackers;
        }
        public override List<Creature> Get_AvailableDeffenders()
        {
            var possibleDefenders = CombatField.Where(x => x.IsTapped == false && x is Creature).Select(x => (Creature)x).ToList();
            if (possibleDefenders.Any() == false) return new();

            //  // Console.Writeline("Dostępne jednostki do obrony");
            //  possibleDefenders.ForEach(x => 
            //      Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.PlayerNumberID} / {x.Name} (atk:{x.CurrentAttack}/{x.CurrentHealth})\n")
            //  );
            return possibleDefenders;

        }

      

       
        public override bool PlayCardFromHand(CardBase card, List<CardBase> landsToPay)
        {
            // Console.ForegroundColor = color;
            // // Console.Writeline($">>>>>>>>>>>>>>>>> Gracz {PlayerNumberID} zagrywa kartą {card.Name}");
            // (bool status, int playerIndex, int creatureIndex) playerResponse = (false, -1, -1);

            switch (card)
            {
                case Creature c:
                    if (!c.CardSpecialActions.Any(x => x.description == "Haste"))
                    {
                        c.IsTapped = true;
                    }
                    CombatField.Add(card);
                    card.UseSpecialAction(ActionType.Play);
                    break;

                case Land:
                    if (IsLandPlayedThisTurn == true) return false;
                    ManaField.Add(card);
                    card.UseSpecialAction(ActionType.Play);
                    break;

                case Enchantment e:
                    List<CardBase> availableTargets2 = _gameEngine.GetValidTargetsForCardType(e, PlayerIndex);
                    if (availableTargets2.Count == 0) return false;

                    break;

                case Instant i:
                    List<CardBase> availableTargets = _gameEngine.GetValidTargetsForCardType(i, PlayerIndex);
                    if (availableTargets.Count == 0) return false;

                    break;
            }
            //// Console.Writeline("zapłąc za karte / usun ją z ręki");
            landsToPay.ForEach(c => c.IsTapped = true);
            RefreshManaStatus();
            Hand.Remove(card);

            //Console.ResetColor();
            return true;
        }
        public override void DisplayPlayerField() => TableHelpers.DisplayFieldTable(_gameEngine, color, CombatField, PlayerNumberID);

        protected IEnumerable<int[]> Combinations(int k, int n)
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

        public void AI_RemoveOneCardFromHand()
        {
            /*_AI_*/ // remove one random card if total holding > 7
            int randomIndexToRemove = rand.Next(1, this.Hand.Count);
            CardBase card = this.Hand[randomIndexToRemove];
            //// Console.Writeline("LOSOWO! Wyrzuciłeś " + card.Name);
            this.Graveyard.Add(card);
            this.Hand.Remove(card);
        }
        public Dictionary<Creature, Creature> AI_RandomDeffendersDeclaration()
        {
            // SEMI RANDOM ! ~ GUIDED 
            // TODO: 
            // AI gonna block ONLY WHEN monster can survive attack
            Creature[] validDeffenders = Get_AvailableDeffenders().ToArray();
            Creature[] attackers = _gameEngine.GetAttackerDeclaration();

            var defenseoptions = GetAllPossibleDeffenseCombination(attackers, validDeffenders);
            Dictionary<Creature, Creature> selectedDeffOption;
            if (defenseoptions[0].Count != 0)
            { selectedDeffOption = defenseoptions[0]; }
            else if (defenseoptions[1].Count != 0)
            { selectedDeffOption = defenseoptions[1]; }
            else selectedDeffOption = defenseoptions[3];

            return selectedDeffOption;
        }

        public  List<int[]> GetAllPossibleAttackCombinationsIndexes(Creature[] _availableTargets)
        {
            List<int[]> result = new();
            //// Console.Writeline("count "+availableTargets.Count);
            if (_availableTargets.Length == 0)
                return new List<int[]> { new int[1] { -1 } };

            int[] arr = _availableTargets.Select(x => CombatField.IndexOf(x)).ToArray();

            for (int i = arr.Length - 1; i >= 0; i--)
            {
                int n = _availableTargets.Length;
                int k = _availableTargets.Length - i;

                result.Add(new int[k + 1]);
                foreach (int[] combo in Combinations(k, n))
                {
                    //  // Console.Writeline(string.Join(", ", combo));
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
        public void AI_PlayLandCardIfPossible()
        {
            /*_AI_*/ // auto pick land if possible
            CardBase firstLandFromHAnd = Hand.FirstOrDefault(x => x is Land);
            if (firstLandFromHAnd != null && IsLandPlayedThisTurn == false)
            {
                //// Console.Writeline("Auto place Land");
                this.PlayCardFromHand(firstLandFromHAnd, new());
            }
        }
        public void AI_PlayRandomInstatFromHand()
        {

            if (this.Hand.Any(x => (x is Instant instant) || (x is Enchantment)))
            {
                // GetAllPossibleSpellCardsCombinations();  // <- too powerfull xD
                //----------------------------------------------------------------------
                GetPureRandomSkillsCombo(); // pure randomized fit in mana cost

            }

        }
        public void AI_MakePlaysWithRandomCardFromHand()
        {
            AI_PlayLandCardIfPossible();
            // hard coded play creature if can
            while (true)
            {
                // get list available draws
                RefreshManaStatus();
                Dictionary<CardBase, (bool result, List<CardBase> landsToPay)> CurrentHandDetailedData = new();
                Hand.ForEach(x => CurrentHandDetailedData.Add(x,x.CheckAvailability()));

                var creaturesOnly = CurrentHandDetailedData.Where(X => X.Value.result == true && X.Key is Creature);

                if (!creaturesOnly.Any()) break;

                var randomCreatureFromHand = creaturesOnly.ElementAt(rand.Next(0, creaturesOnly.Count()));

                this.PlayCardFromHand(randomCreatureFromHand.Key, randomCreatureFromHand.Value.landsToPay);
            }
        }
        public Creature[] AI_RandomAttackDeclaration()
        {
            /*_AI_*/ // random attack sequention from available

            Creature[] availableTargets = this.Get_AvailableAttackers();
            List<int[]> randomIndexesList = this.GetAllPossibleAttackCombinationsIndexes(availableTargets);
            //// Console.Writeline("\t >> possible attack combinations: ");
            //foreach(var combination in randomIndexesList)
            // {
            //     // Console.Writeline("\t\t - "+String.Join(",", combination));
            // }
            int[] inputArr = randomIndexesList[rand.Next(randomIndexesList.Count)];

            return this.GetCreaturesFromField(inputArr, true).ToArray();
            //// Console.Writeline($">>>>>> Attackers declared by Player {PlayerNumberID}: "+String.Join("\n- ", _gameEngine.DeclaredAttackers.Select(x=>x.Name)));
        }
        public List<Dictionary<Creature, Creature>> GetAllPossibleDeffenseCombination(Creature[] incommingAttacks, Creature[] validDeffenders)
        {
            List<Dictionary<Creature, Creature>> possiblescenarios = new(4);

            Dictionary<Creature, Creature> scenarioAllSurvive = new();
            foreach (var _attacker in incommingAttacks.OrderByDescending(x => x.CurrentHealth))
            {
                // // Console.Writeline("attacker");
                foreach (var _defender in validDeffenders.Where(x => x.CurrentHealth > _attacker.CurrentAttack))
                {
                    if (scenarioAllSurvive.ContainsValue(_attacker)) break;
                    if (scenarioAllSurvive.ContainsKey(_defender)) continue;
                    //  // Console.Writeline("add defender");
                    scenarioAllSurvive.Add(_defender, _attacker);

                }
            }
            if (scenarioAllSurvive.Count > 0)
            {
                possiblescenarios.Add(scenarioAllSurvive);
            }
            else
            {
                //// Console.Writeline("scenario 1 - odpada");
                possiblescenarios.Add(new());

            }

            Dictionary<Creature, Creature> scenarioDeffUntilEnemyDie = new();
            foreach (var _attacker in incommingAttacks.OrderBy(x => x.CurrentHealth))
            {
                //    // Console.Writeline("attacker");
                foreach (var _defender in validDeffenders.Where(x => x.CurrentAttack <= _attacker.CurrentHealth).OrderByDescending(X => X.CurrentAttack))
                {
                    // // Console.Writeline(_defender.Name);
                    if (scenarioDeffUntilEnemyDie.ContainsKey(_defender)) continue;
                    if (scenarioDeffUntilEnemyDie.Where(x => x.Value == _attacker).Sum(x => x.Key.CurrentAttack) >= _attacker.CurrentHealth) break;
                    //   // Console.Writeline("add defender");
                    scenarioDeffUntilEnemyDie.Add(_defender, _attacker);
                }
                if (scenarioDeffUntilEnemyDie.Where(x => x.Value == _attacker).Sum(x => x.Key.CurrentAttack) < _attacker.CurrentHealth)
                {
                    // bezsensowna smierc, kilka jednostek i tak nie zabije tego atakujacego, usuń te wpisy z listy i przejdz do kolejnego atakuajcegto
                    var todelete = scenarioDeffUntilEnemyDie.Where(x => x.Value == _attacker);
                    foreach (var c in todelete)
                    {
                        //  // Console.Writeline("del");
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
                //// Console.Writeline("scenario 2 - odpada");
                possiblescenarios.Add(new());

            }

            Dictionary<Creature, Creature> scenarioWinAndSacrificeForDeffenseFromStrongest = new();
            foreach (var option1 in scenarioAllSurvive)
            {
                scenarioWinAndSacrificeForDeffenseFromStrongest.Add(option1.Key, option1.Value);
            }

            //sprawdz czy zostałktos  atakujacy bez blocka
            var attackersleft = incommingAttacks.Except(scenarioWinAndSacrificeForDeffenseFromStrongest.Select(x => x.Value));
            if (attackersleft.Any())
            {
                var deffendersLeft = validDeffenders.Except(scenarioWinAndSacrificeForDeffenseFromStrongest.Keys);
                if (deffendersLeft.Any())
                {
                    // wybranie najslabszego zostałęgo i przypisanie go do najmocniejszego
                    var najslabszyDeff = deffendersLeft.OrderBy(x => x.CurrentHealth).ThenBy(x => x.CurrentAttack).First();
                    var najsilniejszyAtkakujacy = attackersleft.OrderByDescending(x => x.CurrentAttack).First();

                    scenarioWinAndSacrificeForDeffenseFromStrongest.Add(najslabszyDeff, najsilniejszyAtkakujacy);
                }
            }
            if (scenarioWinAndSacrificeForDeffenseFromStrongest.Any())
            {
                possiblescenarios.Add(scenarioWinAndSacrificeForDeffenseFromStrongest);
            }
            else
            {
                //// Console.Writeline("scenario 3 - odpada");
                possiblescenarios.Add(new());

            }

            possiblescenarios.Add(new());

            return possiblescenarios;
        }

        public void GetAllPossibleSpellCardsCombinations()
        {
            List<KeyValuePair<CardBase, (bool result, List<CardBase> landsToPay)>> AvailableCardSpellInHand = new();
            // print available hand
            // Console.WriteLine("Print hand skills list");
            Dictionary<CardBase, (bool result, List<CardBase> landsToPay)> CurrentHandDetailedData = new();
            Hand.ForEach(x => CurrentHandDetailedData.Add(x, x.CheckAvailability()));
            AvailableCardSpellInHand = CurrentHandDetailedData.Where(X => X.Value.result && X.Key is not Creature && X.Key is not Land).ToList();

            Console.WriteLine(" ------------------------------------------------------------------------------- ");

            Console.WriteLine("|  [ID]  |  [Can be used?]  |           [Name]           |     [Mana cost]      |");
            Console.WriteLine("|--------|------------------|----------------------------|----------------------|");
            foreach (var card in AvailableCardSpellInHand)
            {
                Console.WriteLine($"|    {card.Key.ID,2}  | {card.Value.result,16} | {card.Key.Name,26} | {card.Key.ManaCostString,20} |");
            }
            if (AvailableCardSpellInHand.Count == 0)
            {
                Console.WriteLine("        Brak kart skili które mołbys użyć, lub nie stać cie na żadną");
            }
            Console.WriteLine(" ------------------------------------------------------------------------------- ");

            List<int[]> OUTPUT = new List<int[]>();
            int[] arr = AvailableCardSpellInHand.Select(x => x.Key.ID).ToArray();
            List<CardBase> playableSkillsFromHand = AvailableCardSpellInHand.Select(x => x.Key).ToList();
            foreach (var item in arr)
            {
                int[] parentState = new int[1] { item };
                int[] temp = arr;
                int[] itemsLeft = new int[temp.Length - 1];

                int index = 0;
                foreach (int leftitem in temp)
                {
                    if (leftitem == parentState[parentState.Length - 1]) continue;

                    itemsLeft[index++] = leftitem;
                }

                RecurPowerSet(ref OUTPUT, parentState, itemsLeft, playableSkillsFromHand);
            }

            Console.WriteLine(OUTPUT.Count);
        }
   

        public List<(CardBase skillCard, CardBase target)> GetPureRandomSkillsCombo()
        {
            List<KeyValuePair<CardBase, (bool result, List<CardBase> landsToPay)>> AvailableCardSpellInHand = new();
            // print available hand
            // Console.WriteLine("Print hand skills list");
            Dictionary<CardBase, (bool result, List<CardBase> landsToPay)> CurrentHandDetailedData = new();
            Hand.ForEach(x => CurrentHandDetailedData.Add(x, x.CheckAvailability()));
            AvailableCardSpellInHand = CurrentHandDetailedData.Where(X => X.Value.result && X.Key is not Creature && X.Key is not Land).ToList();

            //////Console.WriteLine(" ------------------------------------------------------------------------------- ");

            //////Console.WriteLine("|  [ID]  |  [Can be used?]  |           [Name]           |     [Mana cost]      |");
            //////Console.WriteLine("|--------|------------------|----------------------------|----------------------|");
            //////foreach (var card in AvailableCardSpellInHand)
            //////{
                //////Console.WriteLine($"|    {card.Key.ID,2}  | {card.Value.result,16} | {card.Key.Name,26} | {card.Key.ManaCostString,20} |");
            //////}
            //////if (AvailableCardSpellInHand.Count == 0)
            //////{
                //////Console.WriteLine("         Brak kart skili któe mołbys użyć, lub nie stać cie na żadną");
            //////}
            //////Console.WriteLine(" ------------------------------------------------------------------------------- ");

            //List
            List<Dictionary<int, int>> skillsToSum = new List<Dictionary<int, int>>();
            List<CardBase> SkillsToPlay = new List<CardBase>();
            int randomIndex = -1;
            while (true)
            {
                randomIndex = rand.Next(-1, AvailableCardSpellInHand.Count);
                if (randomIndex < 0)
                {
                  //  Console.WriteLine("koniec szukania, lub nie granie wcale");
                    break;
                }
                CardBase _randomCard = AvailableCardSpellInHand.ElementAt(randomIndex).Key;

                if (SkillsToPlay.Contains(_randomCard))
                {
                    continue; // nie ma powtarzania, nie da rady rzucic 2x TEJ samej karty
                }

                skillsToSum.Add(_randomCard.ManaCost);

                // podsumowanie i sprawdzenie czy nas stać na ten zestaw
                int[] summed = SimpleSumMana(skillsToSum);
                var costcheck = SimpleManaCheck(summed);
                if (costcheck == false)
                {
                   // Console.WriteLine("pałka się przegła, uzyjesz skili ktore wczesniej wylosowales");
                    break;
                }

                //////Console.WriteLine("losowa kartaID:"+ _randomCard.ID);
                SkillsToPlay.Add(_randomCard);
            }

            List<(CardBase skillCard, CardBase target)> randomizedOutput = new();
            foreach(CardBase card in SkillsToPlay)
            {
                //////Console.WriteLine($"Karta skilla: {card.Name}, dostępne cele:");
                List<CardBase> targets = _gameEngine.GetValidTargetsForCardType(card, PlayerIndex);
                if (targets.Count == 0) continue;
                //////foreach(var target in targets)
                //////{
                    //////Console.WriteLine($"\t - [ {card.TargetsType.ToString(),5} ] {target.Name,20}");
                //////}

                randomizedOutput.Add((card, targets.ElementAt(rand.Next(0, targets.Count))));
            }

            //////Console.WriteLine("\n FINALNIE CZAS NA ROZLOSOWANIE CELÓW, czyli to co tygryski lubią najbardziej xD\n");
            
            //////foreach(var pair in randomizedOutput)
            //////{
                //////Console.WriteLine($"Karta: {pair.skillCard.Name,20} => [cel: {pair.skillCard.TargetsType,5}] {pair.target.Name,-15}");
            //////}
            return randomizedOutput;
        }

    }
}
