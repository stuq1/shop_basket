using System;
using System.Collections.Generic;

interface IPromo
{

    public String Promocode { get; }

    public void ModifyBasket(Basket basket);

    public IBook ModifyBook(IBook book)
    {
        return book;
    }

}

interface IBook
{

    public String Name { get; }
    public String Author { get; }
    public int Price { get; set; }
    public int Count { get; set; }
    public bool FreeDelivery { get; }

}

class BookPhysical : IBook
{

    public virtual string Name { get; } = "";
    public virtual string Author { get; } = "";
    public virtual int Price { get; set; } = 0;
    public virtual int Count { get; set; } = 1;
    public virtual bool FreeDelivery { get; } = false;

    public BookPhysical(string Name, string Author, int Price, int Count = 1)
    {
        this.Name = Name;
        this.Author = Author;
        this.Price = Price;
        this.Count = Count;
    }

    public BookPhysical(IBook Book, int Count = 1)
    {
        this.Name = Book.Name;
        this.Author = Book.Author;
        this.Price = Book.Price;
        this.Count = Count;
    }

}

class BookDigital : IBook
{
    public virtual string Name { get; } = "";
    public virtual string Author { get; } = "";
    public virtual int Price { get; set; } = 0;
    public virtual int Count { get; set; } = 1;
    public virtual bool FreeDelivery { get; } = true;

    public BookDigital(string Name, string Author, int Price, int Count = 1)
    {
        this.Name = Name;
        this.Author = Author;
        this.Count = Count;
        this.Price = Price;
    }

    public BookDigital(IBook Book, int Count = 1)
    {
        this.Name = Book.Name;
        this.Author = Book.Author;
        this.Price = Book.Price;
        this.Count = Count;
    }

}

class BookWithSale : IBook
{
    public IPromo Promo { get; }
    public IBook Book { get; }

    public string Name { get => Promo.ModifyBook(Book).Name; }
    public string Author { get => Promo.ModifyBook(Book).Author; }
    public int Price { get => Promo.ModifyBook(Book).Price; set {} }
    public int Count { get => Promo.ModifyBook(Book).Count; set {} }
    public bool FreeDelivery { get => Promo.ModifyBook(Book).FreeDelivery; }

    public BookWithSale (IPromo promo, IBook book)
    {
        this.Promo = promo;
        this.Book = book;
    }

}

class Books
{
    public static IBook DeadSouls = new BookPhysical("Dead Souls", "Gogol", 550, 1);
    public static IBook DeadSouls2 = new BookPhysical("Dead Souls 2", "Gogol", 250, 1);
    public static IBook Viy = new BookPhysical("Viy ", "Gogol", 500, 1);
    public static IBook WarAndPeace = new BookPhysical("War And Peace", "Leo Tolstoy", 450, 1);
    public static IBook WarAndPeaceDigital = new BookDigital("War And Peace", "Leo Tolstoy", 450, 1);
}

class Basket
{


    public int DeliveryFreeMin { get; set; } = 1000;
    public int DeliveryPrice { get; set; } = 200;
    public int SalePricePercent { get; set; } = 0;
    public int SalePrice { get; set; } = 0;

    public List<IBook> Books { get; } = new List<IBook>();
    public List<IPromo> Promos { get; } = new List<IPromo>();

    public void AddBook(IBook book)
    {
        foreach (IBook book_ in this.Books)
        {
            if (book_.Name == book.Name && book_.GetType() == book.GetType())
            {
                book_.Count += book.Count;
                return;
            }
        }

        this.Books.Add(book);
    }

    public void AddPromo(IPromo promo)
    {
        foreach (IPromo promo_ in this.Promos)
        {
            if (promo_.Promocode == promo.Promocode)
            {
                return;
            }
        }

        this.Promos.Add(promo);
    }

    public int CalcPrice()
    {

        foreach (IPromo promo in this.Promos)
        {
            promo.ModifyBasket(this);
        }

        bool freeDelivery = true;
        int price = 0;

        foreach (IBook book in this.Books)
        {
            if (!book.FreeDelivery)
            {
                freeDelivery = false;
            }

            price += book.Price * book.Count;
        }

        if (this.SalePricePercent > 100)
            this.SalePricePercent = 100;
        if (this.SalePrice > price)
            this.SalePrice = price;

        price -= this.SalePrice;

        double p = price*((100 - this.SalePricePercent)/100.0d);
        price = (int)Math.Truncate(p);

        // Да, стоимость доставки добавляется уже после применения скидок
        if (!freeDelivery && price < DeliveryFreeMin)
        {
            price += this.DeliveryPrice;
        }

        if (price < 0)
            price = 0;

        return price;
    }

}

class FreeDeliveryPromo : IPromo
{
    string IPromo.Promocode { get => "FREE_DELIVERY"; }

    void IPromo.ModifyBasket(Basket basket)
    {
        basket.DeliveryPrice = 0;
    }

}

class Sale50MoneysPromo : IPromo
{
    string IPromo.Promocode { get => "SALE50MONEYS"; }

    void IPromo.ModifyBasket(Basket basket)
    {
        basket.SalePrice = Math.Max(basket.SalePrice, 50);
    }

}

class FreeDeadSouls2Promo : IPromo
{
    string IPromo.Promocode { get => "DEAD_SOULS_2"; }

    void IPromo.ModifyBasket(Basket basket)
    {

        foreach (IBook book in basket.Books)
        {
            if (book is BookPhysical && book.Name == Books.DeadSouls.Name)
            {
                for (int i = 0; i < basket.Books.Count; i++)
                {
                    if (basket.Books[i] is BookPhysical && basket.Books[i].Name == Books.DeadSouls2.Name)
                    {
                        if (basket.Books[i].Count == 1)
                        {
                            basket.Books[i] = new BookWithSale(this, basket.Books[i]);
                        }
                        else
                        {
                            basket.Books[i].Count -= 1;
                            basket.AddBook(new BookWithSale(this, basket.Books[i]));
                        }
                    }
                }

                return;
            }
        }
    }

