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
        private readonly string _containerName = GetValue("ContainerName");
        private Connection _connection;

        public Connection Connection
        {
            get { return _connection ?? (_connection = new Connection(_userCredentials)); }
        }

        public ActionResult CreateDownloadFileActionResult(string folderName, string fileName)
        {
            CreateSubContainer(folderName);
            var container = Connection.GetContainerInformation(_containerName);
            return new RedirectResult(Path.Combine(container.CdnUri, folderName + "/" + fileName), false);
        }

        public void DeleteFile(string folderName, string fileName)
        {
            CreateSubContainer(folderName);
            Connection.DeleteStorageItem(_containerName, folderName + "/" + fileName);
        }

        public Stream GetFile(string folderName, string fileName)
        {
            try
            {
                CreateSubContainer(folderName);
                var file = Connection.GetStorageItem(_containerName, folderName + "/" + fileName);
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
            CreateSubContainer(folderName);
            Connection.PutStorageItem(_containerName, packageFile, folderName + "/" + fileName);
        }

        private void CreateSubContainer(string folderName)
        {
            var existing = Connection.GetContainerItemList(_containerName, true);
            if (!existing.Contains(folderName))
            {
                var s = new MemoryStream(0);
                Connection.PutStorageItem(_containerName, s, folderName);
            }
        }

        private static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}