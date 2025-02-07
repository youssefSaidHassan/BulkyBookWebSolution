﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Range(1,10000)]
        [Display(Name = "List Price")]
        public double ListPrice { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 1 to 50")]
        public double Price { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 51 to 100")]        
        public double Price50 { get; set; }
        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for more than 100")]
        public double Price100 { get; set; }

        [ValidateNever]
        [Display(Name = "Product Image")]
        public string ImageUrl { get; set; }

        [Display(Name = "Category")]
        public int CategoryId{get; set;}

        [ValidateNever]
        [ForeignKey("CategoryId")]
        public Category Category{get; set;}

        [Display(Name = "Cover Type")]
        public int CoverTypeId { get; set; }

        [ValidateNever]
        [ForeignKey("CoverTypeId")]
        public CoverType CoverType { get; set; }

    }
}
