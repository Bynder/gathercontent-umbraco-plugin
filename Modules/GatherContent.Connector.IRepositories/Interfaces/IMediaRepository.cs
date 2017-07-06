using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.IRepositories.Interfaces
{
    public interface IMediaRepository<T> : IRepository where T : class
    {
        T UploadFile(string targetPath, File fileInfo);

        string ResolveMediaPath(CmsItem item, T createdItem, CmsField field);
    }
}
