using BasicUtils;
using TDDLab.Core.InvoiceMgmt;

namespace TDDLab.Core.Tests
{
    public class AddressTests
    {
        [Fact]
        public void ValidAddress_IsValid()
        {
            var address = TestData.ValidAddress();

            Assert.True(address.IsValid);
            Assert.Empty(address.Validate());
        }

        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var address = new Address("ul. Prosta 1", "Warszawa", "mazowieckie", "00-001");

            Assert.Equal("ul. Prosta 1", address.AddressLine1);
            Assert.Equal("Warszawa", address.City);
            Assert.Equal("mazowieckie", address.State);
            Assert.Equal("00-001", address.Zip);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingAddressLine1_BreaksOnlyAddressLine1Rule(string value)
        {
            var address = new Address(value, "Warszawa", "mazowieckie", "00-001");

            AssertOnlyBroken(address, Address.ValidationRules.AddressLine1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingCity_BreaksOnlyCityRule(string value)
        {
            var address = new Address("ul. Prosta 1", value, "mazowieckie", "00-001");

            AssertOnlyBroken(address, Address.ValidationRules.City);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingState_BreaksOnlyStateRule(string value)
        {
            var address = new Address("ul. Prosta 1", "Warszawa", value, "00-001");

            AssertOnlyBroken(address, Address.ValidationRules.State);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingZip_BreaksOnlyZipRule(string value)
        {
            var address = new Address("ul. Prosta 1", "Warszawa", "mazowieckie", value);

            AssertOnlyBroken(address, Address.ValidationRules.Zip);
        }

        [Fact]
        public void AllFieldsEmpty_BreaksAllFourRules()
        {
            var address = new Address(null, null, null, null);

            var broken = address.Validate().ToList();

            Assert.Equal(4, broken.Count);
            Assert.Contains(Address.ValidationRules.AddressLine1, broken);
            Assert.Contains(Address.ValidationRules.City, broken);
            Assert.Contains(Address.ValidationRules.State, broken);
            Assert.Contains(Address.ValidationRules.Zip, broken);
        }

        [Fact]
        public void WhitespaceCity_IsTreatedAsSpecified()
        {
            // IsNotEmpty używa string.IsNullOrEmpty, więc same spacje przechodzą walidację.
            var address = new Address("ul. Prosta 1", "   ", "mazowieckie", "00-001");

            Assert.True(address.IsValid);
        }

        private static void AssertOnlyBroken(Address address, IRule expectedRule)
        {
            var broken = address.Validate().ToList();

            Assert.False(address.IsValid);
            Assert.Single(broken);
            Assert.Contains(expectedRule, broken);
        }
    }
}
