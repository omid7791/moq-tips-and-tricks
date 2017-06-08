using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace MoqTipsAndTricks
{
    public class BasketManagerTests
    {
        private const string InterfacesAndBehaviour = "Interfaces & Behaviour";
        private const string ReturnValues = "Returning Values";
        private const string Arguments = "Arguments";
        private const string Exceptions = "Exceptions";
        private const string GettersAndSetters = "Getters & Setters";
        private const string Events = "Events";

        [Fact]
        [Trait("Category", InterfacesAndBehaviour)]
        public void ShouldThrowAnExceptionIfNonVirtualClassMethodsAndPropertiesUsed()
        {
            var basketMock = new Mock<Basket>();
            var basketManager = new BasketManager(basketMock.Object);

            basketManager.AddProduct(new Product());

            //basketMock.Verify(basket => basket.AddProduct(new Product()));
            Assert.ThrowsAny<Exception>(() => basketMock.Verify(basket => basket.AddProduct(new Product())));
        }

        [Fact]
        [Trait("Category", InterfacesAndBehaviour)]
        public void AllMockedByDefault()
        {
            var basketMock = new Mock<IBasket>();
            var basketManager = new BasketManagerWithInterface(basketMock.Object);
            
            basketManager.AddProduct(new Product());

            basketMock.Verify(basket => basket.AddProduct(It.IsAny<Product>()));
        }

        [Fact]
        [Trait("Category", InterfacesAndBehaviour)]
        public void StrictSetup()
        {
            var basketMock = new Mock<IBasket>(MockBehavior.Strict);
            var basketManager = new BasketManagerWithInterface(basketMock.Object);
            basketMock.Setup(basket => basket.AddProduct(It.IsAny<Product>()));
            //basketMock.Setup(basket => basket.AddProduct(new Product()));

            basketManager.AddProduct(new Product());

            basketMock.Verify(basket => basket.AddProduct(It.IsAny<Product>()));
        }

        [Fact]
        [Trait("Category", ReturnValues)]
        public void ShouldReturnCorrectTotalPrice()
        {
            var basketMock = new Mock<IBasket>();
            var basketManager = new BasketManagerWithInterface(basketMock.Object);
            //basketMock.Setup(basket => basket.GetTotalPrice());
            basketMock.Setup(basket => basket.GetTotalPrice()).Returns(() => 5);
            
            Assert.Equal(7, basketManager.GetTotalPrice());
        }

        [Fact]
        [Trait("Category", ReturnValues)]
        public void ShouldReturnCorrectTotalPriceForIncrementsOfThree()
        {
            var basketMock = new Mock<IBasket>();
            var basketManager = new BasketManagerWithInterface(basketMock.Object);

            var i = 5;
            basketMock.Setup(basket => basket.GetTotalPrice())
                .Returns(() => i).Callback(() => i += 3);

            Assert.Equal(7, basketManager.GetTotalPrice());
            Assert.Equal(10, basketManager.GetTotalPrice());
            Assert.Equal(13, basketManager.GetTotalPrice());
        }

        [Fact]
        [Trait("Category", Arguments)]
        public void ShouldAddProductToBasketWithRightPrice()
        {
            var basketMock = new Mock<IBasket>();
            var basketManager = new BasketManagerWithInterface(basketMock.Object);
            basketManager.AddProduct(new Product { Price = 44m });
            
            basketMock.Verify(basket => basket.AddProduct(It.Is<Product>(product => product.Price == 44m)));
        }

        [Fact]
        [Trait("Category", Exceptions)]
        public void ShouldThrowWhenAddingNullProductToBasket()
        {
            var basketMock = new Mock<IBasket>();
            var basketManager = new Mock<BasketManagerWithInterface>(basketMock.Object);

            basketMock.Setup(basket => basket.AddProduct(It.IsAny<Product>()))
                .Throws<ArgumentNullException>();

            Assert.Throws<AccessViolationException>(() => basketManager.Object.AddProduct(new Product()));
        }

        [Fact]
        [Trait("Category", GettersAndSetters)]
        public void ShouldSetPrice()
        {
            var prod = new Mock<IProduct>();
            prod.Object.Price = 45m;
            
            prod.VerifySet(product => product.Price = It.IsAny<decimal>());
        }

        [Fact]
        [Trait("Category", GettersAndSetters)]
        public void ShouldCallGetPrice()
        {
            var prod = new Mock<IProduct>();
            var basket = new Basket();
            var basketManager = new BasketManagerWithInterface(basket);
            prod.Object.Price = 45m;

            basketManager.AddProduct(prod.Object);
            basketManager.GetTotalPrice();
            
            prod.VerifyGet(product => product.Price, Times.Exactly(1));
        }

        [Fact]
        [Trait("Category", Events)]
        public void ShouldCallProductAddedEventHandler()
        {
            var prod = new Mock<IProduct>();
            prod.Object.Price = 5m;
            var basketMock = new Mock<IBasket>();
            var basketManager = new Mock<BasketManagerWithInterface>(basketMock.Object);

            var dummy = basketManager.Object; //This line is crucial to call/execute the constructor in order to initialize the event handlerS

            basketMock.Raise(basket => basket.ProductAdded += null, null, new ProductAddedEventArgs(prod.Object));
            
            basketManager.Verify(b => b.ProductAdded(It.IsAny<object>(), It.IsAny<ProductAddedEventArgs>()));
        }
    }


    #region "Models"

    public class BasketManager
    {
        private readonly Basket _basket;

        public BasketManager(Basket basket)
        {
            _basket = basket;
        }
        public void AddProduct(Product product)
        {
            _basket.AddProduct(product);
        }
    }

    public interface IProduct
    {
        decimal Price { get; set; }
    }

    public class Product : IProduct
    {
        public decimal Price { get; set; }
    }

    public class Basket : IBasket
    {
        public List<IProduct> Products { get; set; }

        public Basket()
        {
            Products = new List<IProduct>();
        }

        public void AddProduct(IProduct product)
        {
            Products.Add(product);
        }

        public decimal GetTotalPrice()
        {
            return Products.Sum(product => product.Price);
        }

        public event EventHandler<ProductAddedEventArgs> ProductAdded;
    }
    
    public class BasketManagerWithInterface {
        private readonly IBasket _basket;

        public BasketManagerWithInterface(IBasket basket)
        {
            _basket = basket;
            _basket.ProductAdded += ProductAdded;
        }

        public virtual void ProductAdded(object sender, ProductAddedEventArgs e)
        {
        }

        public void AddProduct(IProduct product)
        {
            try
            {
                _basket.AddProduct(product);
            }
            catch (Exception e)
            {
                throw new AccessViolationException(); //any exception will do here
            }
        }

        public decimal GetTotalPrice()
        {
            return _basket.GetTotalPrice() + GetVat();
        }

        private static decimal GetVat()
        {
            return 2.00m;
        }
    }

    public interface IBasket
    {
        void AddProduct(IProduct product);
        decimal GetTotalPrice();
        event EventHandler<ProductAddedEventArgs> ProductAdded;
    }

    public class ProductAddedEventArgs
    {
        private readonly IProduct _product;

        public ProductAddedEventArgs(IProduct product)
        {
            _product = product;
        }
    }

    #endregion
}
