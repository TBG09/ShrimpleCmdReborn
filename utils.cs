namespace ShrimpleCmd
{
    public class utils
    {
        public static void print(string msg)
        {
            Console.Write(msg);
        }
        public static void println(string msg)
        {
            Console.WriteLine(msg);
        }
        public static string read()
        {
            return Console.ReadLine();
        }

        // what am i doing here?
        public static string getExceptionTy(Exception ex, Settings settings)
        {
            if (settings.getDebug(settings))
            {
                return ex.ToString();
            }
            else
            {
                return ex.GetType().Name + "\n" + ex.Message;
            }
        }
    }
}