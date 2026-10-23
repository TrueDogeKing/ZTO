using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests
{
    public class InvoiceTests
    {

        [Fact]
        public void ValidInvoice_IsValid()
        {
            var invoice = TestData.ValidInvoice();

            Assert.True(invoice.IsValid);
            Assert.Empty(invoice.Validate());
        }

        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var recipient = TestData.ValidRecipient();
            var address = TestData.ValidAddress();
            var line = TestData.ValidLine();
            var discount = new Money(5, "PLN");

            var invoice = new Invoice("FV/1/2026", recipient, address, new[] { line }, discount);

            Assert.Equal("FV/1/2026", invoice.InvoiceNumber);
            Assert.Same(recipient, invoice.Recipient);
            Assert.Same(address, invoice.BillToAddress);
            Assert.Same(discount, invoice.Discount);
            Assert.Single(invoice.Lines);
            Assert.Same(line, invoice.Lines[0]);
        }

        [Fact]
        public void ConstructorWithoutDiscount_SetsDiscountToNull()
        {
            Assert.Null(TestData.ValidInvoice().Discount);
        }

        [Fact]
        public void Constructor_CopiesLinesToNewList()
        {
            var source = new List<InvoiceLine> { TestData.ValidLine() };

            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), source);
            source.Add(TestData.ValidLine());

            Assert.Single(invoice.Lines);
        }

        [Fact]
        public void Constructor_NullLines_DoesNotThrow_AndLinesAreEmpty()
        {
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), null);

            Assert.NotNull(invoice.Lines);
            Assert.Empty(invoice.Lines);
        }

        // --- pusta faktura ---

        [Fact]
        public void DefaultConstructor_LinesAreEmptyNotNull()
        {
            var invoice = new Invoice();

            Assert.NotNull(invoice.Lines);
            Assert.Empty(invoice.Lines);
        }

        [Fact]
        public void DefaultConstructor_BreaksAllRulesExceptDiscount()
        {
            var invoice = new Invoice();

            var broken = invoice.Validate().ToList();

            Assert.False(invoice.IsValid);
            Assert.Equal(4, broken.Count);
            Assert.Contains(Invoice.ValidationRules.InvoiceNumber, broken);
            Assert.Contains(Invoice.ValidationRules.BillingAddress, broken);
            Assert.Contains(Invoice.ValidationRules.Recipient, broken);
            Assert.Contains(Invoice.ValidationRules.Lines, broken);
        }

        [Fact]
        public void SampleFromValidationSample_EmptyInvoiceHasBrokenRecipientRule()
        {
            // Przykład z pliku Validation_sample.cs
            var recipientRuleBroken = new Invoice().Validate().Contains(Invoice.ValidationRules.Recipient);

            Assert.True(recipientRuleBroken);
        }

        // --- numer faktury ---

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingInvoiceNumber_BreaksOnlyInvoiceNumberRule(string number)
        {
            var invoice = new Invoice(number, TestData.ValidRecipient(), TestData.ValidAddress(), new[] { TestData.ValidLine() });

            AssertOnlyBroken(invoice, Invoice.ValidationRules.InvoiceNumber);
        }

        // --- adres rozliczeniowy ---

        [Fact]
        public void NullBillingAddress_BreaksOnlyBillingAddressRule()
        {
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), null, new[] { TestData.ValidLine() });

            AssertOnlyBroken(invoice, Invoice.ValidationRules.BillingAddress);
        }

        [Fact]
        public void InvalidBillingAddress_BreaksOnlyBillingAddressRule()
        {
            var badAddress = new Address("ul. Prosta 1", "Warszawa", "mazowieckie", "");
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), badAddress, new[] { TestData.ValidLine() });

            AssertOnlyBroken(invoice, Invoice.ValidationRules.BillingAddress);
        }

        // --- odbiorca ---

        [Fact]
        public void NullRecipient_BreaksOnlyRecipientRule()
        {
            var invoice = new Invoice("FV/1/2026", null, TestData.ValidAddress(), new[] { TestData.ValidLine() });

            AssertOnlyBroken(invoice, Invoice.ValidationRules.Recipient);
        }

        [Fact]
        public void RecipientWithoutName_BreaksOnlyRecipientRule()
        {
            var recipient = new Recipient("", TestData.ValidAddress());
            var invoice = new Invoice("FV/1/2026", recipient, TestData.ValidAddress(), new[] { TestData.ValidLine() });

            AssertOnlyBroken(invoice, Invoice.ValidationRules.Recipient);
        }

        [Fact]
        public void RecipientWithInvalidAddress_BreaksRecipientRule_Cascading()
        {
            // Zły adres odbiorcy -> nieprawidłowy Recipient -> nieprawidłowa faktura.
            var recipient = new Recipient("Jan Kowalski", new Address("", "Warszawa", "mazowieckie", "00-001"));
            var invoice = new Invoice("FV/1/2026", recipient, TestData.ValidAddress(), new[] { TestData.ValidLine() });

            AssertOnlyBroken(invoice, Invoice.ValidationRules.Recipient);
        }

        // --- rabat ---

        [Fact]
        public void NullDiscount_IsValid()
        {
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), new[] { TestData.ValidLine() }, null);

            Assert.True(invoice.IsValid);
        }

        [Fact]
        public void ValidDiscount_IsValid()
        {
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), new[] { TestData.ValidLine() }, new Money(5, "PLN"));

            Assert.True(invoice.IsValid);
        }

        [Fact]
        public void DiscountWithoutCurrency_BreaksOnlyDiscountRule()
        {
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), new[] { TestData.ValidLine() }, new Money(5, ""));

            AssertOnlyBroken(invoice, Invoice.ValidationRules.Discount);
        }

        [Fact]
        public void DiscountBiggerThanLinesTotal_IsStillValid()
        {
            // Brak reguły porównującej rabat z sumą pozycji – to zachowanie obecnego kodu.
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(),
                new[] { new InvoiceLine("Mleko", new Money(10, "PLN")) }, new Money(1000, "PLN"));

            Assert.True(invoice.IsValid);
        }

        // --- pozycje ---

        [Fact]
        public void EmptyLines_BreaksOnlyLinesRule()
        {
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), new InvoiceLine[0]);

            AssertOnlyBroken(invoice, Invoice.ValidationRules.Lines);
        }

        [Fact]
        public void OneInvalidLineAmongValid_BreaksOnlyLinesRule()
        {
            var lines = new[]
            {
                TestData.ValidLine(),
                new InvoiceLine("", TestData.ValidMoney()),
                TestData.ValidLine()
            };
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), lines);

            AssertOnlyBroken(invoice, Invoice.ValidationRules.Lines);
        }

        [Fact]
        public void LineWithInvalidMoney_BreaksLinesRule_Cascading()
        {
            var lines = new[] { new InvoiceLine("Mleko", new Money(10, null)) };
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), lines);

            AssertOnlyBroken(invoice, Invoice.ValidationRules.Lines);
        }

        [Fact]
        public void ManyValidLines_IsValid()
        {
            var lines = new[] { TestData.ValidLine(), TestData.ValidLine(), TestData.ValidLine() };
            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), lines);

            Assert.True(invoice.IsValid);
            Assert.Equal(3, invoice.Lines.Count);
        }

        // --- AttachInvoiceLine ---

        [Fact]
        public void AttachInvoiceLine_AddsLine()
        {
            var invoice = new Invoice();
            var line = TestData.ValidLine();

            invoice.AttachInvoiceLine(line);

            Assert.Single(invoice.Lines);
            Assert.Same(line, invoice.Lines[0]);
        }

        [Fact]
        public void AttachInvoiceLine_SetsBackReferenceToInvoice()
        {
            var invoice = new Invoice();
            var line = TestData.ValidLine();

            invoice.AttachInvoiceLine(line);

            Assert.Same(invoice, line.Invoice);
        }

        [Fact]
        public void AttachInvoiceLine_MakesEmptyLinesRuleSatisfied()
        {
            var invoice = new Invoice();

            invoice.AttachInvoiceLine(TestData.ValidLine());

            Assert.DoesNotContain(invoice.Validate(), r => r.Name == Invoice.ValidationRules.Lines.Name);
        }

        [Fact]
        public void AttachInvoiceLine_ToExistingInvoice_AppendsLine()
        {
            var invoice = TestData.ValidInvoice();

            invoice.AttachInvoiceLine(TestData.ValidLine());

            Assert.Equal(2, invoice.Lines.Count);
            Assert.True(invoice.IsValid);
        }

        // NIESPÓJNOŚĆ: AttachInvoiceLine ustawia line.Invoice, ale konstruktor już nie.
        // Usuń Skip, jeśli zdecydujecie, że konstruktor też ma ustawiać powiązanie.
        [Fact(Skip = "Niespójność: konstruktor Invoice nie ustawia line.Invoice")]
        public void Constructor_SetsBackReferenceOnLines()
        {
            var line = TestData.ValidLine();

            var invoice = new Invoice("FV/1/2026", TestData.ValidRecipient(), TestData.ValidAddress(), new[] { line });

            Assert.Same(invoice, line.Invoice);
        }

        private static void AssertOnlyBroken(Invoice invoice, BasicUtils.IRule expectedRule)
        {
            var broken = invoice.Validate().ToList();

            Assert.False(invoice.IsValid);
            Assert.Single(broken);
            Assert.Contains(expectedRule, broken);
        }
    }
}
