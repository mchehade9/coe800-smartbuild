//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestingCoe800.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class StoreItem
    {
        public int Id { get; set; }
        public int StoreIDFk { get; set; }
        public string ItemName { get; set; }
        public string Price { get; set; }
    
        public virtual Store Store { get; set; }
    }
}
