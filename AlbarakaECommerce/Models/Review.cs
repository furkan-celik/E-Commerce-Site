//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AlbarakaECommerce.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Review
    {
        public int reviewId { get; set; }
        public Nullable<System.DateTime> reviewDate { get; set; }
        public string reviewContent { get; set; }
        public Nullable<int> customerId { get; set; }
        public Nullable<int> productId { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }
    }
}
