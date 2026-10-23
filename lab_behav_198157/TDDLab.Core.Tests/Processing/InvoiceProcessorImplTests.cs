using System;
using TDDLab.Core.InvoiceMgmt;
using static TDDLab.Core.Tests.Processing.InvoiceBuilder;

namespace TDDLab.Core.Tests.Processing
{
    public class InvoiceProcessorImplTests
    {
        private readonly InvoiceProcessorImpl processor = new InvoiceProcessorImpl();

        [Fact]
        public void Process_InvalidInvoice_ReturnsFailed()
        {
            var result = processor.Process(InvalidInvoice());

            Assert.Equal(ProcessingResult.Failed(), result);
        }

        [Fact]
        public void Process_InvalidInvoice_DoesNotRegisterProducts()
        {
            processor.Process(InvalidInvoice());

            Assert.Empty(processor.Products);
        }

        [Fact]
        public void Process_InvoiceWithoutLines_ReturnsFailed()
        {
            var result = processor.Process(ValidInvoice());

            Assert.Equal(ProcessingResult.Failed(), result);
        }

        [Fact]
        public void Process_InvoiceWithInvalidLine_ReturnsFailed()
        {
            var invoice = ValidInvoice(new InvoiceLine("", new Money(10)));

            var result = processor.Process(invoice);

            Assert.Equal(ProcessingResult.Failed(), result);
            Assert.Empty(processor.Products);
        }

        [Fact]
        public void Process_ValidInvoice_ReturnsSucceeded()
        {
            var result = processor.Process(ValidInvoice(new InvoiceLine("Mleko", new Money(10))));

            Assert.Equal(ProcessingResult.Succeeded(), result);
        }

        [Fact]
        public void Process_SingleLine_RegistersProductWithLineMoney()
        {
            processor.Process(ValidInvoice(new InvoiceLine("Mleko", new Money(10))));

            Assert.Single(processor.Products);
            Assert.Equal(new Money(10), processor.Products["Mleko"]);
        }

        [Fact]
        public void Process_DistinctProducts_RegistersEachSeparately()
        {
            processor.Process(ValidInvoice(
                new InvoiceLine("Mleko", new Money(10)),
                new InvoiceLine("Chleb", new Money(5))));

            Assert.Equal(2, processor.Products.Count);
            Assert.Equal(new Money(10), processor.Products["Mleko"]);
            Assert.Equal(new Money(5), processor.Products["Chleb"]);
        }

        [Fact]
        public void Process_FirstOccurrenceOfProduct_IgnoresDiscount()
        {
            processor.Process(ValidInvoiceWithDiscount(new Money(3),
                new InvoiceLine("Mleko", new Money(10))));

            Assert.Equal(new Money(10), processor.Products["Mleko"]);
        }

        [Fact]
        public void Process_RepeatedProduct_AddsLineMoneyMinusDiscount()
        {
            processor.Process(ValidInvoiceWithDiscount(new Money(3),
                new InvoiceLine("Mleko", new Money(10)),
                new InvoiceLine("Mleko", new Money(10))));

            Assert.Equal(new Money(10 + (10 - 3)), processor.Products["Mleko"]);
        }

        [Fact]
        public void Process_DiscountGreaterThanLineMoney_AddsNothingForRepeatedProduct()
        {
            processor.Process(ValidInvoiceWithDiscount(new Money(50),
                new InvoiceLine("Mleko", new Money(10)),
                new InvoiceLine("Mleko", new Money(10))));

            Assert.Equal(new Money(10), processor.Products["Mleko"]);
        }

        [Fact]
        public void Process_ManyInvoices_AccumulatesProductsBetweenCalls()
        {
            processor.Process(ValidInvoiceWithDiscount(Money.ZERO, new InvoiceLine("Mleko", new Money(10))));
            processor.Process(ValidInvoiceWithDiscount(new Money(2), new InvoiceLine("Mleko", new Money(10))));

            Assert.Equal(new Money(18), processor.Products["Mleko"]);
        }

        [Fact]
        public void Process_RepeatedProductWithoutDiscount_ThrowsNullReference()
        {
            // Test charakteryzujacy: przy braku rabatu (Discount == null) powtorzony produkt
            // powoduje NullReferenceException w Money.operator- (prawdopodobnie blad w implementacji).
            var invoice = ValidInvoice(
                new InvoiceLine("Mleko", new Money(10)),
                new InvoiceLine("Mleko", new Money(10)));

            Assert.Throws<NullReferenceException>(() => processor.Process(invoice));
        }
    }
}
