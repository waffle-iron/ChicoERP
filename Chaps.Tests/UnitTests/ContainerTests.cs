namespace MVVMLib.Tests.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Chaps.Container;
    using System.Threading;

    [TestClass]
    public class ContainerTests
    {
        [TestMethod]
        public void TransientRegistrationIsResolving()
        {
            var container = new Container();

            container.RegisterType<IDependentClass, DependentClass>();

            var result = container.Resolve<IDependentClass>();
            var result2 = container.Resolve<IDependentClass>();
            Assert.IsNotNull(result);

            Assert.IsTrue(result.GetType() == typeof(DependentClass));
            Assert.AreNotEqual(result, result2);
        }

        [TestMethod]
        public void SingeltonRegistrationIsResolving()
        {
            var container = new Container();
            var instance = new DependentClass() { testProperty = "Test!" };
            container.RegisterInstance<IDependentClass>(instance);

            var result = container.Resolve<IDependentClass>();
            Assert.IsNotNull(result);

            Assert.AreEqual(result, instance);
        }

        [TestMethod]
        public void HierarchicalRegistrationIsResolving()
        {
            var container = new Container();
            container.RegisterType<IDependentClass, DependentClass>().LifeTime(LifeTimeMode.Hierarchical);
            container.RegisterType<IDummyClass, DummyClass>().LifeTime(LifeTimeMode.Hierarchical);

            var result = container.Resolve<IDummyClass>() as DummyClass;
            var result2 = container.Resolve<IDependentClass>() as DependentClass;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result2);

            Assert.AreNotEqual(result2, result.dependentClass);
        }

        [TestMethod]
        public void PerThreadRegistrationIsResolving()
        {
            var container = new Container();
            container.RegisterType<IDependentClass, DependentClass>().LifeTime(LifeTimeMode.PerThread);

            
            IDependentClass resultT2 = null;

            Thread T2 = new Thread(new ThreadStart(() =>
            {
                resultT2 = container.Resolve<IDependentClass>();
            }));

            T2.Start();

            IDependentClass resultT1 = container.Resolve<IDependentClass>();
            IDependentClass result2T1 = container.Resolve<IDependentClass>();

            T2.Join();

            Assert.AreEqual(resultT1, result2T1);
            Assert.IsNotNull(resultT2);
            Assert.AreNotEqual(resultT1, resultT2);
        }

        [TestMethod]
        [ExpectedException(typeof(ContainerRegistrationException))]
        public void TypeRegistrationIsRefusingUninheritedTypes()
        {

            var container = new Container();

            container.RegisterType<IDummyClass, DependentClass>();

        }

        [TestMethod]
        [ExpectedException(typeof(ContainerRegistrationException))]
        public void InstanceRegistrationIsRefusingUninheretedTypes()
        {
            var container = new Container();

            container.RegisterInstance<IDummyClass>(new DependentClass());
        }

        [TestMethod]
        public void ContainerIsChoosingRightConstructorFromAttribute()
        {
            var container = new Container();

            container.RegisterType<IAttributeClass, AttributeClass>();

            AttributeClass result = container.Resolve<IAttributeClass>() as AttributeClass;
            Assert.IsNotNull(result);

            Assert.IsTrue(result.correctConstructor);
        }

        [TestMethod]
        public void ContainerIsChoosingRightConstructor()
        {
            var container = new Container();

            container.RegisterType<IConstructorClass, ConstructorClass>();

            ConstructorClass result = container.Resolve<IConstructorClass>() as ConstructorClass;
            Assert.IsNotNull(result);

            Assert.IsTrue(result.correctConstructor);
        }

        [TestMethod]
        public void ContainerIsInjectingDependenciesCorrectly()
        {
            var container = new Container();

            container.RegisterType<IDummyClass, DummyClass>();
            container.RegisterType<IDependentClass, DependentClass>();

            DummyClass result = container.Resolve<IDummyClass>() as DummyClass;
            Assert.IsNotNull(result);

            Assert.IsTrue(result.dependentClass.isInitialized);
        }

        [TestMethod]
        public void ContainerReturnNullWhenNotAbleToResolveDependencie()
        {
            var container = new Container();

            container.RegisterType<IDummyClass, DummyClass>();

            Assert.IsNull(container.Resolve<IDependentClass>());
        }

        [TestMethod]
        [ExpectedException(typeof(ContainerRegistrationException))]
        public void ContainerThrowsExceptionOnDoubleRegistration()
        {
            var container = new Container();

            container.RegisterType<IDependentClass, DependentClass>();
            container.RegisterType<IDependentClass, DependentClass>();
        }


    }

    #region DummyClasses

    class ConstructorClass : IConstructorClass
    {
        public bool correctConstructor;

        public ConstructorClass(IDependentClass dependentClass)
        {
            correctConstructor = true;
        }

        public ConstructorClass()
        {
            correctConstructor = false;
        }


    }


    class AttributeClass : IAttributeClass
    {
        public bool correctConstructor;

        [DIConstructor]
        public AttributeClass()
        {
            correctConstructor = true;
        }

        public AttributeClass(IDependentClass dependentClass)
        {
            correctConstructor = false;
        }
    }



    class DependentClass : IDependentClass
    {
        public string testProperty;
        public bool isInitialized = false;

        public DependentClass()
        {
            isInitialized = true;
        }
    }

    class DummyClass : IDummyClass
    {
        public DependentClass dependentClass;
        public DummyClass(IDependentClass dependentClass)
        {
            this.dependentClass = dependentClass as DependentClass;
        }
    }

    interface IDummyClass
    {

    }

    interface IDependentClass
    {

    }

    interface IAttributeClass
    {

    }

    internal interface IConstructorClass
    {

    }
    #endregion
}

