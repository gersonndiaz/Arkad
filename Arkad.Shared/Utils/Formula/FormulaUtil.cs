using NCalc;
using System.Globalization;

namespace Arkad.Shared.Utils.Formula
{
    public class FormulaUtil
    {
        /// <summary>
        /// Obtiene listado de Operadores Disponibles
        /// </summary>
        /// <returns></returns>
        public static List<FormulaItem> AvailableOperators()
        {
            List<FormulaItem> operators = new List<FormulaItem>
            {
                new FormulaItem { Id = "plus", Content = "+" },
                new FormulaItem { Id = "minus", Content = "-" },
                new FormulaItem { Id = "multiply", Content = "*" },
                new FormulaItem { Id = "divide", Content = "/" },
                new FormulaItem { Id = "leftParen", Content = "(" },
                new FormulaItem { Id = "rightParen", Content = ")" },
            };

            return operators;
        }

        /// <summary>
        /// Valida la fórmula ingresada
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string ValidateAndReplaceFormula(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                throw new InvalidOperationException("La fórmula está vacía.");
            }

            var tokens = formula.Split(' ');
            var validOperators = AvailableOperators().Select(x => x.Content).ToArray();
            var lastTokenWasOperator = true;
            var parenCount = 0;

            Random random = new Random();
            Dictionary<string, double> variableValues = new Dictionary<string, double>();

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (token == "(")
                {
                    parenCount++;
                    lastTokenWasOperator = true; // Un operador puede seguir a un paréntesis de apertura
                }
                else if (token == ")")
                {
                    parenCount--;
                    if (parenCount < 0)
                    {
                        throw new InvalidOperationException("Los paréntesis están desbalanceados.");
                    }
                    lastTokenWasOperator = false; // Un operador no puede seguir inmediatamente a un paréntesis de cierre
                }
                else if (validOperators.Contains(token))
                {
                    if (lastTokenWasOperator)
                    {
                        throw new InvalidOperationException("La fórmula contiene operadores en posiciones inválidas.");
                    }
                    lastTokenWasOperator = true; // Marcar que un operador acaba de ser encontrado
                }
                else if (token.StartsWith("[") && token.EndsWith("]"))
                {
                    if (!lastTokenWasOperator)
                    {
                        throw new InvalidOperationException("La fórmula contiene operadores en posiciones inválidas.");
                    }
                    lastTokenWasOperator = false; // Un operador debe seguir a una variable

                    // Generar un valor aleatorio con 3 decimales para cada variable
                    if (!variableValues.ContainsKey(token))
                    {
                        double randomValue = Math.Round(random.NextDouble() * 100, 3);
                        variableValues[token] = randomValue;
                    }
                }
                else if (double.TryParse(token, out _))
                {
                    // Si el token es un número, lo dejamos pasar sin hacer nada especial
                    if (!lastTokenWasOperator)
                    {
                        throw new InvalidOperationException("La fórmula contiene operadores en posiciones inválidas.");
                    }
                    lastTokenWasOperator = false; // Un operador debe seguir a un número
                }
                else
                {
                    throw new InvalidOperationException($"Token inválido encontrado: {token}");
                }
            }

            if (parenCount != 0)
            {
                throw new InvalidOperationException("Los paréntesis están desbalanceados.");
            }

            if (lastTokenWasOperator)
            {
                throw new InvalidOperationException("La fórmula no puede terminar con un operador.");
            }

            // Reemplazar las variables con sus valores aleatorios formateados a 3 decimales
            var replacedFormula = formula;
            foreach (var kvp in variableValues)
            {
                replacedFormula = replacedFormula.Replace(kvp.Key, kvp.Value.ToString("F3", CultureInfo.InvariantCulture));
            }

            // Evaluar la fórmula reemplazada utilizando NCalc
            var expression = new Expression(replacedFormula);

