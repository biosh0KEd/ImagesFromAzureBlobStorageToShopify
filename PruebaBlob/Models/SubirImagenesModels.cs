using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PruebaBlob.Models
{
	public class SubirImagenesModels
	{
    }

    public class FileModel
    {
        [Required(ErrorMessage = "Please select file.")]
        [Display(Name = "Browse File")]
        public HttpPostedFileBase[] files { get; set; }

    }


}