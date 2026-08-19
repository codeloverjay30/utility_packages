using Moq;

namespace MoqRepositories
{
    public abstract class MoqRepositoryBase
    {
        public Mock<T> CreateStrictMockObject<T>(T obj)
            where T:class
        {
            return new Mock<T>(MockBehavior.Strict);
        }
        public Mock<T> CreateLooseMockObject<T>(T obj)
            where T:class
        {
            return new Mock<T>(MockBehavior.Loose);
        }
    }
}
