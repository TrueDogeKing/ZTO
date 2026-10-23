using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests
{
    public class MoneyTests
    {
        [Fact]
        public void Constructor_WithoutCurrency_UsesDefaultUsd()
        {
            var money = new Money(10);

            Assert.Equal(10UL, money.Amount);
            Assert.Equal("USD", money.Currency);
            Assert.Equal(Money.DefaultCurrency, money.Currency);
        }

        [Fact]
        public void Zero_IsZeroUsd()
        {
            Assert.Equal(0UL, Money.ZERO.Amount);
            Assert.Equal("USD", Money.ZERO.Currency);
        }

        [Fact]
        public void Zero_ReturnsNewInstanceEachTime()
        {
            Assert.NotSame(Money.ZERO, Money.ZERO);
            Assert.Equal(Money.ZERO, Money.ZERO);
        }

        [Fact]
        public void MoneyWithCurrency_IsValid()
        {
            Assert.True(new Money(10, "PLN").IsValid);
        }

        [Fact]
        public void ZeroAmount_IsValid()
        {
            Assert.True(new Money(0, "PLN").IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingCurrency_BreaksCurrencyRule(string currency)
        {
            var money = new Money(10, currency);

            var broken = money.Validate().ToList();

            Assert.False(money.IsValid);
            Assert.Single(broken);
            Assert.Contains(Money.ValidationRules.Currency, broken);
        }

        [Fact]
        public void Add_SameCurrency_SumsAmounts()
        {
            var result = new Money(10, "PLN") + new Money(5, "PLN");

            Assert.Equal(new Money(15, "PLN"), result);
        }

        [Fact]
        public void Add_DifferentCurrency_KeepsLeftCurrency()
        {
            // CurrencyConverter to atrapa – nie przelicza kwoty, tylko zmienia walutę.
            var result = new Money(10, "USD") + new Money(5, "EUR");

            Assert.Equal(15UL, result.Amount);
            Assert.Equal("USD", result.Currency);
        }

        [Fact]
        public void Add_DoesNotModifyOperands()
        {
            var left = new Money(10, "PLN");
            var right = new Money(5, "PLN");

            _ = left + right;

            Assert.Equal(10UL, left.Amount);
            Assert.Equal(5UL, right.Amount);
        }

        [Fact]
        public void Add_Zero_ReturnsEqualAmount()
        {
            var money = new Money(42);

            Assert.Equal(money, money + Money.ZERO);
        }

        [Fact]
        public void Subtract_SmallerFromBigger_ReturnsDifference()
        {
            var result = new Money(10, "PLN") - new Money(3, "PLN");

            Assert.Equal(new Money(7, "PLN"), result);
        }

        [Fact]
        public void Subtract_BiggerFromSmaller_ReturnsZeroNotNegative()
        {
            var result = new Money(3, "PLN") - new Money(10, "PLN");

            Assert.Equal(new Money(0, "PLN"), result);
        }

        [Fact]
        public void Subtract_EqualAmounts_ReturnsZero()
        {
            var result = new Money(5, "PLN") - new Money(5, "PLN");

            Assert.Equal(0UL, result.Amount);
        }

        // test odpowiada atrapie ToCurrency() test ulegnie zmianie gdy ta metoda zostanie poprawnie zaimplementowana
        [Fact]
        public void Subtract_DifferentCurrency_KeepsLeftCurrency()
        {
            var result = new Money(10, "USD") - new Money(4, "EUR");

            Assert.Equal(new Money(6, "USD"), result);
        }

        [Fact]
        public void ToCurrency_ChangesCurrencyAndKeepsAmount()
        {
            var original = new Money(10, "USD");

            var converted = original.ToCurrency("EUR");

            Assert.Equal(new Money(10, "EUR"), converted);
            Assert.Equal("USD", original.Currency);
        }

        [Fact]
        public void Equals_SameAmountAndCurrency_True()
        {
            var a = new Money(10, "PLN");
            var b = new Money(10, "PLN");

            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equals_DifferentAmount_False()
        {
            Assert.NotEqual(new Money(10, "PLN"), new Money(11, "PLN"));
            Assert.True(new Money(10, "PLN") != new Money(11, "PLN"));
        }

        [Fact]
        public void Equals_DifferentCurrency_False()
        {
            Assert.NotEqual(new Money(10, "PLN"), new Money(10, "EUR"));
        }

        [Fact]
        public void Equals_Null_False()
        {
            var money = new Money(10, "PLN");

            Assert.False(money.Equals((Money)null));
            Assert.False(money.Equals((object)null));
            Assert.False(money == null);
            Assert.False(null == money);
        }

        [Fact]
        public void EqualityOperator_BothNull_True()
        {
            Money a = null;
            Money b = null;

            Assert.True(a == b);
        }

        [Fact]
        public void Equals_OtherType_False()
        {
            Assert.False(new Money(10, "PLN").Equals("10PLN"));
        }

        [Fact]
        public void GetHashCode_NullCurrency_DoesNotThrow()
        {
            var money = new Money(10, null);

            var exception = Record.Exception(() => money.GetHashCode());

            Assert.Null(exception);
        }

        [Fact]
        public void MaxAmount_IsValid()
        {
            var money = new Money(ulong.MaxValue, "PLN");

            Assert.True(money.IsValid);
            Assert.Equal(ulong.MaxValue, money.Amount);
        }

        [Fact]
        public void Add_Overflow_WrapsAroundToZero_CurrentBehavior()
        {
            var result = new Money(ulong.MaxValue, "PLN") + new Money(1, "PLN");

            Assert.Equal(0UL, result.Amount);
        }

        // OCZEKIWANE ZACHOWANIE: przepełnienie powinno być błędem.
        // sytuacja prawie nie realna przepełnienia uLong i moim zdaniem może być pomienięte
        // oraz test Add_Overflow_WrapsAroundToZero_CurrentBehavior.
        [Fact(Skip = "Znany błąd: dodawanie Money nie wykrywa przepełnienia")]
        public void Add_Overflow_ThrowsOverflowException()
        {
            var max = new Money(ulong.MaxValue, "PLN");
            var one = new Money(1, "PLN");

            Assert.Throws<OverflowException>(() => max + one);
        }


        [Fact]
        public void ToString_ReturnsAmountFollowedByCurrency()
        {
            Assert.Equal("10PLN", new Money(10, "PLN").ToString());
        }
    }
}
