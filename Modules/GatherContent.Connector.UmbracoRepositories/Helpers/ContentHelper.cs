using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace GatherContent.Connector.UmbracoRepositories.Helpers
{
	public enum ContentType
	{
		document,
		media,
		member
	}
	public class ContentHelper
	{
		public static string GetUdi(Guid key, ContentType contentType)
		{
			return $"umb://{contentType}/{key.ToString().Replace("-", String.Empty)}";
		}

		public static string GetId(IPublishedContent content, string alias)
		{
			string id = String.Empty;
			if (alias == "Umbraco.MultiNodeTreePicker")
			{
				id = content.Id.ToString();
			}
			if (alias == "Umbraco.MultiNodeTreePicker2")
			{
				id = GetUdi(content.GetKey(), ContentType.document);
			}
			return id;
		}
	}
}
