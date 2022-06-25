using HintKeep.Storage;
using Xunit;

namespace HintKeep.Tests.Unit.Storage
{
    public static class ExtensionsTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("%", "%25")]
        [InlineData("/", "%2F")]
        [InlineData("\\", "%5C")]
        [InlineData("#", "%23")]
        [InlineData("?", "%3F")]
        [InlineData("\x00", "%00")]
        [InlineData("\x01", "%01")]
        [InlineData("\x02", "%02")]
        [InlineData("\x03", "%03")]
        [InlineData("\x04", "%04")]
        [InlineData("\x05", "%05")]
        [InlineData("\x06", "%06")]
        [InlineData("\x07", "%07")]
        [InlineData("\x08", "%08")]
        [InlineData("\x09", "%09")]
        [InlineData("\x0A", "%0A")]
        [InlineData("\x0B", "%0B")]
        [InlineData("\x0C", "%0C")]
        [InlineData("\x0D", "%0D")]
        [InlineData("\x0E", "%0E")]
        [InlineData("\x0F", "%0F")]
        [InlineData("\x10", "%10")]
        [InlineData("\x11", "%11")]
        [InlineData("\x12", "%12")]
        [InlineData("\x13", "%13")]
        [InlineData("\x14", "%14")]
        [InlineData("\x15", "%15")]
        [InlineData("\x16", "%16")]
        [InlineData("\x17", "%17")]
        [InlineData("\x18", "%18")]
        [InlineData("\x19", "%19")]
        [InlineData("\x1A", "%1A")]
        [InlineData("\x1B", "%1B")]
        [InlineData("\x1C", "%1C")]
        [InlineData("\x1D", "%1D")]
        [InlineData("\x1E", "%1E")]
        [InlineData("\x1F", "%1F")]
        [InlineData("\x7F", "%7F")]
        [InlineData("\x80", "%80")]
        [InlineData("\x81", "%81")]
        [InlineData("\x82", "%82")]
        [InlineData("\x83", "%83")]
        [InlineData("\x84", "%84")]
        [InlineData("\x85", "%85")]
        [InlineData("\x86", "%86")]
        [InlineData("\x87", "%87")]
        [InlineData("\x88", "%88")]
        [InlineData("\x89", "%89")]
        [InlineData("\x8A", "%8A")]
        [InlineData("\x8B", "%8B")]
        [InlineData("\x8C", "%8C")]
        [InlineData("\x8D", "%8D")]
        [InlineData("\x8E", "%8E")]
        [InlineData("\x8F", "%8F")]
        [InlineData("\x90", "%90")]
        [InlineData("\x91", "%91")]
        [InlineData("\x92", "%92")]
        [InlineData("\x93", "%93")]
        [InlineData("\x94", "%94")]
        [InlineData("\x95", "%95")]
        [InlineData("\x96", "%96")]
        [InlineData("\x97", "%97")]
        [InlineData("\x98", "%98")]
        [InlineData("\x99", "%99")]
        [InlineData("\x9A", "%9A")]
        [InlineData("\x9B", "%9B")]
        [InlineData("\x9C", "%9C")]
        [InlineData("\x9D", "%9D")]
        [InlineData("\x9E", "%9E")]
        [InlineData("\x9F", "%9F")]
        public static void ToEncodedKeyProperty_ConvertsValue_ReturnsDecodedValue(string value, string expectedValue)
            => Assert.Equal(expectedValue, value.ToEncodedKeyProperty());
    }
}