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

    public class Product
    {
        public decimal Price { get; set; }
    }

    public class Basket
    {
        public List<Product> Products { get; set; }

        public Basket()
        {
            Products = new List<Product>();
        }

        public void AddProduct(Product product)
        {
            Products.Add(product);
        }
    }
    
    public class BasketManagerWithInterface {
        private readonly IBasket _basket;

        public BasketManagerWithInterface(IBasket basket)
        {
            _basket = basket;
        }
        
        public void AddProduct(Product product)
        {
            _basket.AddProduct(product);
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
        void AddProduct(Product product);
        decimal GetTotalPrice();
    }

    #endregion
}
