using System.Collections.Generic;

namespace Yoloanno.Types
{
    public class Categories
    {
        public static List<Category> CategoriesArray = new List<Category>
        {
            new Category(0, "Plate"),
            new Category(1, "0"),
            new Category(2, "1"),
            new Category(3, "2"),
            new Category(4, "3"),
            new Category(5, "4"),
            new Category(6, "5"),
            new Category(7, "6"),
            new Category(8, "7"),
            new Category(9, "8"),
            new Category(10, "9"),
            new Category(11, "А"),
            new Category(12, "В"),
            new Category(13, "Е"),
            new Category(14, "I"),
            new Category(15, "К"),
            new Category(16, "M"),
            new Category(17, "Н"),
            new Category(18, "О"),
            new Category(19, "Р"),
            new Category(20, "С"),
            new Category(21, "Т"),
            new Category(22, "Х")
        };
    }

    public class Category
    {
        public int Index;
        public string Description;

        public Category(int index, string descr)
        {
            Index = index;
            Description = descr;
        }

        public override string ToString()
        {
            return string.Format("{0} \t({1})", Description, Index);
        }
    }
}
