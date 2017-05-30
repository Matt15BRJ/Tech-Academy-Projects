
using JobBoardMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JobBoardMVC.Controllers
{
	public class FileController : Controller
	{
		private JobBoardMvcContext db = new JobBoardMvcContext();
		//GET: File
		public ActionResult Index(Guid id)
		{
			var fileToRetrieve = db.Resumes.Find(id);
			return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
		}

	}
}
