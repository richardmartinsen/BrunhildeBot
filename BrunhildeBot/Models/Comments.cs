﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BrunhildeBot.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Comments
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Comments()
        {
            this.CommentKeyPhrases = new HashSet<CommentKeyPhrases>();
        }
    
        public string RedditId { get; set; }
        public string ParentRedditId { get; set; }
        public Nullable<int> UpVotes { get; set; }
        public Nullable<int> DownVotes { get; set; }
        public Nullable<System.DateTime> CreatedUTC { get; set; }
        public Nullable<double> Sentiment { get; set; }
        public string Language { get; set; }
        public string SubReddit { get; set; }
        public string Comment { get; set; }
        public Nullable<short> Year { get; set; }
        public Nullable<byte> Month { get; set; }
        public Nullable<byte> Daynumber { get; set; }
        public Nullable<byte> Hours { get; set; }
        public Nullable<byte> Minutes { get; set; }
        public Nullable<byte> Seconds { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommentKeyPhrases> CommentKeyPhrases { get; set; }
    }
}
