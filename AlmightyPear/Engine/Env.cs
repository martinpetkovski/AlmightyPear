namespace Engine
{
    public class Env
    {
        // global
        public static char PathSeparator = ':';
        public static string TempBinPath = "_TEMP";
        public static string ArchiveBinPath = "_ARCHIVE";
        public static int CharacterLimit = 5000;
        public static int BinPathCharacterLimit = 100;

        // data
        public static IThemeController ThemeManager { get; set; }
        public static UserModel UserData { get; private set; }
        public static BinMetaModel BinData { get; private set; }

        // controller 
        public static BinController BinController { get; private set; }
        public static FirebaseController FirebaseController { get; private set; }

        public static void Initialize()
        {
            UserData = new UserModel();
            BinData = new BinMetaModel();

            //initialize func
            FirebaseController = new FirebaseController();
            FirebaseController.Initialize();
            BinController = new BinController();
        }
    }
}