            try
            {
                var evaluationResult = expression.Evaluate();

                if (evaluationResult == null)
                {
                    return $"[ERROR]: La evaluación devolvió un resultado nulo.";
                }

                if (evaluationResult is double result)
                {
                    string formattedResult = result.ToString("F3", CultureInfo.InvariantCulture);
                    return $"[OK]: {replacedFormula} = {formattedResult}";
                }
                else
                {
                    return $"[ERROR]: El resultado de la evaluación no es un número válido.";
                }
            }
            catch (EvaluationException ex)
            {
                return $"[ERROR]: {ex.Message}";
            }
        }

        public static float ValidateAndReplaceFormula(string formula, List<FormulaVariable> variables)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                throw new InvalidOperationException("La fórmula está vacía.");
            }

            Console.WriteLine($"[ValidateAndReplaceFormula] Fórmula original: {formula}");

            // Imprimir todos los valores en la lista de variables antes de procesar la fórmula
            Console.WriteLine("[ValidateAndReplaceFormula] Contenido de la lista de variables:");
            foreach (var variable in variables)
            {
                Console.WriteLine($"[ValidateAndReplaceFormula] ID: {variable.Id}, Valor: {variable.Value}");
            }

            var tokens = formula.Split(' ');
            var validOperators = AvailableOperators().Select(x => x.Content).ToArray();
            var lastTokenWasOperator = true;
            var parenCount = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                Console.WriteLine($"[ValidateAndReplaceFormula] Procesando token: {token}");

                if (token == "(")
                {
                    parenCount++;
                    lastTokenWasOperator = true;
                    Console.WriteLine($"[ValidateAndReplaceFormula] Paréntesis de apertura encontrado. Contador de paréntesis: {parenCount}");
                }
                else if (token == ")")
                {
                    parenCount--;
                    if (parenCount < 0)
                    {
                        throw new InvalidOperationException("Los paréntesis están desbalanceados.");
                    }
                    lastTokenWasOperator = false;
                    Console.WriteLine($"[ValidateAndReplaceFormula] Paréntesis de cierre encontrado. Contador de paréntesis: {parenCount}");
                }
                else if (validOperators.Contains(token))
                {
                    if (lastTokenWasOperator)
                    {
                        throw new InvalidOperationException("La fórmula contiene operadores en posiciones inválidas.");
                    }
                    lastTokenWasOperator = true;
                    Console.WriteLine($"[ValidateAndReplaceFormula] Operador válido encontrado: {token}");
                }
                else if (token.StartsWith("[") && token.EndsWith("]"))
                {
                    if (!lastTokenWasOperator)
                    {
                        throw new InvalidOperationException("La fórmula contiene operadores en posiciones inválidas.");
                    }
                    lastTokenWasOperator = false;

                    var id = token.Trim('[', ']');
                    Console.WriteLine($"[ValidateAndReplaceFormula] ID encontrado: {id}");

                    // Buscar el valor en la lista de variables
                    var variable = variables.FirstOrDefault(v => v.Id == id);
                    if (variable != null)
                    {
                        tokens[i] = variable.Value.ToString("F3", CultureInfo.InvariantCulture);
                        Console.WriteLine($"[ValidateAndReplaceFormula] Reemplazando ID '{id}' con valor: {variable.Value}");
                    }
                    else
                    {
                        Console.WriteLine($"[ValidateAndReplaceFormula] Error: El ID '{id}' no tiene un valor asociado en la lista.");
                        throw new InvalidOperationException($"El ID '{id}' no tiene un valor asociado en la lista.");
                    }
                }
                else if (double.TryParse(token, out _))
                {
                    if (!lastTokenWasOperator)
                    {
                        throw new InvalidOperationException("La fórmula contiene operadores en posiciones inválidas.");
                    }
                    lastTokenWasOperator = false;
                    Console.WriteLine($"[ValidateAndReplaceFormula] Número encontrado: {token}");
                }
                else
                {
                    throw new InvalidOperationException($"Token inválido encontrado: {token}");
                }
            }

            if (parenCount != 0)
            {
                throw new InvalidOperationException("Los paréntesis están desbalanceados.");
            }

            if (lastTokenWasOperator)
            {
                throw new InvalidOperationException("La fórmula no puede terminar con un operador.");
            }

            // Unir los tokens en la fórmula final reemplazada
            var replacedFormula = string.Join(" ", tokens);
            Console.WriteLine($"[ValidateAndReplaceFormula] Fórmula después del reemplazo: {replacedFormula}");

            // Evaluar la fórmula reemplazada utilizando NCalc
            var expression = new Expression(replacedFormula);

            try
            {
                var evaluationResult = expression.Evaluate();
                Console.WriteLine($"[ValidateAndReplaceFormula] Resultado de la evaluación: {evaluationResult}");

                if (evaluationResult == null)
                {
                    throw new InvalidOperationException("La evaluación devolvió un resultado nulo.");
                }

                if (evaluationResult is double result)
                {
                    return (float)result;
                }
                else
                {
                    throw new InvalidOperationException("El resultado de la evaluación no es un número válido.");
                }
            }
            catch (EvaluationException ex)
            {
                Console.WriteLine($"[ValidateAndReplaceFormula] Error al evaluar la fórmula: {ex.Message}");
                throw new InvalidOperationException($"Error al evaluar la fórmula: {ex.Message}");
            }
        }
    }

    public class FormulaItem
    {
        public string Id { get; set; }
        public string Content { get; set; }
    }

    public class FormulaVariable
    {
        public string Id { get; set; }
        public double Value { get; set; }
    }

}
