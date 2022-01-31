using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public static class InputHelper
    {
        
        public static Dictionary<Creature, Creature> Input_DefendersDeclaration(List<Creature> _Attackers , List<CardBase> _CombatField)
        {
            Console.WriteLine("'cancel' = cancel/skip\n ENTER = accept/skip");
            Dictionary<Creature, Creature> _deffendersToDeclare = new();
            while (true)
            { 
                string input = Console.ReadLine() ?? "".Trim();

                if (input.ToLower() == "cancel") 
                {
                    Console.WriteLine("Selections canceled. Turn skipped.");
                    return _deffendersToDeclare;
                }
                if (String.IsNullOrEmpty(input)) 
                {
                    Console.WriteLine("Accepted. Go to next turn.");
                    return _deffendersToDeclare;
                }

                if(input.Length<3){
                    Console.WriteLine("Wrong input string, type correct syntax example: 0-0");
                } // minumum 0-0

                string[] input_splitted = input.Split("-");                                      

                if(input_splitted.Length < 2) 
                {
                    Console.WriteLine("Wrong format, type example 0-0 0(EnemyID)-0(My CombatField monster id)");
                    continue;
                }

                int firstInput=-1, secondInput =-1;
                if(Int32.TryParse(input_splitted[0], out firstInput)&&Int32.TryParse(input_splitted[1],out secondInput))
                {
                    // sukces jeden i drugi sa poprawnymi liczbami
                    // sprawdzenie zakresów 
                    if(firstInput<_Attackers.Count && secondInput < _CombatField.Count)
                    {
                        // sprawdzenie czy id obrońcy juz występuje w zadeklarowanych
                        var deffender = (Creature)_CombatField[secondInput];
                        var attacker = _Attackers[firstInput];

                        // sprawdzenie czy stworek ktorym chcemy sie broni nie jest juz tapnięty
                        if(deffender.isTapped) {
                            Console.WriteLine("wybierz innego blockera, ten jest niesostępny");
                            continue;
                        }

                        // sprawdzenie czy wpis obrony przeciw temu atakujacemu (jako klucz) juz widnieje
                        if(_deffendersToDeclare.ContainsKey(deffender)== false)
                        {
                            // pierwsze wystąpienie obrony przeciw temu potworkowi, dodanie go wraz z naszym obrońcą
                            _deffendersToDeclare.Add(deffender, attacker);
                            Console.WriteLine($"dodano: {attacker.Name} vs {deffender.Name}");
                        }
                        else
                        {
                            Console.WriteLine("Nie można przypisać potworka do obrony przeciw dwóm różnym napastnikom");
                        }

                        if(_deffendersToDeclare.Count == _CombatField.Where(x=>((Creature)x).isTapped == false).Count())
                        {
                            Console.WriteLine("Brak więcej stworkow do rozdysponowania, next turn");
                            break;
                        }
                    }
                }
            }

            return _deffendersToDeclare;
        }
    }
}