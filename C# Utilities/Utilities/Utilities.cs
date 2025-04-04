namespace Utilities
{
    public class clsUtil
    {
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static void ErrorLogger(Exception ex)
        {
            clsLogger FileLogger = new clsLogger(clsFile.LogToFile);
            FileLogger.Log(clsFormat.ExceptionToString(ex));
        }

        public static bool CreateFolderIfDoesNotExist(string FolderPath)
        {
            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(FolderPath);
                    return true;
                }
                catch (Exception ex)
                {
                    clsUtil.ErrorLogger(ex);
                    return false;
                }
            }
            return true;
        }

    }
}
