namespace GXSoftwareUK.UsingHelper.Console.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MyTeam")]
    public class MyTeam
    {
        public virtual int Id { get; set; }

        [Required]
        [StringLength(200)]
        public virtual string Nickname { get; set; }

        public DateTime LastLogin { get; set; }

        [StringLength(200)]
        public virtual string FirstName { get; set; }

        [StringLength(200)]
        public virtual string LastName { get; set; }

    }
}
