﻿using Momentum.Models;

namespace Momentum.ViewModels.HomeVMs
{
    public class HomeVM
    {
        public IEnumerable<Product> TopSeller { get; set; }
        public IEnumerable<Product> Console { get; set; }
        public IEnumerable<Product> PC { get; set; }
        public IEnumerable<Product> Headphone { get; set; }

        public IEnumerable<Blog> Blog { get; set; }
    }
}
