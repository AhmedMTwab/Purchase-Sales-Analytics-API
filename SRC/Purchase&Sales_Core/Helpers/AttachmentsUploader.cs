using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Purchase_Sales_Core.Helpers;

public static class AttachmentsUploader
{
    public static async Task SaveJobFile(IFormFile uploadedFile , string filePath)
    {
        
                try
                {
                    await using (var fileStream = new FileStream(
                        filePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 81920,
                        useAsync: true))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }
                  
                }
                catch
                {
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    throw;
                }

    }
}
