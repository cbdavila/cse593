using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommandAndControl.Models
{
    public class FileName
    {
        [Required(ErrorMessage = "You must select a file.")]
        public string filePath { get; set; }
    }
}