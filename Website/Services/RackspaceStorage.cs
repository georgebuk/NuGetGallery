using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Rackspace.CloudFiles;
using Rackspace.CloudFiles.Domain;
using Rackspace.CloudFiles.Exceptions;

namespace NuGetGallery
{
    public class RackspaceStorage : IFileStorageService
    {
        private readonly UserCredentials _userCredentials = new UserCredentials(GetValue("RackspaceUser"), GetValue("RackspaceToken"));
        private Connection _connection;

        public Connection Connection
        {
            get { return _connection ?? (_connection = new Connection(_userCredentials)); }
        }

        public ActionResult CreateDownloadFileActionResult(string folderName, string fileName)
        {
            var container = Connection.GetContainerInformation(GetValue("ContainerName"));
            return new RedirectResult(Path.Combine(container.CdnUri, fileName), false);
        }

        public void DeleteFile(string folderName, string fileName)
        {
            Connection.DeleteStorageItem(folderName, fileName);
        }

        public Stream GetFile(string folderName, string fileName)
        {
            try
            {
                var file = Connection.GetStorageItem(folderName, fileName);
                file.ObjectStream.Position = 0;
                return file.ObjectStream;
            }
            catch (StorageItemNotFoundException)
            {
                return null;
            }
        }

        public void SaveFile(string folderName, string fileName, Stream packageFile)
        {
            Connection.PutStorageItem(GetValue("ContainerName"), packageFile, fileName);
        }

        private static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}