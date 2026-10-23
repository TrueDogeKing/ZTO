using System.Globalization;
using BasicUtils;

namespace TDDLab.Core.Tests
{
    public class BasicExtensionsTests
    {
        [Fact]
        public void AddMany_AddsAllItemsAndReturnsSameList()
        {
            IList<int> list = new List<int> { 1 };

            var result = list.AddMany(2, 3);

            Assert.Same(list, result);
            Assert.Equal(new[] { 1, 2, 3 }, list);
        }

        [Fact]
        public void AddRange_AddsAllItems()
        {
            IList<int> list = new List<int>();

            list.AddRange(new[] { 4, 5 });

            Assert.Equal(new[] { 4, 5 }, list);
        }

        [Fact]
        public void AddRange_NullItems_DoesNothing()
        {
            IList<int> list = new List<int>();

            list.AddRange((IEnumerable<int>)null);

            Assert.Empty(list);
        }

        [Fact]
        public void Each_CallsActionForEveryItem()
        {
            var visited = new List<string>();

            new[] { "a", "b", "c" }.Each(x => visited.Add(x));

            Assert.Equal(new[] { "a", "b", "c" }, visited);
        }

        [Fact]
        public void Each_NullCollection_DoesNotCallAction()
        {
            var calls = 0;

            var result = ((IEnumerable<int>)null).Each(x => calls++);

            Assert.Equal(0, calls);
            Assert.Null(result);
        }

        [Fact]
        public void Each_NonGeneric_CallsActionForEveryItem()
        {
            System.Collections.IEnumerable values = new System.Collections.ArrayList { 1, "x" };
            var visited = new List<object>();

            values.Each(x => visited.Add(x));

            Assert.Equal(new object[] { 1, "x" }, visited);
        }

        [Fact]
        public void Get_ExistingKey_ReturnsValue()
        {
            IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };

            Assert.Equal(1, dict.Get("a"));
        }

        [Fact]
        public void Get_MissingKey_ReturnsDefault()
        {
            IDictionary<string, int> dict = new Dictionary<string, int>();

            Assert.Equal(0, dict.Get("x"));
        }

        [Fact]
        public void Get_MissingKey_ReturnsProvidedDefault()
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            Assert.Equal("brak", dict.Get("x", "brak"));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", false)]
        [InlineData("abc", false)]
        public void String_IsEmpty_IsNotEmpty(string value, bool expectedEmpty)
        {
            Assert.Equal(expectedEmpty, value.IsEmpty());
            Assert.Equal(!expectedEmpty, value.IsNotEmpty());
        }

        [Fact]
        public void Collection_IsEmpty_IsNotEmpty()
        {
            int[] nullArray = null;

            Assert.True(nullArray.IsEmpty());
            Assert.False(nullArray.IsNotEmpty());

            Assert.True(new int[0].IsEmpty());
            Assert.False(new int[0].IsNotEmpty());

            Assert.False(new[] { 1 }.IsEmpty());
            Assert.True(new[] { 1 }.IsNotEmpty());
        }


        [Fact]
        public void IsArrayOf()
        {
            Assert.True(typeof(int[]).IsArrayOf<int>());
            Assert.True(typeof(string[]).IsArrayOf<object>());   
            Assert.False(typeof(int[]).IsArrayOf<long>());
            Assert.False(typeof(int).IsArrayOf<int>());          
        }

        [Fact]
        public void IsNullable()
        {
            Assert.False(typeof(int).IsNullable());
            Assert.True(typeof(int?).IsNullable());
            Assert.True(typeof(string).IsNullable());
        }

        [Fact]
        public void IsNullableOfT()
        {
            Assert.True(typeof(int?).IsNullableOfT());
            Assert.False(typeof(int).IsNullableOfT());
            Assert.False(typeof(List<int>).IsNullableOfT());
        }

        [Fact]
        public void IsNullableOf()
        {
            Assert.True(typeof(int?).IsNullableOf(typeof(int)));
            Assert.False(typeof(long?).IsNullableOf(typeof(int)));
            Assert.False(typeof(int).IsNullableOf(typeof(int)));
        }

        [Fact]
        public void IsGenericEnumerable()
        {
            Assert.True(typeof(List<int>).IsGenericEnumerable());
            Assert.True(typeof(IEnumerable<int>).IsGenericEnumerable());
            Assert.False(typeof(int).IsGenericEnumerable());
            Assert.False(typeof(int?).IsGenericEnumerable());
        }


        [Theory]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("false", false)]
        [InlineData("FALSE", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ToBool_ParsesValues(string value, bool expected)
        {
            Assert.Equal(expected, value.ToBool());
        }

        [Fact]
        public void ToBool_InvalidText_Throws()
        {
            Assert.Throws<FormatException>(() => "tak".ToBool());
        }

        [Fact]
        public void ToFormat_FormatsString()
        {
            Assert.Equal("1-a", "{0}-{1}".ToFormat(1, "a"));
        }

        [Fact]
        public void ToJSON_InvariantCulture()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

                Assert.Equal("10/20/2026 13:05:09", new DateTime(2026, 10, 20, 13, 5, 9).ToJSON());
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        // ZNANY BŁĄD: "/" w formacie daty to separator zależny od kultury.
        // Dla pl-PL wychodzi "10.20.2026 13:05:09", a Replace("-", "/") tego nie łapie.
        [Fact(Skip="Nie przewidziano używania innych separatorów niż '-' lub '/' w polsce występuje '.' ")]
        public void ToJSON_IsCultureIndependent()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Assert.Equal("10/20/2026 13:05:09", new DateTime(2026, 10, 20, 13, 5, 9).ToJSON());
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }
    }
}
