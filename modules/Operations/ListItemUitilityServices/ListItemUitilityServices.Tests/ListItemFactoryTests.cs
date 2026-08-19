using NUnit.Framework;
using Moq;
using ListItemUitilityServices;
using ListItemUitilityServices.Models;
using StringUtilityServices;
using MathUtilityServices;
using System.Collections.Generic;
using System.Linq;

namespace ListItemUitilityServices.Tests
{
    [TestFixture]
    public class ListItemFactoryTests
    {
        private Mock<IStringUtilityService> _mockStringService;
        private Mock<IMathUtilityService> _mockMathService;
        private Mock<IListItemsFactoryBuilder> _mockBuilder;
        private ListItemsFactoriesModel _model;

        [SetUp]
        public void Setup()
        {
            _mockStringService = new Mock<IStringUtilityService>();
            _mockMathService = new Mock<IMathUtilityService>();

            _model = new ListItemsFactoriesModel
            {
                StringUtilityServices = _mockStringService.Object ,
                MathUtilityServices = _mockMathService.Object
            };

            _mockBuilder = new Mock<IListItemsFactoryBuilder>();
            _mockBuilder.Setup(b => b.ListItemsFactoriesModel).Returns(_model);
        }

        [Test]
        public void CreateListItems_WhenTypeIsNumber_ShouldUseMathService()
        {
            // Arrange
            var items = new List<string> { "Apple" , "Banana" };
            var sep = ". ";
            _mockMathService.Setup(s => s.RangeFrom(1 , 3))
                            .Returns(new List<int> { 1 , 2 });

            var factory = new ListItemFactory(_mockBuilder.Object);

            // Act
            var result = factory.CreateListItems(sep , items , ListItemsType.NUMBER);

            // Assert
            Assert.That(result , Is.EqualTo("1. Apple2. Banana"));
            _mockMathService.Verify(s => s.RangeFrom(1 , 3) , Times.Once);
        }

        [Test]
        public void CreateListItems_WhenTypeIsAlphabet_ShouldUseStringService()
        {
            // Arrange
            var items = new List<string> { "Car" , "Bike" };
            var sep = ") ";
            // 邏輯中 A + 2 = C (startPoint 到 endPoint)
            _mockStringService.Setup(s => s.RangeFrom('A' , 'C'))
                              .Returns(new List<char> { 'A' , 'B' });

            var factory = new ListItemFactory(_mockBuilder.Object);

            // Act
            var result = factory.CreateListItems(sep , items , ListItemsType.ALPHABET);

            // Assert
            Assert.That(result , Is.EqualTo("A) CarB) Bike"));
            _mockStringService.Verify(s => s.RangeFrom('A' , 'C') , Times.Once);
        }

        [Test]
        public void Configure_ShouldInvokeBuilderBuild()
        {
            // Arrange
            var factory = new ListItemFactory(_mockBuilder.Object);

            // Act
            factory.Configure();

            // Assert
            _mockBuilder.Verify(b => b.Build() , Times.Once);
        }
    }
}
