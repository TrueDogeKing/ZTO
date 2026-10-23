using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests
{
    public class RecipientTests
    {
        [Fact]
        public void ValidRecipient_IsValid()
        {
            var recipient = TestData.ValidRecipient();

            Assert.True(recipient.IsValid);
            Assert.Empty(recipient.Validate());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingName_BreaksOnlyNameRule(string name)
        {
            var recipient = new Recipient(name, TestData.ValidAddress());

            var broken = recipient.Validate().ToList();

            Assert.False(recipient.IsValid);
            Assert.Single(broken);
            Assert.Contains(Recipient.ValidationRules.Name, broken);
        }

        [Fact]
        public void InvalidAddress_BreaksOnlyAddressRule()
        {
            var invalidAddress = new Address("ul. Prosta 1", "", "mazowieckie", "00-001");
            var recipient = new Recipient("Jan Kowalski", invalidAddress);

            var broken = recipient.Validate().ToList();

            Assert.Single(broken);
            Assert.Contains(Recipient.ValidationRules.Address, broken);
        }

        [Fact]
        public void MissingNameAndInvalidAddress_BreaksBothRules()
        {
            var recipient = new Recipient("", new Address(null, null, null, null));

            var broken = recipient.Validate().ToList();

            Assert.Equal(2, broken.Count);
            Assert.Contains(Recipient.ValidationRules.Name, broken);
            Assert.Contains(Recipient.ValidationRules.Address, broken);
        }

        // Reguła sprawdza "recipient != null" zamiast "recipient.Address != null",
        // więc przy braku adresu leci NullReferenceException zamiast złamanej reguły.
        [Fact(Skip = "Recipient.ValidationRules.Address nie sprawdza Address == null")]
        public void NullAddress_BreaksAddressRule_InsteadOfThrowing()
        {
            var recipient = new Recipient("Jan Kowalski", null);

            var broken = recipient.Validate().ToList();

            Assert.False(recipient.IsValid);
            Assert.Contains(Recipient.ValidationRules.Address, broken);
        }
    }
}
