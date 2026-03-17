namespace Practica_2;

static class Program
{

    [STAThread]
    static void Main()
    {

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}