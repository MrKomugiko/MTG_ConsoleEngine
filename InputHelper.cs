using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public static class InputHelper
    {
        public static Dictionary<Creature, Creature> Input_DefendersDeclaration(Engine _gameEngine, List<Creature> _Attackers , List<Creature> _legalDeffenders, int _playerIndex)
        {
            Console.WriteLine("[ Example input: 0-0, 0-1, 1-2 ]\n[ deffender - attacker, ...-...]");   
            Dictionary<Creature, Creature> _deffendersToDeclare = new();
            while (true)
            { 
                
                TryAgain:
                string input = Console.ReadLine()??"";

                if (String.IsNullOrEmpty(input)) 
                {
                    Console.WriteLine("Accepted. Go to next turn.");
                    return _deffendersToDeclare;
                }
                                                            
                var inputArr = input.Split(",");            // 0-0, 0-1, 0-2
                foreach(var pair in inputArr)               // [0-0],[0-1],[0-2]
                {                                           
                    int attackerID, deffenderID;
                    string[] input_splitted = pair.Split("-");  // [0],[0]
                    if(input_splitted.Length<2)
                    {
                        Console.WriteLine("zły format use 0-0,1-0,2-0  (deffender VS attacker), wprowadz ponownie:");
                        goto TryAgain;

                    }
                    // sprawdzenie czy dwa podane parametry sa liczbą 
                    if(Int32.TryParse(input_splitted[1], out attackerID) && Int32.TryParse(input_splitted[0],out deffenderID))
                    {   
                        // sprawdzenie czy zakresy podanych ID nie wykraczaja poza dozwolone granice liczby mobkow
                        if(attackerID<_Attackers.Count && deffenderID < _legalDeffenders.Count)
                        {
                            if(_Attackers.Contains(_Attackers[attackerID]) && _gameEngine.Players[_playerIndex].CombatField.Contains(_legalDeffenders[deffenderID]))
                            {
                                var deffender = (Creature)_legalDeffenders[deffenderID];
                                var attacker = _Attackers[attackerID];
                                
                                // sprawdzenie czy stworek ktorym chcemy sie broni nie jest juz tapnięty
                                if(deffender.isTapped) {
                                    Console.WriteLine("blocker pominięty, jest juz tapnięty");
                                    continue;
                                }

                                // sprawdzenie czy wpis obrony przeciw temu atakujacemu (jako klucz) juz widnieje
                                if(_deffendersToDeclare.ContainsKey(deffender)== false)
                                {
                                    // pierwsze wystąpienie obrony przeciw temu potworkowi, dodanie go wraz z naszym obrońcą
                                    _deffendersToDeclare.Add(deffender,attacker);
                                    Console.WriteLine($"dodano: {deffender.Name} vs {attacker.Name}");
                                }
                                else
                                {
                                    Console.WriteLine("blocker pominięty, Nie można przypisać potworka do obrony przeciw dwóm różnym napastnikom");
                                }
                            }

                            if(_deffendersToDeclare.Count == _legalDeffenders.Where(x=>((Creature)x).isTapped == false).Count())
                            {
                                Console.WriteLine("Brak więcej stworkow do rozdysponowania, next turn");
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("pominięto, błędny index");
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Wrong input string, type correct syntax example: 0-0,1-2,0-1 \n only one creature can be assigned to ddeffense, \nyou can assign multiple creatures to deffend vs one attacker");
                        continue;
                    }

                    //---------------------------------------------------------------------------------------------------------
                            // if(input_splitted.Length < 2) 
                            // {
                            //     Console.WriteLine("Wrong format, type example 0-0 0(EnemyID)-0(My CombatField monster id)");
                            //     continue;
                            // }
                            
                            // if(Int32.TryParse(input_splitted[0], out attackerID)&&Int32.TryParse(input_splitted[1],out deffenderID))
                            // {
                            //     // sukces jeden i drugi sa poprawnymi liczbami
                            //     // sprawdzenie zakresów 
                            //     if(attackerID<_Attackers.Count-1 && deffenderID < _CombatField.Count-1)
                            //     {
                            //         // sprawdzenie czy id obrońcy juz występuje w zadeklarowanych
                            //         var deffender = (Creature)_CombatField[deffenderID];
                            //         var attacker = _Attackers[attackerID];

                            //         // sprawdzenie czy stworek ktorym chcemy sie broni nie jest juz tapnięty
                            //         if(deffender.isTapped) {
                            //             Console.WriteLine("wybierz innego blockera, ten jest niesostępny");
                            //             continue;
                            //         }

                            //         // sprawdzenie czy wpis obrony przeciw temu atakujacemu (jako klucz) juz widnieje
                            //         if(_deffendersToDeclare.ContainsKey(deffender)== false)
                            //         {
                            //             // pierwsze wystąpienie obrony przeciw temu potworkowi, dodanie go wraz z naszym obrońcą
                            //             _deffendersToDeclare.Add(deffender, attacker);
                            //             Console.WriteLine($"dodano: {attacker.Name} vs {deffender.Name}");
                            //         }
                            //         else
                            //         {
                            //             Console.WriteLine("Nie można przypisać potworka do obrony przeciw dwóm różnym napastnikom");
                            //         }

                            //         if(_deffendersToDeclare.Count == _CombatField.Where(x=>((Creature)x).isTapped == false).Count())
                            //         {
                            //             Console.WriteLine("Brak więcej stworkow do rozdysponowania, next turn");
                            //             break;
                            //         }
                            //     }
                            // }
                }

                return _deffendersToDeclare;
            }
        }
        public static (bool status, int playerIndex, int creatureIndex) Input_SinglePairPlayerMonster(Engine _gameEngine)
        {
            Console.WriteLine("[ Example input: 0-0 ] [ TargetPlayer - TargetCreature ]");
            int choosenIndexField, choosenIndexCreature;
            while(true)
            {
                var input = Console.ReadLine()??"";
                var inputarr = input.Split("-");
                if(String.IsNullOrEmpty(input)) 
                {
                    Console.WriteLine("cancel operation of assignation enchantment card");
                        return (false, -1, -1);
                }
                if(inputarr.Length<2) 
                {
                    Console.WriteLine("niepoprawny/nie pełny format : <side combat land number> - <index of monster from this area> ex. 1-0");
                    continue;
                }
                if(Int32.TryParse(inputarr[0], out choosenIndexField) == false) 
                {
                    Console.WriteLine("wprowadz poprawny numer strony 0 lub 1, sprobuj ponownie");
                    continue;
                }
                if(choosenIndexField > 1  || choosenIndexField < 0) 
                {
                    Console.WriteLine("niepoprawny numer strony, wprowadz 0 dla swojej czesci lub 1 dla przeciwnika, spróbuj ponownie");
                    continue;   
                }
                if(Int32.TryParse(inputarr[1], out choosenIndexCreature) == false) 
                {
                    Console.WriteLine("niepoprawny index karty z pola, sprobuj ponownie");
                    continue;
                }
                if(choosenIndexCreature > 0 && choosenIndexCreature > _gameEngine.Players[choosenIndexField].CombatField.Count-1) 
                {
                    Console.WriteLine($"niepoprawny numer karty potworka, podaj wartosc od 0 do {_gameEngine.Players[choosenIndexField].CombatField.Count-1}, spróbuj ponownie");
                    continue;   
                }

                return (true, choosenIndexField, choosenIndexCreature);
            }
        }
    
    }
}