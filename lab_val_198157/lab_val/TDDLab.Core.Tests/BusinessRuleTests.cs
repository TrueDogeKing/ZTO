using BasicUtils;
using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests
{
    public class BusinessRuleTests
    {
        private static BusinessRule<Money> PositiveAmountRule() =>
            new BusinessRule<Money>("PositiveAmount", "Amount should be positive", m => m.Amount > 0);

        // BusinessRule

        [Fact]
        public void Constructor_SetsNameAndDescription()
        {
            var rule = PositiveAmountRule();

            Assert.Equal("PositiveAmount", rule.Name);
            Assert.Equal("Amount should be positive", rule.Description);
        }

        [Fact]
        public void IsSatisfiedBy_UsesPredicate()
        {
            var rule = PositiveAmountRule();

            Assert.True(rule.IsSatisfiedBy(new Money(1)));
            Assert.False(rule.IsSatisfiedBy(new Money(0)));
        }

        [Fact]
        public void RuleName_ComesFromPropertyGetterName()
        {
            // MethodBase.GetCurrentMethod() wywołane w getterze zwraca "get_<NazwaWłaściwości>".
            Assert.Equal("get_Currency", Money.ValidationRules.Currency.Name);
            Assert.Equal("get_Recipient", Invoice.ValidationRules.Recipient.Name);
        }

        [Fact]
        public void RuleDescription_IsHumanReadableMessage()
        {
            Assert.Equal("Currency should be specified", Money.ValidationRules.Currency.Description);
        }

        [Fact]
        public void Equals_SameName_True()
        {
            Assert.Equal(Invoice.ValidationRules.Recipient, Invoice.ValidationRules.Recipient);
            Assert.True(Invoice.ValidationRules.Recipient.Equals(Invoice.ValidationRules.Recipient));
        }

        [Fact]
        public void Equals_DifferentNameSameType_False()
        {
            Assert.False(Invoice.ValidationRules.Recipient.Equals(Invoice.ValidationRules.Lines));
        }

        [Fact]
        public void Equals_RulesWithSameNameButDifferentPredicate_AreEqual()
        {
            var a = new BusinessRule<Money>("X", "a", m => true);
            var b = new BusinessRule<Money>("X", "b", m => false);

            Assert.True(a.Equals(b));
        }

        // Dziwne zachowanie (do omówienia): Equals zwraca true dla null i obiektów innego typu.
        [Fact]
        public void Equals_Null_ReturnsTrue_Quirk()
        {
            Assert.True(Invoice.ValidationRules.Recipient.Equals(null));
        }

        [Fact]
        public void Equals_ObjectOfOtherType_ReturnsTrue_Quirk()
        {
            Assert.True(Invoice.ValidationRules.Recipient.Equals("cokolwiek"));
        }

        [Fact]
        public void Equals_RuleOfOtherDomainType_ReturnsTrue_Quirk()
        {
            // Reguła Invoice "równa" regule Money – przez to Contains może dać fałszywe true.
            Assert.True(Invoice.ValidationRules.Recipient.Equals(Money.ValidationRules.Currency));
        }

        [Fact]
        public void Contains_WithRuleOfOtherDomainType_GivesFalsePositive_Quirk()
        {
            var broken = new Invoice().Validate();

            Assert.True(broken.Contains(Money.ValidationRules.Currency));
        }

        // BusinessRuleSet

        [Fact]
        public void RuleSet_Empty()
        {
            var set = new BusinessRuleSet<Money>();

            Assert.True(set.IsEmpty);
            Assert.Equal(0, set.Count);
            Assert.Empty(set.Messages);
            Assert.Empty(set.BrokenBy(new Money(1)));
        }

        [Fact]
        public void RuleSet_CountAndIsEmpty()
        {
            var set = new BusinessRuleSet<Money>(Money.ValidationRules.Currency, PositiveAmountRule());

            Assert.Equal(2, set.Count);
            Assert.False(set.IsEmpty);
        }

        [Fact]
        public void RuleSet_ConstructedFromList()
        {
            var rules = new List<IBusinessRule<Money>> { Money.ValidationRules.Currency };

            var set = new BusinessRuleSet<Money>(rules);

            Assert.Equal(1, set.Count);
        }

        [Fact]
        public void RuleSet_Messages_ReturnsDescriptionsInOrder()
        {
            var set = new BusinessRuleSet<Money>(Money.ValidationRules.Currency, PositiveAmountRule());

            Assert.Equal(new[] { "Currency should be specified", "Amount should be positive" }, set.Messages);
        }

        [Fact]
        public void RuleSet_Contains_ComparesByName()
        {
            var set = new BusinessRuleSet<Money>(Money.ValidationRules.Currency);

            Assert.True(set.Contains(Money.ValidationRules.Currency));
            Assert.False(set.Contains(PositiveAmountRule()));
        }

        [Fact]
        public void RuleSet_BrokenBy_ReturnsOnlyBrokenRules()
        {
            var positive = PositiveAmountRule();
            var set = new BusinessRuleSet<Money>(Money.ValidationRules.Currency, positive);

            var broken = set.BrokenBy(new Money(0, "PLN")).ToList();

            Assert.Single(broken);
            Assert.Equal("PositiveAmount", broken[0].Name);
        }

        [Fact]
        public void RuleSet_BrokenBy_ReturnsAllBrokenRulesInOrder()
        {
            var set = new BusinessRuleSet<Money>(Money.ValidationRules.Currency, PositiveAmountRule());

            var broken = set.BrokenBy(new Money(0, null)).ToList();

            Assert.Equal(new[] { "get_Currency", "PositiveAmount" }, broken.Select(r => r.Name));
        }

        [Fact]
        public void RuleSet_BrokenBy_ObjectOfWrongType_Throws()
        {
            var set = new BusinessRuleSet<Money>(Money.ValidationRules.Currency);

            Assert.Throws<InvalidCastException>(() => set.BrokenBy(TestData.ValidAddress()));
        }

        // ValidatedDomainObject

        [Fact]
        public void IsValid_IsConsistentWithValidate()
        {
            var valid = TestData.ValidInvoice();
            var invalid = new Invoice();

            Assert.Equal(!valid.Validate().Any(), valid.IsValid);
            Assert.Equal(!invalid.Validate().Any(), invalid.IsValid);
        }
    }
}
