using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace MoqTipsAndTricks
{
    public class BasketManagerTests
    {
        [Fact]
        [Trait("Category", "1")]
        public void AllMockedByDefault()
        {
            var basketMock = new Mock<Basket>();
            var basketManager = new BasketManager(basketMock.Object);

            basketManager.AddProduct(It.IsAny<Product>());

            basketMock.Verify(basket => basket.AddProduct(new Product()));
        }

        //[Fact]
        //[Trait("Category", "1")]
        //public void AllMockedByDefault()
        //{
        //    var basketMock = new Mock<IBasket>();
        //    var basketManager = new BasketManagerWithInterface(basketMock.Object);

        //    basketMock.Setup(basket => basket.AddProduct(new Product()));
        //    basketManager.AddProduct(new Product());

        //    basketMock.Verify(basket => basket.AddProduct(It.IsAny<Product>()));
        //}

        [Fact]
        [Trait("Category", "2")]
        public void dd()
        {
            var basketMock = new Mock<IBasket>();
            var basketManager = new BasketManagerWithInterface(basketMock.Object);

            basketMock.Setup(basket => basket.AddProduct(new Product()));
            basketManager.AddProduct(new Product());

            basketMock.Verify(basket => basket.AddProduct(It.IsAny<Product>()));
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
    }

    public interface IBasket
    {
        void AddProduct(Product product);
    }

    #endregion
}
