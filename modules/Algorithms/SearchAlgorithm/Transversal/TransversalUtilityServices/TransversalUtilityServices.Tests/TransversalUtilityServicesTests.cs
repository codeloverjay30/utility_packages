using FluentAssertions;

namespace TransversalUtilityServices.Tests
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Person BestFriend { get; set; }
        public List<Person> Children { get; set; } = new List<Person>();
    }
    public class DFSTransversalServiceTests
    {
        private readonly DFSTransversalService _service;

        public DFSTransversalServiceTests()
        {
            _service = new DFSTransversalService();
        }

        [Fact]
        public void Transverse_ShouldVisitAllNodes_InSimpleHierarchy()
        {
            // Arrange
            var root = new Person 
            { 
                Name = "Grandpa", 
                BestFriend = new Person { Name = "OldPal" } 
            };
            var visitedNames = new List<string>();

            // Act
            _service.Transverse(root, obj => 
            {
                if (obj is Person p) visitedNames.Add(p.Name);
            });

            // Assert
            visitedNames.Should().HaveCount(2);
            visitedNames.Should().Contain(new[] { "Grandpa", "OldPal" });
        }

        [Fact]
        public void Transverse_ShouldHandleCircularReference_WithoutStackOverflow()
        {
            // Arrange
            var personA = new Person { Name = "Alice" };
            var personB = new Person { Name = "Bob" };
            personA.BestFriend = personB;
            personB.BestFriend = personA; // 循環引用

            var visitedObjects = new List<object>();

            // Act
            Action act = () => _service.Transverse(personA, obj => visitedObjects.Add(obj));

            // Assert
            act.Should().NotThrow<StackOverflowException>();
            visitedObjects.Should().HaveCount(2);
            visitedObjects.Should().Contain(new[] { personA, personB });
        }

        [Fact]
        public void Transverse_ShouldVisitCollectionItems()
        {
            // Arrange
            var root = new Person { Name = "Parent" };
            root.Children.Add(new Person { Name = "Child 1" });
            root.Children.Add(new Person { Name = "Child 2" });

            var visitedNames = new List<string>();

            // Act
            _service.Transverse(root, obj => 
            {
                if (obj is Person p) visitedNames.Add(p.Name);
            });

            // Assert
            visitedNames.Should().Contain(new[] { "Parent", "Child 1", "Child 2" });
        }

        [Fact]
        public void Transverse_ShouldSkipStringsAndPrimitives()
        {
            // Arrange
            var root = new Person { Name = "Test", Age = 30 };
            int visitCount = 0;

            // Act
            _service.Transverse(root, obj => 
            {
                visitCount++;
            });

            // Assert
            // 預期只會訪問 root 本身。
            // 雖然 Name 是 string，Age 是 int，但程式碼中已 continue 跳過遞迴。
            visitCount.Should().Be(1); 
        }

        [Fact]
        public void Transverse_ShouldDoNothing_WhenRootIsNull()
        {
            // Arrange
            int visitCount = 0;

            // Act
            _service.Transverse(null, obj => visitCount++);

            // Assert
            visitCount.Should().Be(0);
        }
    }
}