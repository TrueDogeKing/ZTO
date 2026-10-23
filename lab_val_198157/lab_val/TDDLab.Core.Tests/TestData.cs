using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests
{
    /// <summary>
    /// Fabryka poprawnych obiektów domenowych.
    /// Każdy test zaczyna od poprawnego obiektu i psuje w nim dokładnie jedną rzecz.
    /// </summary>
    internal static class TestData
    {
        public static Address ValidAddress() =>
            new Address("ul. Prosta 1", "Warszawa", "mazowieckie", "00-001");

        public static Recipient ValidRecipient() =>
            new Recipient("Jan Kowalski", ValidAddress());

        public static Money ValidMoney(ulong amount = 100) =>
            new Money(amount, "PLN");

        public static InvoiceLine ValidLine() =>
            new InvoiceLine("Mleko", ValidMoney());

        public static Invoice ValidInvoice() =>
            new Invoice("FV/1/2026", ValidRecipient(), ValidAddress(), new[] { ValidLine() });
    }
}
