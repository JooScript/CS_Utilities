namespace Utilities
{
    public class clsFile
    {
        private static readonly string LogFilePath;

        static clsFile()
        {
            //LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            LogFilePath = Path.Combine("C:\\Users\\yousef\\Desktop\\", "error.log");
        }

        public static bool RememberUsernameAndPasswordToFile(string Username, string Password, string FileName = "data.txt", string Splitter = "#//#")
        {
            try
            {
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string filePath = currentDirectory + $"\\{FileName}";
                if (Username == "" && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                string dataToSave = Username + Splitter + Password;

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(dataToSave);
                    return true;
                }
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                return false;
            }
        }

        public static bool GetStoredUsernameAndPasswordFromFile(ref string Username, ref string Password, string FileName = "data.txt", string Splitter = "#//#")
        {
            try
            {
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string filePath = currentDirectory + $"\\{FileName}";

                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Console.WriteLine(line);
                            string[] result = line.Split(new string[] { Splitter }, StringSplitOptions.None);
                            Username = result[0];
                            Password = result[1];
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                return false;
            }
        }

        public static void LogToFile(string ErrorMessage)
        {
            try
            {
                File.AppendAllText(LogFilePath, ErrorMessage);
            }
            catch (Exception logEx)
            {
                clsUtil.ErrorLogger(logEx);
            }
        }

        public static async Task LogToFileAsync(string errorMessage)
        {
            try
            {
                await File.AppendAllTextAsync(LogFilePath, errorMessage.ToString());
            }
            catch (Exception logEx)
            {
                clsUtil.ErrorLogger(logEx);
            }
        }

        public static string ReplaceFileNameWithGUID(string sourceFile)
        {
            string fileName = sourceFile;
            FileInfo fi = new FileInfo(fileName);
            string extn = fi.Extension;
            return clsUtil.GenerateGUID() + extn;
        }

        public static bool CopyImageToProjectImagesFolder(string DestinationFolder, ref string sourceFile)
        {
            if (!clsUtil.CreateFolderIfDoesNotExist(DestinationFolder))
            {
                return false;
            }

            string destinationFile = DestinationFolder + ReplaceFileNameWithGUID(sourceFile);
            try
            {
                File.Copy(sourceFile, destinationFile, true);
            }
            catch (IOException iox)
            {
                clsUtil.ErrorLogger(iox);
                return false;
            }

            sourceFile = destinationFile;
            return true;
        }

    }
}
