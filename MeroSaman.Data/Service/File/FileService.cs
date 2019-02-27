using MeroSaman.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MeroSaman.Service
{
    public class FileService : IFileService
    {
        private readonly IHostingEnvironment _env;

        public FileService(IHostingEnvironment env)
        {
            _env = env;
        }
        public string SavaImage(IFormFile formFile)
        {
            string rootpath = _env.WebRootPath;
            string folderPath = Path.Combine(rootpath, "Image");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var tempFolderName = Path.GetTempFileName();
            string fileName = formFile.FileName;
            string physicalfilepath = folderPath + "\\" + fileName;
            var relativePath = Path.Combine("Image", fileName);//uploads
            relativePath = relativePath.Replace("\\", "/").Replace(@"\", "/");
            if (!relativePath.StartsWith("/"))
            {
                relativePath = "/" + relativePath;
            }
            if (formFile != null && formFile.Length != 0)
            {
                try
                {
                    CopyStream(formFile.OpenReadStream(), physicalfilepath);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //}
            }
            return "save";
        }

        private void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
      
    }
}
