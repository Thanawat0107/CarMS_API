using CarMS_API.Services.IServices;
using CarMS_API.Utility;

namespace CarMS_API.Services
{
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUpload(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFile(IFormFile file, string subFolder)
        {
            //สร้างชื่อไฟล์
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid().ToString() + fileExtension;


            //โฟลเดอร์จัดเก็บ
            var folderDirectory = $"{_webHostEnvironment.WebRootPath}{SD.ImgPath}/{subFolder}";


            if (!Directory.Exists(folderDirectory))
            {
                Directory.CreateDirectory(folderDirectory);
            }


            //บันทึกไฟล์ลงดิสก์
            var filePath = Path.Combine(folderDirectory, fileName);
            await using FileStream fs = new FileStream(filePath, FileMode.Create);
            await file.OpenReadStream().CopyToAsync(fs);


            //ชื่อไฟล์สำหรับบันทึกลง Database
            var fullPath = $"{SD.ImgPath}/{subFolder}/{fileName}";
            return fullPath;
        }


        public bool DeleteFile(string filePath)
        {
            if (File.Exists(_webHostEnvironment.WebRootPath + filePath))
            {
                File.Delete(_webHostEnvironment.WebRootPath + filePath);
                return true;
            }
            return false;
        }
    }
}
