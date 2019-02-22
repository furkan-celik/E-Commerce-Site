using AlbarakaECommerce.Models;

namespace AlbarakaECommerce.Codes
{
    public class CartProduct
    {
        public Product product;
        public int amount;

        public CartProduct(Product _product, int _amount)
        {
            product = _product;
            amount = _amount;
        }

        public CartProduct(Product _product)
        {
            product = _product;
            amount = 1;
        }
    }
}