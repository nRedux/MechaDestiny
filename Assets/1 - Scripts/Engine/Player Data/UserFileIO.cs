using System.IO;
using UnityEngine;

public static class UserFileIO
{

    public const string USERDATA_FILENAME = "UserData";

    public const string PROJECT_SAVE_EXTENSION = ".bdf";
    
    public const string SUBDIRECTORY_NAME = "UserData";
    
    public const string TESTPREFIX_PLAYERDATA_FILENAME = "TestPlayerData_";
    
    private const string OLD_POSTFIX = "_PREV";



    public static string GetSaveSubdirectoryPath()
    {
        return Application.persistentDataPath + "/" + SUBDIRECTORY_NAME;
    }


    public static void DeleteDataFile( string fileName )
    {
        string path = GetLocalFilePath( fileName );
        if( !File.Exists( path ) )
            return;
        File.Delete( path );
    }


    public static string GetLocalFilePath( string fileName )
    {
        return GetSaveSubdirectoryPath() + "/" + fileName + PROJECT_SAVE_EXTENSION;
    }


    public static string GetBackupFilePath( string fileName )
    {
        return GetSaveSubdirectoryPath() + "/" + fileName + OLD_POSTFIX + PROJECT_SAVE_EXTENSION;
    }



    public static void WriteTextUserDataFile( string json, string fileName )
    {
        /*
        if( BreachwayVersioning.CurrentVersion >= BreachwayVersioning.PLAYER_DATA_VERSION &&
            AnyUserDataExists() )
        {
            MakeBackupCopyOfData();
        }
        */

        //If needed, create the local data file directory
        CreateLocalFileDirectory();

        //Normal local path
        string path = GetLocalFilePath( fileName );
        //Temp file path
        string tempPath = path + ".tmp";

        //Write into temp file path - destroying old file
        using( var fs = new FileStream( tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough ) )
        using( var writer = new StreamWriter( fs ) )
        {
            writer.Write( json );
            writer.Flush();
            writer.Close();
        }

        //MakeBackupCopyOfData();

        //Delete old main data - prep for move
        if( File.Exists( path ) )
            File.Delete( path );
        File.Move( tempPath, path );
    }

    public static string ReadTextUserDataFile( string fileName )
    {
        string path = GetLocalFilePath( fileName );
        if( !File.Exists( path ) )
            return null;

        return File.ReadAllText( path );
    }

    private static void CreateLocalFileDirectory()
    {
        if( Directory.Exists( GetSaveSubdirectoryPath() ) )
            return;
        Directory.CreateDirectory( GetSaveSubdirectoryPath() );
    }
}
