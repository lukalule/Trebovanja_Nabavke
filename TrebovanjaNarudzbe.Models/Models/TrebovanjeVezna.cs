//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrebovanjaNarudzbe.Models.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TrebovanjeVezna
    {
        public int TrebovanjeVeznaId { get; set; }
        public int ArtikalId { get; set; }
        public int TrebovanjeId { get; set; }
        public int StatusArtiklaId { get; set; }
        public int TrebovanaKolicina { get; set; }
        public int KolicinaKojaNedostaje { get; set; }
    
        public virtual RezervisaniArtikli RezervisaniArtikli { get; set; }
        public virtual Status Status { get; set; }
        public virtual Trebovanje Trebovanje { get; set; }
        public virtual vInformacijeOArtiklu vInformacijeOArtiklu { get; set; }
    }
}
