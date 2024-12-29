using Xunit;
using FluentAssertions;
using JsonSerializer = LocalAI.NET.Utils.JsonSerializer;

namespace LocalAI.NET.Tests.Utils
{
    public class JsonSerializerTests
    {
        private class TestObject
        {
            public string StringProp { get; set; } = "";
            public int IntProp { get; set; }
            public DateTime? DateProp { get; set; }
        }

        [Fact]
        public void Serialize_WithDefaultSettings_ShouldUseCamelCase()
        {
            // Arrange
            var obj = new TestObject 
            { 
                StringProp = "test",
                IntProp = 42
            };

            // Act
            var json = JsonSerializer.Serialize(obj);

            // Assert
            json.Should().Contain("stringProp");
            json.Should().NotContain("StringProp");
        }

        [Fact]
        public void Deserialize_WithNullValues_ShouldDeserializeCorrectly()
        {
            // Arrange
            var json = "{\"stringProp\":null,\"intProp\":42,\"dateProp\":null}";

            // Act
            var obj = JsonSerializer.Deserialize<TestObject>(json);

            // Assert
            obj.Should().NotBeNull();
            obj!.StringProp.Should().BeEmpty();
            obj.IntProp.Should().Be(42);
            obj.DateProp.Should().BeNull();
        }
    }
}