    IBook IPromo.ModifyBook(IBook book)
    {
        if (book is BookPhysical && book.Name == Books.DeadSouls2.Name)
        {
            IBook FreeDeadSouls2 = new BookPhysical(Books.DeadSouls2);
            FreeDeadSouls2.Price = 0;
            return FreeDeadSouls2;
        }

        return book;
    }

}

class Sale5PercentPromo : IPromo
{
    string IPromo.Promocode { get => "SALE5"; }

    void IPromo.ModifyBasket(Basket basket)
    {
        basket.SalePricePercent = Math.Max(basket.SalePricePercent, 5);
    }

}

class FreeThirdGogolBook : IPromo
{
    string IPromo.Promocode { get => "FREE_THIRD_GOGOL"; }

    void IPromo.ModifyBasket(Basket basket)
    {
        int authorBooksCount = 0;
        List<KeyValuePair<int, int>> authorBooks = new List<KeyValuePair<int, int>>();

        for (int i = 0; i < basket.Books.Count; i++)
        {
            if (basket.Books[i] is BookPhysical && basket.Books[i].Author == "Gogol" && basket.Books[i].Price > 0)
            {
                authorBooksCount += basket.Books[i].Count;
                authorBooks.Add(new KeyValuePair<int, int>(i, basket.Books[i].Price));
            }
        }

        if (authorBooksCount > 2)
        {
            authorBooks.Sort((pair1, pair2) =>
            {
                if (pair1.Value > pair2.Value)
                    return 1;
                else if (pair1.Value < pair2.Value)
                    return -1;
                else
                    return 0;
            });

            int ind = authorBooks.ToArray()[0].Key;
            freeThirdGogolBook = new BookPhysical(basket.Books[ind], 1);
            freeThirdGogolBook.Price = 0;
            if (basket.Books[ind].Count > 1)
            {
                basket.Books[ind] = new BookWithSale(this, freeThirdGogolBook);
            }
            else
            {
                basket.Books[ind].Count -= 1;
                basket.Books.Add(new BookWithSale(this, freeThirdGogolBook));
            }
        }
    }

    private IBook freeThirdGogolBook;

    public IBook ModifyBook(IBook Book)
    {
        return Book;
    }

}

class Promos
{
    public static IPromo FreeDeliveryPromo = new FreeDeliveryPromo();
    public static IPromo Sale5PercentPromo = new Sale5PercentPromo();
    public static IPromo Sale50MoneysPromo = new Sale50MoneysPromo();
    public static IPromo FreeDeadSouls2Promo = new FreeDeadSouls2Promo();
    public static IPromo FreeThirdGogolBook = new FreeThirdGogolBook();
}

class Program
{

    static void Case1()
    {
        Basket basket = new Basket();

        basket.AddBook(new BookPhysical(Books.DeadSouls));
        basket.AddBook(new BookPhysical(Books.DeadSouls2));
        basket.AddBook(new BookPhysical(Books.Viy));

        Console.WriteLine(basket.CalcPrice());
        basket.AddPromo(Promos.FreeThirdGogolBook);
        Console.WriteLine(basket.CalcPrice());
    }

    static void Case2()
    {
        Basket basket = new Basket();

        basket.AddBook(new BookPhysical(Books.DeadSouls));
        basket.AddBook(new BookPhysical(Books.DeadSouls2));

        Console.WriteLine(basket.CalcPrice());
        basket.AddPromo(Promos.FreeDeadSouls2Promo);
        Console.WriteLine(basket.CalcPrice());
    }

    static void Case3()
    {
        Basket basket = new Basket();

        basket.AddBook(new BookPhysical(Books.DeadSouls));
        basket.AddBook(new BookPhysical(Books.DeadSouls2));

        Console.WriteLine(basket.CalcPrice());
        basket.AddPromo(Promos.FreeDeliveryPromo);
        Console.WriteLine(basket.CalcPrice());
    }

    static void Case4()
    {
        Basket basket = new Basket();

        basket.AddBook(new BookPhysical(Books.WarAndPeace));
        basket.AddBook(new BookDigital(Books.WarAndPeaceDigital));

        Console.WriteLine(basket.CalcPrice());
        basket.AddPromo(Promos.Sale50MoneysPromo);
        basket.AddPromo(Promos.Sale5PercentPromo);
        Console.WriteLine(basket.CalcPrice());
    }

    static void Case5()
    {
        Basket basket = new Basket();

        basket.AddBook(new BookDigital(Books.WarAndPeaceDigital));

        Console.WriteLine(basket.CalcPrice());
        basket.AddPromo(Promos.FreeDeliveryPromo);
        Console.WriteLine(basket.CalcPrice());
    }

    static void Case6()
    {
        Basket basket = new Basket();

        basket.AddBook(new BookPhysical(Books.WarAndPeace));
        basket.AddBook(new BookDigital(Books.WarAndPeaceDigital));

        Console.WriteLine(basket.CalcPrice());
        basket.AddPromo(Promos.FreeDeliveryPromo);
        Console.WriteLine(basket.CalcPrice());
    }

    static void Main(string[] args)
    {
        Case1();
        Console.WriteLine();
        Case2();
        Console.WriteLine();
        Case3();
        Console.WriteLine();
        Case4();
        Console.WriteLine();
        Case5();
        Console.WriteLine();
        Case6();
        Console.WriteLine();
    }

}
