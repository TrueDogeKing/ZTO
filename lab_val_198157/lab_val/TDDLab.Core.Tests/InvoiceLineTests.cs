using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests
{
    public class InvoiceLineTests
    {
        [Fact]
        public void ValidLine_IsValid()
        {
            var line = TestData.ValidLine();

            Assert.True(line.IsValid);
            Assert.Empty(line.Validate());
        }

        [Fact]
        public void Constructor_SetsProperties_AndInvoiceIsNull()
        {
            var money = new Money(10, "PLN");

            var line = new InvoiceLine("Mleko", money);

            Assert.Equal("Mleko", line.ProductName);
            Assert.Same(money, line.Money);
            Assert.Null(line.Invoice);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingProductName_BreaksOnlyProductNameRule(string productName)
        {
            var line = new InvoiceLine(productName, TestData.ValidMoney());

            var broken = line.Validate().ToList();

            Assert.Single(broken);
            Assert.Contains(InvoiceLine.ValidationRules.ProductName, broken);
        }

        [Fact]
        public void NullMoney_BreaksOnlyMoneyRule()
        {
            var line = new InvoiceLine("Mleko", null);

            var broken = line.Validate().ToList();

            Assert.Single(broken);
            Assert.Contains(InvoiceLine.ValidationRules.Money, broken);
        }

        [Fact]
        public void InvalidMoney_BreaksOnlyMoneyRule()
        {
            var line = new InvoiceLine("Mleko", new Money(10, ""));

            var broken = line.Validate().ToList();

            Assert.Single(broken);
            Assert.Contains(InvoiceLine.ValidationRules.Money, broken);
        }

        [Fact]
        public void ZeroMoney_IsValid()
        {
            var line = new InvoiceLine("Gratis", Money.ZERO);

            Assert.True(line.IsValid);
        }

        [Fact]
        public void ToString_ContainsProductNameAndMoney()
        {
            var line = new InvoiceLine("Mleko", new Money(10, "PLN"));

            Assert.Equal("Mleko for 10PLN", line.ToString());
        }
    }
}
