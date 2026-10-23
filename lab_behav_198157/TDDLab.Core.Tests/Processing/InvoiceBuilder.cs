using System.Collections.Generic;
using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests.Processing
{
    internal static class InvoiceBuilder
    {
        public static Address ValidAddress()
        {
            return new Address("Wybrzeze Wyspianskiego 27", "Wroclaw", "dolnoslaskie", "50-370");
        }

        public static Recipient ValidRecipient()
        {
            return new Recipient("Jan Kowalski", ValidAddress());
        }

        public static Invoice ValidInvoice(params InvoiceLine[] lines)
        {
            return new Invoice("FV/1/2026", ValidRecipient(), ValidAddress(), lines);
        }

        public static Invoice ValidInvoiceWithDiscount(Money discount, params InvoiceLine[] lines)
        {
            return new Invoice("FV/1/2026", ValidRecipient(), ValidAddress(), lines, discount);
        }

        public static Invoice InvalidInvoice()
        {
            // brak numeru faktury -> regula InvoiceNumber niespelniona
            return new Invoice(null, ValidRecipient(), ValidAddress(), new List<InvoiceLine>
            {
                new InvoiceLine("Mleko", new Money(10))
            });
        }
    }
}
