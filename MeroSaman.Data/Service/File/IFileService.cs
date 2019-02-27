using MeroSaman.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeroSaman.Service
{
  public  interface IFileService
    {
        string SavaImage(IFormFile formFile);
    }
}
