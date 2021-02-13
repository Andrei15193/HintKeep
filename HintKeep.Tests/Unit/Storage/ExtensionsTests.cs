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

        [Theory]
        [InlineData(null, null)]
        [InlineData("%25", "%")]
        [InlineData("%2F", "/")]
        [InlineData("%5C", "\\")]
        [InlineData("%23", "#")]
        [InlineData("%3F", "?")]
        [InlineData("%00", "\x00")]
        [InlineData("%01", "\x01")]
        [InlineData("%02", "\x02")]
        [InlineData("%03", "\x03")]
        [InlineData("%04", "\x04")]
        [InlineData("%05", "\x05")]
        [InlineData("%06", "\x06")]
        [InlineData("%07", "\x07")]
        [InlineData("%08", "\x08")]
        [InlineData("%09", "\x09")]
        [InlineData("%0A", "\x0A")]
        [InlineData("%0B", "\x0B")]
        [InlineData("%0C", "\x0C")]
        [InlineData("%0D", "\x0D")]
        [InlineData("%0E", "\x0E")]
        [InlineData("%0F", "\x0F")]
        [InlineData("%10", "\x10")]
        [InlineData("%11", "\x11")]
        [InlineData("%12", "\x12")]
        [InlineData("%13", "\x13")]
        [InlineData("%14", "\x14")]
        [InlineData("%15", "\x15")]
        [InlineData("%16", "\x16")]
        [InlineData("%17", "\x17")]
        [InlineData("%18", "\x18")]
        [InlineData("%19", "\x19")]
        [InlineData("%1A", "\x1A")]
        [InlineData("%1B", "\x1B")]
        [InlineData("%1C", "\x1C")]
        [InlineData("%1D", "\x1D")]
        [InlineData("%1E", "\x1E")]
        [InlineData("%1F", "\x1F")]
        [InlineData("%7F", "\x7F")]
        [InlineData("%80", "\x80")]
        [InlineData("%81", "\x81")]
        [InlineData("%82", "\x82")]
        [InlineData("%83", "\x83")]
        [InlineData("%84", "\x84")]
        [InlineData("%85", "\x85")]
        [InlineData("%86", "\x86")]
        [InlineData("%87", "\x87")]
        [InlineData("%88", "\x88")]
        [InlineData("%89", "\x89")]
        [InlineData("%8A", "\x8A")]
        [InlineData("%8B", "\x8B")]
        [InlineData("%8C", "\x8C")]
        [InlineData("%8D", "\x8D")]
        [InlineData("%8E", "\x8E")]
        [InlineData("%8F", "\x8F")]
        [InlineData("%90", "\x90")]
        [InlineData("%91", "\x91")]
        [InlineData("%92", "\x92")]
        [InlineData("%93", "\x93")]
        [InlineData("%94", "\x94")]
        [InlineData("%95", "\x95")]
        [InlineData("%96", "\x96")]
        [InlineData("%97", "\x97")]
        [InlineData("%98", "\x98")]
        [InlineData("%99", "\x99")]
        [InlineData("%9A", "\x9A")]
        [InlineData("%9B", "\x9B")]
        [InlineData("%9C", "\x9C")]
        [InlineData("%9D", "\x9D")]
        [InlineData("%9E", "\x9E")]
        [InlineData("%9F", "\x9F")]
        [InlineData("%2", "%2")]
        [InlineData("%", "%")]
        public static void FromEncodedKeyProperty_ConvertsValue_ReturnsDecodedValue(string value, string expectedValue)
            => Assert.Equal(expectedValue, value.FromEncodedKeyProperty());
    }
}