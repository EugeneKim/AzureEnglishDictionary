using System.ComponentModel.DataAnnotations;

namespace WebDashboard.Models
{
	public class AddWordModel
	{
		#region Properties

		[Required]
		[StringLength(64, ErrorMessage = "Word is too long.")]
		public string? Word { get; set; }

		#endregion
	}
}
