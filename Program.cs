internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            DatabaseHelper.InitializeDatabase();
            DatabaseHelper.LoadDataAndUpdate();

        } catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        } catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
