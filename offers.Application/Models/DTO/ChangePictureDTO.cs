using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Models.DTO
{
    public class ChangePictureDTO
    {
        [Required(ErrorMessage = "photo is required!")]
        public IFormFile Photo { get; set; }
    }
}
