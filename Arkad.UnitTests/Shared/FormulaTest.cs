using Arkad.Shared.Utils.Formula;

namespace Arkad.UnitTests.Shared
{
    public class FormulaTest
    {
        [Fact]
        public void AvailableOperators_ShouldReturnExpectedOperators()
        {
            // Act
            var operators = FormulaUtil.AvailableOperators();

            // Assert
            Assert.NotNull(operators);
            Assert.Equal(6, operators.Count); // Deben ser 6 operadores
            Assert.Contains(operators, o => o.Content == "+");
            Assert.Contains(operators, o => o.Content == "-");
            Assert.Contains(operators, o => o.Content == "*");
            Assert.Contains(operators, o => o.Content == "/");
            Assert.Contains(operators, o => o.Content == "(");
            Assert.Contains(operators, o => o.Content == ")");
        }

        [Fact]
        public void ValidateAndReplaceFormula_ValidFormulaWithoutVariables_ShouldReturnExpectedResult()
        {
            // Arrange
            var formula = "2 + 3";

            // Act
            var result = FormulaUtil.ValidateAndReplaceFormula(formula);

            // Assert
            Assert.Contains("[OK]:", result);
            Assert.Contains("= 5", result); // Resultado esperado
        }

        [Fact]
        public void ValidateAndReplaceFormula_WithVariables_ShouldReplaceAndEvaluateCorrectly()
        {
            // Arrange
            var formula = "[x] + [y] * 2";
            var variables = new List<FormulaVariable>
                            {
                                new FormulaVariable { Id = "x", Value = 5 },
                                new FormulaVariable { Id = "y", Value = 10 }
                            };

            // Act
            var result = FormulaUtil.ValidateAndReplaceFormula(formula, variables);

            // Assert
            Assert.Equal(25, result); // (5 + 10 * 2) = 25
        }

        [Fact]
        public void ValidateAndReplaceFormula_UnbalancedParentheses_ShouldThrowException()
        {
            // Arrange
            var formula = "( 2 + 3";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                FormulaUtil.ValidateAndReplaceFormula(formula)
            );

            Assert.Equal("Los paréntesis están desbalanceados.", exception.Message);
        }

        [Fact]
        public void ValidateAndReplaceFormula_EmptyFormula_ShouldThrowException()
        {
            // Arrange
            var formula = "";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                FormulaUtil.ValidateAndReplaceFormula(formula)
            );

            Assert.Equal("La fórmula está vacía.", exception.Message);
        }

    }
}
